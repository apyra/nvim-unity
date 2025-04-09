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
        public static readonly string LauncherPath = Utils.GetLauncherPath();
        private static readonly string socket = Utils.GetSocketPath();
        public static bool projectOpenInNeovim = false;

        public static bool OpenFile(string filePath, int line)
        {
            if (line < 1) line = 1;

            try
            {
                string normalizedPath = Utils.NormalizePath(filePath);

                if (!projectOpenInNeovim)
                {
                    string root = Utils.FindProjectRoot(filePath);
                    return OpenFileViaLauncher(normalizedPath, line, socket, root);
                    firstTimeOpenning = false;
                }
                else
                {
                    return OpenInSocketInstance(normalizedPath,line);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NvimUnity] Error opening file: {ex}");
                return false;
            }
        }
        
        private static bool OpenInSocketInstance(string filePath, int line)
        {
            try
            {
                string command = $"nvim --server \"{socket}\" --remote-send \":e {filePath}<CR>{line}G\"";

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

        private static bool OpenFileViaLauncher(string filePath, int line, string root)
        {
            try
            {
                string args = Utils.BuildLauncherCommand(filePath, line, socket, root);

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
    }
}

