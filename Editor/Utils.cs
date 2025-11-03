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
                "/snap/bin/nvim" // Linux Snap
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
#if !UNITY_EDITOR_WIN
            string preferredTerminal = NeovimPreferences.GetPreferredTerminal();

            string fileName = "/usr/bin/env";
            string args = "echo 'No terminal emulator found!'; sleep 1";

            Dictionary<string, string> terminals = NeovimPreferences.GetAvailableTerminals();

            if (terminals.TryGetValue(preferredTerminal, out var cmdFormat) &&
                IsTerminalAvailable(preferredTerminal))
#if UNITY_EDITOR_LINUX
            {
                fileName = preferredTerminal;

                if (cmdFormat == "")
                {
                    args = $"{defaultApp} {path} {line}";
                }
                else
                {
                    args = string.Format(cmdFormat, $"{defaultApp} {path} {line}");
                }
            }

            else
            {
                foreach (var t in terminals)
                {
                    if (IsTerminalAvailable(t.Key))
                    {
                        fileName = t.Key;
                        args = $"{defaultApp} {path} {line}";
                        break;
                    }
                }
            }
#else
            {
                if (cmdFormat == "")
                {
                    fileName = preferredTerminal;
                    args = $"-e {defaultApp} {path} {line}";
                }
                else
                {
                    args = string.Format(cmdFormat, $"{defaultApp} {path} {line}");
                }
            }

            else
            {
                foreach (var t in terminals)
                {
                    if (IsTerminalAvailable(t.Key))
                    {
                        args = string.Format(t.Value, $"{defaultApp} {path} {line}");
                    }
                }
            }
#endif

            return new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                UseShellExecute = true,
                CreateNoWindow = false
            };
#else
            return null;
#endif
        }

        public static bool IsTerminalAvailable(string terminalName)
        {
#if !UNITY_EDITOR_WIN
            UnityEngine.Debug.Log($"[NvimUnity] Checking if terminal is available: {terminalName}");
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = terminalName,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                UnityEngine.Debug.Log($"[NvimUnity] Executing: {psi.FileName} {psi.Arguments}");
                using (var process = Process.Start(psi))
                {
                    process.WaitForExit(500);
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
#else
            return true;
#endif
        }
    }
}

