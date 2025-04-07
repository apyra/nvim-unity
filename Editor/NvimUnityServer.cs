/ NvimUnityServer.cs
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System.Net.Http;

namespace NvimUnity
{
    [InitializeOnLoad]
    public static class NvimUnityServer
    {
        private static HttpListener _listener;
        private static Thread _listenerThread;
        private static bool _isRunning = false;
        private static Dictionary<string, string> _terminalByOS = new();

        static NvimUnityServer()
        {
            Debug.Log("[NvimUnity] Initializing server...");
            LoadConfig();
            StartServer();
        }

        private static void LoadConfig()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string configPath = Path.GetFullPath(Path.Combine(projectRoot, "Packages/com.apyra.nvim-unity/Launcher/config.json"));

            Debug.Log("[NvimUnity] Loading config from: " + configPath);

            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    var parsed = JsonUtility.FromJson<Wrapper>(json);
                    if (parsed.terminals != null)
                        _terminalByOS = parsed.terminals;
                    Debug.Log("[NvimUnity] Loaded terminal configuration");
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[NvimUnity] Failed to parse config.json: " + e.Message);
                }
            }
            else
            {
                Debug.LogWarning("[NvimUnity] Config file not found");
            }
        }

        [Serializable]
        private class Wrapper
        {
            public Dictionary<string, string> terminals;
        }

        [Serializable]
        private class ListWrapper
        {
            public List<string> templates;
        }

        [Serializable]
        private class TerminalArrayWrapper
        {
            public List<string> templates;
        }

        public static void StartServer()
        {
            if (_isRunning) return;

            _isRunning = true;
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:5005/");

            _listenerThread = new Thread(() =>
            {
                try
                {
                    _listener.Start();
                    Debug.Log("[NvimUnity] HTTP Server started on port 5005");
                }
                catch (Exception e)
                {
                    Debug.LogError("[NvimUnity] Failed to start HTTP Server: " + e.Message);
                    return;
                }

                while (_isRunning)
                {
                    try
                    {
                        var context = _listener.GetContext();
                        HandleRequest(context);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("[NvimUnity] Exception in listener thread: " + ex.Message);
                    }
                }
            });

            _listenerThread.Start();
        }

        private static void HandleRequest(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath;
            Debug.Log("[NvimUnity] Incoming request: " + path);

            if (path == "/regenerate")
            {
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                UnityEditorInternal.InternalEditorUtility.RequestScriptReload();
                Utils.RegenerateProjectFiles();
                Debug.Log("[NvimUnity] Regeneration triggered");
            }
            else if (path == "/open")
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string payload = reader.ReadToEnd().Trim();
                string[] parts = payload.Split(':');

                if (parts.Length >= 1)
                {
                    string filePath = parts[0];
                    int line = parts.Length >= 2 && int.TryParse(parts[1], out var l) ? l : 1;

                    Debug.Log("[NvimUnity] Requested to open file: " + filePath + ":" + line);
                    TryOpenInNvim(filePath, line);
                }
                else
                {
                    Debug.LogWarning("[NvimUnity] Received malformed open request");
                }
            }
            else if (path == "/status")
            {
                Debug.Log("[NvimUnity] Status check received");
            }


            string response = "OK";
            byte[] buffer = Encoding.UTF8.GetBytes(response);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        public static void TryOpenInNvim(string filePath, int line)
        {
            string fullPath = Path.GetFullPath(filePath);
            string rootDir = Utils.FindProjectRoot(fullPath);

            if (string.IsNullOrEmpty(rootDir))
            {
                Debug.LogWarning("[NvimUnity] Could not determine project root for: " + fullPath);
                TryOpenStandalone(fullPath, line);
                return;
            }

            string socketPath = Path.Combine(rootDir, ".nvim_socket");
            Debug.Log("[NvimUnity] Socket path: " + socketPath);

            string cmd = File.Exists(socketPath)
                ? GetNvimCommand(socketPath, fullPath, line)
                : GetNvimListenCommand(socketPath, fullPath, line);

            RunDetachedTerminalFallbacks(cmd);
        }

        public static bool TryOpenStandalone(string filePath, int line)
        {
            string cmd = $"nvim \"{filePath}\" +{line}";
            try
            {
                RunDetachedTerminalFallbacks(cmd);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[NvimUnity] Fallback failed: " + ex.Message);
                return false;
            }
        }

        private static string GetNvimCommand(string socket, string filePath, int line)
        {
            return $"nvim --server \"{socket}\" --remote-tab +{line} \"{filePath}\"";
        }

        private static string GetNvimListenCommand(string socket, string filePath, int line)
        {
            return $"nvim --listen \"{socket}\" +{line} \"{filePath}\"";
        }

        private static void RunDetachedTerminalFallbacks(string cmd)
        {
            string os = GetCurrentOS();
            List<string> fallbackTerminals = GetTerminalsForOS(os, cmd);

            foreach (string terminalCmd in fallbackTerminals)
            {
                try
                {
                    Debug.Log($"[NvimUnity] Trying terminal: {terminalCmd}");
                    Process.Start(new ProcessStartInfo
                    {
#if UNITY_EDITOR_WIN
                        FileName = "cmd.exe",
                        Arguments = $"/c start \"\" {terminalCmd}",
#else
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{terminalCmd} &\"",
#endif
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[NvimUnity] Failed to launch terminal: {terminalCmd}, Error: {ex.Message}");
                }
            }

            throw new Exception("[NvimUnity] All terminal attempts failed.");
        }

        private static List<string> GetTerminalsForOS(string os, string cmd)
        {
            List<string> terminals = new();

            if (_terminalByOS.TryGetValue(os, out var raw))
            {
                if (raw.TrimStart().StartsWith("["))
                {
                    try
                    {
                        var wrapper = new TerminalArrayWrapper();
                        wrapper.templates = JsonUtility.FromJson<ListWrapper>("{\"templates\":" + raw + "}").templates;
                        foreach (var tmpl in wrapper.templates)
                            terminals.Add(tmpl.Replace("{cmd}", cmd));
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[NvimUnity] Failed to parse array of terminals for {os}: {e.Message}");
                    }
                }
                else
                {
                    terminals.Add(raw.Replace("{cmd}", cmd));
                }
            }

            if (terminals.Count == 0)
            {
#if UNITY_EDITOR_WIN
                terminals.Add($"wt -w 0 nt -d . cmd /k {cmd}");
#elif UNITY_EDITOR_OSX
                terminals.Add($"osascript -e 'tell app \"Terminal\" to do script \"{cmd}\"'");
#else
                terminals.Add($"x-terminal-emulator -e bash -c '{cmd}'");
#endif
            }

            return terminals;
        }

        private static string GetCurrentOS()
        {
#if UNITY_EDITOR_WIN
            return "Windows";
#elif UNITY_EDITOR_OSX
            return "OSX";
#else
            return "Linux";
#endif
        }

        public static void StopServer()
        {
            Debug.Log("[NvimUnity] Stopping server...");
            _isRunning = false;
            _listener?.Stop();
            _listenerThread?.Abort();
        }
    }
}

