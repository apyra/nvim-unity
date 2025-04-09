using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NvimUnity
{
    public static class FileOpener
    {
        public static readonly string LauncherPath = Utils.GetLauncherPath();
        private static readonly string socket = Utils.GetSocketPath();
        
        public static bool OpenFile(string filePath, int line)
        {
            if (line < 1) line = 1;

            try
            {
                string normalizedPath = Utils.NormalizePath(filePath);
                string root = Utils.FindProjectRoot(filePath);
                bool isRunniginNeovim = SocketChecker.IsSocketActive(socket);
                string args = Utils.BuildLauncherCommand(filePath, line, socket, root, isRunniginNeovim);

                var psi = new ProcessStartInfo
                {
                    FileName = LauncherPath,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}

