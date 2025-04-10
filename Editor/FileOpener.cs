using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

using Debug = UnityEngine.Debug;

namespace NvimUnity
{
    public static class FileOpener
    {
        private static readonly string socket = @"\\.\pipe\nvim-unity";

        public static bool OpenFile(string filePath, int line)
        {
            if (line < 1) line = 1;

            try
            {
                string normalizedPath = Utils.NormalizePath(filePath);
                bool isRunnigInNeovim = SocketChecker.IsSocketActive(socket);
                string args = Utils.BuildLauncherCommand(
                        filePath, 
                        line, 
                        NeovimEditor.Terminal, 
                        socket, 
                        NeovimEditor.RootFolder, 
                        isRunnigInNeovim);

                var psi = new ProcessStartInfo
                {
                    FileName = NeovimEditor.App,
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

