using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

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
            string root = Path.GetDirectoryName(editorDir); // Up in editor folder
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
                "/usr/local/bin/nvim", // Usual in Intel macOS e Linux
                "/opt/homebrew/bin/nvim", // Apple Silicon (M1/M2)
                "/snap/bin/nvim", // Linux Snap
                "/run/current-system/sw/bin/nvimunity",
                Path.Combine("/etc/profiles/per-user", Environment.UserName, "bin/nvimunity"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nix-profile/bin/nvimunity"),
            };

            foreach (var p in possiblePaths)
            {
                if (File.Exists(p))
                    return p;
            }

            return "nvim"; // PATH fallback
#endif
        }

        public static string GetLauncherPath()
        {
            string launcherPath = Environment.GetEnvironmentVariable("NVIMUNITY_PATH");

            if (string.IsNullOrEmpty(launcherPath))
            {
#if UNITY_EDITOR_WIN
                // Windows fallbacks
                string[] fallbackPaths = new[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "NvimUnity", "NvimUnity.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "NvimUnity", "NvimUnity.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NvimUnity", "NvimUnity.exe")
                };

                return fallbackPaths.FirstOrDefault(File.Exists);
#else
                // Linux/macOS fallbacks
                string[] fallbackPaths = new[]
                {
                    "/usr/bin/nvimunity",
                    "/usr/local/bin/nvimunity",
                    "/opt/nvimunity/nvimunity.sh",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "NvimUnity.AppImage"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/bin/nvimunity"),
                    "/Applications/NvimUnity.app/Contents/MacOS/nvimunity",
                    "/run/current-system/sw/bin/nvimunity",
                    Path.Combine("/etc/profiles/per-user", Environment.UserName, "bin/nvimunity"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nix-profile/bin/nvimunity"),
                };

                var path = fallbackPaths.FirstOrDefault(File.Exists);

                return path;
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
                    Process.Start(new ProcessStartInfo
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
                UnityEngine.Debug.LogWarning("[NvimUnity] Failed to chmod launcher: " + e.Message);
            }
#endif
        }

        public static ProcessStartInfo BuildProcessStartInfo(string defaultApp, string path, int line)
        {
#if UNITY_EDITOR_WIN
            return null;
#else
            string preferredTerminal = NeovimPreferences.GetPreferredTerminal();
            UnityEngine.Debug.Log($"[NvimUnity] preferredTerminal: {preferredTerminal}");

            string fileName = null;
            string args = null;

            Dictionary<string, string> terminals = NeovimPreferences.GetAvailableTerminals();

            if (terminals.TryGetValue(preferredTerminal, out var cmdPrefix) && ExistsOnPath(preferredTerminal))
            {
                fileName = preferredTerminal;
                args = $"{cmdPrefix} {defaultApp} \"{path}\" +{line}";
            } 
            else
            {
                foreach (var term in terminals)
                {
                    if (!ExistsOnPath(term.Key))
                        continue;

                    fileName = term.Key;
                    args = $"{term.Value} {defaultApp} \"{path}\" +{line}";
                    break;
                }
            }

            if (fileName == null || args == null) {
                UnityEngine.Debug.LogError("[NvimUnity] failed to find terminal");
                return null;
            }

            return new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                UseShellExecute = true,
                CreateNoWindow = false
            };
#endif
        }

        public static bool ExistsOnPath(string fileName)
        {
            return GetFullPath(fileName) != null;
        }

        public static string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var values = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in values.Split(Path.PathSeparator))
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }
    }
}

