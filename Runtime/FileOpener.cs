using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace NvimUnity
{
    public static class FileOpener
    {
        public static void OpenFile(string filePath, int line, string serverAddress, Dictionary<string, string> terminalConfig)
        {
            try
            {
                string normalizedPath = Utils.NormalizePath(filePath);
                string cmd = Utils.BuildLauncherCommand(normalizedPath, line, serverAddress);
                string os = Utils.GetCurrentOS();
                List<string> terminals = TerminalCommandBuilder.GetCommands(terminalConfig, os, cmd);

                foreach (var terminal in terminals)
                {
                    if (TryStartDetachedTerminal(terminal))
                        return;
                }

                Debug.LogWarning("[NvimUnity] Failed to open file in any terminal.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[NvimUnity] Error opening file: {e.Message}");
            }
        }

        private static bool TryStartDetachedTerminal(string command)
        {
            try
            {
#if UNITY_EDITOR_WIN
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c start \"\" {command}",
                    UseShellExecute = true,
                    CreateNoWindow = true
                });
#else
                Process.Start(new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
#endif
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[NvimUnity] Failed to start terminal: {e.Message}");
                return false;
            }
        }
    }
}

