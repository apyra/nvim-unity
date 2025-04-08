using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace NvimUnity
{
    public static class FileOpener
    {
        public static bool OpenFile(string filePath, int line, string serverAddress)
        {
            try
            {
                string normalizedPath = Utils.NormalizePath(filePath);
                string cmd = Utils.BuildLauncherCommand(normalizedPath, line, serverAddress);
                string os = Utils.GetCurrentOS();

                var config = ConfigLoader.LoadTerminalConfig();
                Dictionary<string, List<string>> terminals = config["terminals"] as Dictionary<string, List<string>>;
                List<string> terminalCommands = TerminalCommandBuilder.GetCommands(terminals, os, cmd);

                foreach (var terminal in terminalCommands)
                {
                    if (TryStartDetachedTerminal(terminal))
                        return true;
                }

                Debug.LogWarning("[NvimUnity] Failed to open file in any terminal.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[NvimUnity] Error opening file: {e.Message}");
            }

            return false;
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

