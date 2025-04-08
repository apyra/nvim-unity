using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NvimUnity
{
    public static class FileOpener
    {
        private static readonly string LauncherPath = Utils.GetLauncherPath();
        private static readonly string Socket = Utils.GetSocketPath();

        public static bool OpenFile(string filePath, int line)
        {
            if (line < 1) line = 1;

            try
            {
                string normalizedPath = Utils.NormalizePath(filePath);

                if (IsServerRunning(NvimUnityServer.ServerAddress))
                {
                    return OpenInRunningServer(normalizedPath, line);
                }
                else if (!string.IsNullOrEmpty(Socket) && File.Exists(Socket))
                {
                    return OpenInSocketInstance(normalizedPath, line);
                }
                else
                {
                    string root = Utils.FindProjectRoot(filePath);
                    return OpenFileViaLauncher(normalizedPath, line, NvimUnityServer.ServerAddress, root);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NvimUnity] Error opening file: {ex}");
                return false;
            }
        }

        private static bool OpenInRunningServer(string filePath, int line)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent($"{filePath}:{line}", Encoding.UTF8, "text/plain");
                    var response = client.PostAsync(NvimUnityServer.ServerAddress + "open", content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        Debug.Log($"[NvimUnity] Opened in running server: {filePath}:{line}");
                        return true;
                    }

                    Debug.LogWarning($"[NvimUnity] Server responded with: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NvimUnity] Failed to send to server: {ex.Message}");
            }

            return false;
        }

        private static bool OpenInSocketInstance(string filePath, int line)
        {
            try
            {
                string command = $"nvim --server \"{Socket}\" --remote-send \":e {filePath}<CR>{line}G\"";

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(psi);
                Debug.Log($"[NvimUnity] Sent to running Neovim via socket: {filePath}:{line}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NvimUnity] Could not send via socket: {ex.Message}");
                return false;
            }
        }

        private static bool OpenFileViaLauncher(string filePath, int line, string server, string root)
        {
            try
            {
                string args = Utils.BuildLauncherCommand(filePath, line, server, root);

                var psi = new ProcessStartInfo
                {
                    FileName = LauncherPath,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(psi);
                Debug.Log($"[NvimUnity] Opened via launcher: {filePath}:{line}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NvimUnity] Could not start launcher: {ex.Message}");
                return false;
            }
        }

        private bool IsServerRunning(string serverUrl)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMilliseconds(800);
                    var result = client.GetAsync(serverUrl + "status").Result;
                    return result.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}

