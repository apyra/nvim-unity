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
            return NormalizePath(Path.GetDirectoryName(Application.dataPath));
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
#if UNITY_EDITOR_WIN
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "nvim-unity");
#elif UNITY_EDITOR_OSX
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/nvim-unity");
#else
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".config/nvim-unity");
#endif
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return Path.Combine(folder, "config.json");
        }

        public static Config LoadConfig()
        {
            string path = GetConfigPath();

            if (!File.Exists(path))
            {
                var defaultConfig = new Config();
                SaveConfig(defaultConfig); // salva o template inicial
                return defaultConfig;
            }

            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<Config>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[nvim-unity] Failed to load config: {e.Message}");
                return new Config(); // fallback
            }
        }

        public static void SaveConfig(Config config)
        {
            try
            {
                string json = JsonUtility.ToJson(config, true);
                File.WriteAllText(GetConfigPath(), json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[nvim-unity] Failed to save config: {e.Message}");
            }
        }    
    }

    [Serializable]
    public class Config
    {
        public Dictionary<string, string> terminals = new()
        {
            { "Windows", "wt" },
            { "Linux", "gnome-terminal" },
            { "OSX", "iTerm" }
        };
    }
}

