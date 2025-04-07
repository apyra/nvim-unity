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
        // --- Fields ---
        private static HttpListener _listener;
        private static Thread _listenerThread;
        private static bool _isRunning = false;
        private static bool _initialized = false;

        private static Dictionary<string, string> _terminalByOS = new();
        private static string _status = "Stopped";
        private static string _serverAddress = "http://localhost:5005/";

        public static string ServerAddress
        {
            get => _serverAddress;
            set
            {
                if (!_isRunning) _serverAddress = value;
                else Debug.LogWarning("[NvimUnity] Cannot change server address while server is running.");
            }
        }

        public static string GetStatus() => _status;

        // --- Initialization ---
        static NvimUnityServer() => InitOnce();

        private static void InitOnce()
        {
            if (_initialized) return;
            _initialized = true;

            Debug.Log("[NvimUnity] Initializing server...");
            StopServer();
            LoadConfig();
            StartServer();
        }

        private static void LoadConfig()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string configPath = Path.Combine(projectRoot, "Packages/com.apyra.nvim-unity/Launcher/config.json");

            Debug.Log("[NvimUnity] Loading config from: " + configPath);

            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    var parsed = JsonUtility.FromJson<Wrapper>("{\"terminals\":" + json + "}");
                    if (parsed.terminals != null)
                        _terminalByOS = parsed.terminals;

                    Debug.Log("[NvimUnity] Loaded terminal configuration.");
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[NvimUnity] Failed to parse config.json: " + e.Message);
                }
            }
            else
            {
                Debug.LogWarning("[NvimUnity] Config file not found.");
            }
        }

        [Serializable]
        private class Wrapper { public Dictionary<string, string> terminals; }

        // --- Server Control ---
        public static void StartServer()
        {
            if (_isRunning) return;

            _isRunning = true;
            _status = "Starting...";
            _listener = new HttpListener();
            _listener.Prefixes.Add(ServerAddress);

            _listenerThread = new Thread(() =>
            {
                try
                {
                    _listener.Start();
                    _status = "Running";
                    Debug.Log("[NvimUnity] HTTP Server started on: " + ServerAddress);
                }
                catch (Exception e)
                {
                    _status = "Error: " + e.Message;
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
                        Debug.LogWarning("[NvimUnity] Listener thread exception: " + ex.Message);
                    }
                }
            });

            _listenerThread.Start();
        }

        public static void StopServer()
        {
            Debug.Log("[NvimUnity] Stopping server...");
            _isRunning = false;
            _status = "Stopped";
            _listener?.Stop();
            _listenerThread?.Abort();
        }

        // --- Request Handler ---
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
                HandleOpenRequest(context);
            }

            string response = "OK";
            byte[] buffer = Encoding.UTF8.GetBytes(response);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        private static void HandleOpenRequest(HttpListenerContext context)
        {
            using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
            string payload = reader.ReadToEnd().Trim();
            string[] parts = payload.Split(':');

            if (parts.Length >= 1)
            {
                string filePath = parts[0];
                int line = parts.Length >= 2 && int.TryParse(parts[1], out var l) ? l : 1;

                Debug.Log("[NvimUnity] Open file requested: " + filePath + ":" + line);
                TryOpenInNvim(filePath, line);
            }
            else
            {
                Debug.LogWarning("[NvimUnity] Malformed /open request.");
            }
        }

        // --- Public File Opening API ---
        public static bool OpenFile(string filePath, int line)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            if (line < 1) line = 1;

            string fullPath = Path.GetFullPath(filePath).Replace("\\", "/");

            if (GetStatus() == "Running")
            {
                string data = $"{fullPath}:{line}";

                try
                {
                    using var client = new HttpClient();
                    var content = new StringContent(data, Encoding.UTF8, "text/plain");
                    var result = client.PostAsync(ServerAddress.TrimEnd('/') + "/open", content).Result;

                    if (!result.IsSuccessStatusCode)
                    {
                        Debug.LogWarning($"[NvimUnity] Server responded with error: {result.StatusCode}");
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError("[NvimUnity] Failed to reach /open endpoint: " + ex.Message);
                }
            }

            // fallback if server isn't running
            Debug.Log("[NvimUnity] Server not running, opening directly.");
            return TryOpenStandalone(fullPath, line);
        }

        public static void TryOpenInNvim(string filePath, int line)
        {
            string fullPath = Path.GetFullPath(filePath);
            string rootDir = Utils.FindProjectRoot(fullPath);

            if (string.IsNullOrEmpty(rootDir))
            {
                Debug.LogWarning("[NvimUnity] Could not find project root for: " + fullPath);
                TryOpenStandalone(fullPath, line);
                return;
            }

            string launcherCmd = BuildLauncherCommand(fullPath, line);
            RunDetachedTerminalFallbacks(launcherCmd);
        }

        public static bool TryOpenStandalone(string filePath, int line)
        {
            string launcherCmd = BuildLauncherCommand(filePath, line);

            try
            {
                RunDetachedTerminalFallbacks(launcherCmd);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[NvimUnity] Fallback terminal open failed: " + ex.Message);
                return false;
            }
        }

        // --- Helpers ---
        private static string BuildLauncherCommand(string filePath, int line)
        {
            return $"nvim-open.bat \"{filePath}\" {line} \"{ServerAddress.TrimEnd('/')}\"";
        }

        private static void RunDetachedTerminalFallbacks(string cmd)
        {
            string os = GetCurrentOS();
            List<string> fallbackTerminals = Utils.GetTerminalsForOS(_terminalByOS, os, cmd);

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
                    Debug.LogWarning($"[NvimUnity] Failed to launch: {terminalCmd}, Error: {ex.Message}");
                }
            }

            throw new Exception("[NvimUnity] All terminal fallback attempts failed.");
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
    }
}

