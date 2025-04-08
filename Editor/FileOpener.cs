using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NvimUnity
{
    public static class FileOpener
    {
        public static readonly string launcher = Utils.GetLauncherPath();

        public static bool OpenFile(string filePath, int line)
        {
            if(line < 1) line = 1;

            try
            {
                string normalizedPath = Utils.NormalizePath(filePath);
                string args = Utils.BuildLauncherCommand(normalizedPath, line, NvimUnityServer.ServerAddress);
           
                if (TryStartDetachedTerminal(args))
                    return true;
                
                Debug.LogWarning("[NvimUnity] Failed to open file in any terminal.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[NvimUnity] Error opening file: {e.Message}");
            }

            return false;
        }

        private static bool TryStartDetachedTerminal(string args)
        {
            try
            {

/*#if UNITY_EDITOR_WIN
/*
/*                psi.Arguments = $"/c \"\" {args}";*/
/*#else*/
/*                psi.Arguments = $"-c \"{args}\"";*/
/*#endif*/
                var psi = new ProcessStartInfo();
                psi.FileName = launcher;
                psi.Arguments = args;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                Process.Start(psi);

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

