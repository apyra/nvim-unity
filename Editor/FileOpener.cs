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
        
        private static bool OpenFile(string filePath, int line)
        {
            if (line < 1) line = 1;

            try
            {
                string normalizedPath = Utils.NormalizePath(filePath);
                string root = Utils.FindProjectRoot(filePath);
                string args = Utils.BuildLauncherCommand(filePath, line, socket, root, projectOpenInNeovim);

                var psi = new ProcessStartInfo
                {
                    FileName = LauncherPath,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(psi);
                Debug.Log($"[NvimUnity] Opened via launcher: {filePath}:{line}");
                projectOpenInNeovim = true;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NvimUnity] Could not start launcher: {ex.Message}");
                projectOpenInNeovim = false;
                return false;
            }
        }
    }
}

