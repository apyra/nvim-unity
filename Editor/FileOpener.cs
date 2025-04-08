using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using UnityEngine;

namespace NvimUnity
{
    public static class FileOpener
    {
        private static readonly string LauncherPath = Utils.GetLauncherPath();
        private static readonly string Socket = Utils.GetSocketPath();

        public static bool OpenFileViaLauncher(string filePath, int line)
        {
            if (line < 1) line = 1;

            try
            {
                string normalizedPath = Utils.NormalizePath(filePath);
                string root = Utils.FindProjectRoot(filePath);
                string args = Utils.BuildLauncherCommand(normalizedPath, line, NvimUnityServer.ServerAddress, root);

                if (TryStartDetachedTerminal(args))
                {
                    Debug.Log($"[NvimUnity] Opened file via launcher: {filePath}:{line}");
                    return true;
                }
                else
                {
                    Debug.LogWarning("[NvimUnity] Failed to open file via launcher.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[NvimUnity] Exception while opening file: {e}");
            }

            return false;
        }

        public static bool OpenFileInRunningNeovim(string filePath, int line)
        {
            try
            {
                string normalizedPath = Utils.NormalizePath(filePath);
                if (line < 1) line = 1;

                // Usa nvim --server <socket> --remote
                string command = $"nvim --server \"{Socket}\" --remote-send \":e {normalizedPath}<CR>{line}G\"";

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(psi);
                Debug.Log($"[NvimUnity] Sent file to running Neovim instance: {filePath}:{line}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NvimUnity] Could not send file to running Neovim: {ex.Message}");
                return false;
            }
        }

        private static bool TryStartDetachedTerminal(string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = LauncherPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(psi);
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

