using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NvimUnity
{
    public static class Utils
    {
        public static string GetCurrentOS()
        {
#if UNITY_EDITOR_WIN
            return "Windows";
#elif UNITY_EDITOR_OSX
            return "OSX";
#else
            return "Linux";
#endif
        }

        public static string GetUnityInstallRoot()
        {
            string editorExe = EditorApplication.applicationPath;
            string editorDir = Path.GetDirectoryName(editorExe);
            string root = Path.GetDirectoryName(editorDir); // sobe da pasta Editor/
            return root;
        }

        public static string GetProjectRoot()
        {
            return Path.GetDirectoryName(Application.dataPath);
        }

        public static string NormalizePath(string path)
        {
#if UNITY_EDITOR_WIN
            return path.Replace("/", "\\");
#else
            return path.Replace("\\", "/");
#endif
        }

        public static bool IsInAssetsFolder(string path)
        {
            return path.Replace('\\', '/').Contains("Assets/");
        }

        //-------------- Launcher --------------

        public static string GetNeovimPath()
        {
#if UNITY_EDITOR_WIN
            string path = @"C:\Program Files\Neovim\bin\nvim.exe";
            if (File.Exists(path))
                return path;
            return "nvim";
#else
            string[] possiblePaths = new[]
            {
                "/usr/bin/nvim",
                "/usr/local/bin/nvim", // comum em Intel macOS e Linux
                "/opt/homebrew/bin/nvim", // Apple Silicon (M1/M2)
                "/snap/bin/nvim" // Linux com Snap
            };

            foreach (var p in possiblePaths)
            {
                if (File.Exists(p))
                    return p;
            }

            return "nvim"; // fallback para PATH
#endif
        }


        public static string GetLauncherPath()
        {
            string launcherPath = Environment.GetEnvironmentVariable("NVIMUNITY_PATH");

            if (string.IsNullOrEmpty(launcherPath))
            {
#if UNITY_EDITOR_WIN
                // Fallbacks para Windows
                string[] fallbackPaths = new[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "NvimUnity", "NvimUnity.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NvimUnity", "NvimUnity.exe")
                };

                return fallbackPaths.FirstOrDefault(File.Exists);
#else
                // Fallbacks para Linux/macOS
                string[] fallbackPaths = new[]
                {
                    "/usr/bin/nvimunity",
                    "/usr/local/bin/nvimunity",
                    "/opt/nvimunity/nvimunity.sh",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/bin/nvimunity"),
                    "/Applications/NvimUnity.app/Contents/MacOS/nvimunity"
                };

                return fallbackPaths.FirstOrDefault(File.Exists);
#endif
            }

#if UNITY_EDITOR_WIN
            return Path.Combine(launcherPath, "NvimUnity.exe");
#else
            return launcherPath;
#endif
        }



        public static void EnsureLauncherExecutable()
        {
#if !UNITY_EDITOR_WIN
            try
            {
                string path = GetLauncherPath();
                if (File.Exists(path))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "/bin/chmod",
                        Arguments = $"+x \"{path}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[NvimUnity] Failed to chmod launcher: " + e.Message);
            }
#endif
        }

    }
}

