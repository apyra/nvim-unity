using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NvimUnity
{
    public static class Utils
    {
        public static string GetSocketPath()
        {
            try
            {
                string scriptDir = Path.GetDirectoryName(GetLauncherPath());
                string configPath = Path.Combine(scriptDir, "config.json");

                if (!File.Exists(configPath))
                {
                    Debug.LogWarning($"[NvimUnity] Config file not found: {configPath}");
                    return null;
                }

                string json = File.ReadAllText(configPath);

                // Simples regex para extrair o valor de "socket": "algum_path"
                Match match = Regex.Match(json, @"""socket""\s*:\s*""([^""]+)""");
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim();
                }
                else
                {
                    Debug.LogWarning("[NvimUnity] 'socket' not found in config.json");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[NvimUnity] Error reading socket from config.json: {e.Message}");
            }

            return null;
        }

        public static void RegenerateProjectFiles()
        {
            var editorAssembly = typeof(UnityEditor.Editor).Assembly;
            var syncVS = editorAssembly.GetType("UnityEditor.SyncVS");

            if (syncVS != null)
            {
                var syncMethod = syncVS.GetMethod("SyncSolution", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (syncMethod != null)
                {
                    syncMethod.Invoke(null, null);
                    AssetDatabase.Refresh();
                    CleanExtraCsprojFiles();
                }
            }
        }

        private static void CleanExtraCsprojFiles()
        {
            string root = Directory.GetCurrentDirectory();
            string[] allCsproj = Directory.GetFiles(root, "*.csproj", SearchOption.TopDirectoryOnly);

            var keep = new[]
            {
                "Assembly-CSharp.csproj",
                $"{new DirectoryInfo(root).Name}.csproj"
            };

            foreach (string file in allCsproj)
            {
                if (!keep.Contains(Path.GetFileName(file)))
                {
                    File.Delete(file);
                }
            }
        }

        public static string GetLauncherPath()
        {
            string scriptPath = Path.GetFullPath(Utils.NormalizePath("Packages/com.apyra.nvim-unity/Launcher/nvim-open"));

#if UNITY_EDITOR_WIN
            return scriptPath + ".bat";
#else
            return scriptPath + ".sh";
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

        public static string BuildLauncherCommand(string filePath, int line, string socket, string root, bool isOpen)
        {
            return $"\"{filePath}\" {line} \"{socket}\" \"{root}\" {isOpen.ToString().ToLower()}";
        }

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

        public static string FindProjectRoot(string path)
        {
            var dir = new DirectoryInfo(Path.GetDirectoryName(path));
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Assets")))
            {
                dir = dir.Parent;
            }
            return NormalizePath(dir?.FullName ?? Path.GetDirectoryName(path));
        }

        public static string NormalizePath(string path)
        {
#if UNITY_EDITOR_WIN
            return path.Replace("/", "\\");
#else
            return path.Replace("\\", "/");
#endif
        }
    }
}

