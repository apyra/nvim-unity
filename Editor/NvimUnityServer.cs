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
            LoadConfig();
            StartServer();
        }

        private static void LoadConfig()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string configPath = Path.GetFullPath(Path.Combine(projectRoot, "Packages/com.apyra.nvim-unity/Launcher/config.json"));

            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    var parsed = JsonUtility.FromJson<Wrapper>(json);
                    if (parsed.terminals != null)
                        _terminalByOS = parsed.terminals;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[NvimUnity] Failed to parse config.json: " + e.Message);
                }
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
                _listener.Start();

                while (_isRunning)
                {
                    try
                    {
                        var context = _listener.GetContext();
                        HandleRequest(context);
                    }
                    catch { }
                }
            });

            _listenerThread.Start();
        }

        private static void HandleRequest(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath;

            if (path == "/regenerate")
            {
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                UnityEditorInternal.InternalEditorUtility.RequestScriptReload();
                Utils.RegenerateProjectFiles();
            }
            else if (path == "/open")
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string filePath = reader.ReadToEnd().Trim();

                if (!string.IsNullOrEmpty(filePath))
                {
                    TryOpenInNvim(filePath);
                }
            }

            string response = "OK";
            byte[] buffer = Encoding.UTF8.GetBytes(response);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        private static void TryOpenInNvim(string filePath)
        {
            string fullPath = Path.GetFullPath(filePath);
            string rootDir = Utils.FindProjectRoot(fullPath);
            if (string.IsNullOrEmpty(rootDir)) return;

            string socketPath = Path.Combine(rootDir, ".nvim_socket");

            if (File.Exists(socketPath))
            {
                // Socket exists, connect via --server
                RunDetachedTerminal(GetTerminalCommand($"{GetNvimCommand(socketPath, fullPath)}"));
            }
            else
            {
                // Socket doesn't exist, start new terminal with Neovim
                RunDetachedTerminal(GetTerminalCommand($"{GetNvimListenCommand(socketPath, fullPath)}"));
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
            return Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c start \"\" {command}",
                CreateNoWindow = true,
                UseShellExecute = false
            });
#elif UNITY_EDITOR_OSX
            return Process.Start(new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command} &\"",
                CreateNoWindow = true,
                UseShellExecute = false
            });
#else // Linux
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
            _isRunning = false;
            _listener?.Stop();
            _listenerThread?.Abort();
        }
    }
}

