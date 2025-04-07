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
                string filePathWithLine = reader.ReadToEnd().Trim();

                if (!string.IsNullOrEmpty(filePathWithLine))
                {
                    Debug.Log("[NvimUnity] Requested to open: " + filePathWithLine);
                    TryOpenInNvim(filePathWithLine);
                }
                else
                {
                    Debug.LogWarning("[NvimUnity] Received empty path");
                }
            }


            string response = "OK";
            byte[] buffer = Encoding.UTF8.GetBytes(response);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        private static void TryOpenInNvim(string filePathWithLine)
        {
            string[] parts = filePathWithLine.Split('+');
            string fullPath = Path.GetFullPath(parts[0]);
            string lineArg = parts.Length > 1 ? "+" + parts[1] : "";

            Debug.Log("[NvimUnity] Full path: " + fullPath);
            if (!string.IsNullOrEmpty(lineArg)) Debug.Log("[NvimUnity] Line arg: " + lineArg);

            string rootDir = Utils.FindProjectRoot(fullPath);
            if (string.IsNullOrEmpty(rootDir))
            {
                Debug.LogWarning("[NvimUnity] Could not determine project root for: " + fullPath);
                return;
            }

            string socketPath = Path.Combine(rootDir, ".nvim_socket");
            Debug.Log("[NvimUnity] Socket path: " + socketPath);

            if (File.Exists(socketPath))
            {
                Debug.Log("[NvimUnity] Reusing existing Neovim instance");
                RunDetachedTerminal(GetTerminalCommand($"{GetNvimCommand(socketPath, fullPath)} {lineArg}"));
            }
            else
            {
                Debug.Log("[NvimUnity] Launching new Neovim instance");
                RunDetachedTerminal(GetTerminalCommand($"{GetNvimListenCommand(socketPath, fullPath)} {lineArg}"));
            }
        }


        private static string GetNvimCommand(string socket, string filePath)
        {
            return $"nvim --server \"{socket}\" --remote-tab \"{filePath}\"";
        }

        private static string GetNvimListenCommand(string socket, string filePath)
        {
            return $"nvim --listen \"{socket}\" \"{filePath}\"";
        }

        private static string GetTerminalCommand(string cmd)
        {
#if UNITY_EDITOR_WIN
            return GetTerminalFor("Windows").Replace("{cmd}", cmd);
#elif UNITY_EDITOR_OSX
            return GetTerminalFor("OSX").Replace("{cmd}", cmd);
#else
            return GetTerminalFor("Linux").Replace("{cmd}", cmd);
#endif
        }

        private static string GetTerminalFor(string os)
        {
            if (_terminalByOS.TryGetValue(os, out var template))
            {
                return template;
            }

#if UNITY_EDITOR_WIN
            return $"wt -w 0 nt -d . cmd /k {{cmd}}";
#elif UNITY_EDITOR_OSX
            return $"osascript -e 'tell app \"Terminal\" to do script \"{{cmd}}\"'";
#else
            return $"x-terminal-emulator -e bash -c '{{cmd}}'";
#endif
        }

        private static Process RunDetachedTerminal(string command)
        {
#if UNITY_EDITOR_WIN
            Debug.Log("[NvimUnity] Launching detached terminal (Windows): " + command);
            return Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c start \"\" {command}",
                CreateNoWindow = true,
                UseShellExecute = false
            });
#elif UNITY_EDITOR_OSX
            Debug.Log("[NvimUnity] Launching detached terminal (macOS): " + command);
            return Process.Start(new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command} &\"",
                CreateNoWindow = true,
                UseShellExecute = false
            });
#else // Linux
            Debug.Log("[NvimUnity] Launching detached terminal (Linux): " + command);
            return Process.Start(new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command} &\"",
                CreateNoWindow = true,
                UseShellExecute = false
            });
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

