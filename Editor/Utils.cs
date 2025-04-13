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

        //-------------- Launcher --------------

        public static string GetNeovimPath()
        {
#if UNITY_EDITOR_WIN
                return @"C:\Program Files\Neovim\bin\nvim.exe"; // Windows
#else
                return "/usr/bin/nvim"; // Linux/macOS
#endif
        }

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
}

