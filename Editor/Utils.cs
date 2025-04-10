using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NvimUnity
{
    public static class Utils
    {
        //-------------- Project Methods --------------

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

        //-------------- Launcher App --------------

        public static string GetLauncherPath()
        {
            string scriptPath = NormalizePath(Path.GetFullPath("Packages/com.apyra.nvim-unity/Launcher/nvim-open"));

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

        public static string BuildLauncherCommand(string filePath, int line, string terminal, string socket, string root, bool isOpen)
        {
            return $"\"{filePath}\" {line} \"{terminal}\" \"{socket}\" \"{root}\" {(isOpen ? "true" : "false")}";
        }
        
        //-------------- Others --------------

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

        public static string FindProjectRoot()
        {
            var current = Directory.GetCurrentDirectory();

            while (!string.IsNullOrEmpty(current))
            {
                bool hasAssets = Directory.Exists(Path.Combine(current, "Assets"));
                bool hasLibrary = Directory.Exists(Path.Combine(current, "Library"));
                bool hasPackages = Directory.Exists(Path.Combine(current, "Packages"));

                if (hasAssets && hasLibrary && hasPackages)
                    return current;

                current = Directory.GetParent(current)?.FullName;
            }

            return null;
        }

        public static string NormalizePath(string path)
        {
#if UNITY_EDITOR_WIN
            return path.Replace("/", "\\");
#else
            return path.Replace("\\", "/");
#endif
        }

        //-------------- Config --------------

        public static string GetConfigPath()
        {
            string basePath;

            if (GetCurrentOS() == "Windows")
                basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "nvim-unity");
            else
                basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "nvim-unity");

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            return Path.Combine(basePath, "config.json");
        }

        public static Config GetConfig()
        {
            string path = GetConfigPath();

            if (!File.Exists(path))
            {
                // Criar e salvar template padrão
                var defaultConfig = new Config
                {
                    terminals = new Dictionary<string, string>
                    {
                        { "Windows", "wt" },
                        { "Linux", "gnome-terminal" },
                        { "OSX", "iTerm" }
                    }
                };

                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Config>(json);
        }

        public static void SaveConfig(Config config)
        {
            string path = GetConfigPath();
            string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }

    public class Config
    {
        public Dictionary<string, string> terminals { get; set; }
    }
}

