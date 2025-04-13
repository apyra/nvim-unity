using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NvimUnity
{
    [Serializable]
    public class Config
    {
        public bool use_custom_terminal = true;
        public Dictionary<string, string> terminals = new()
        {
            { "Windows", "wt" },
            { "Linux", "gnome-terminal" },
            { "OSX", "iTerm" }
        };
    }

    public static class ConfigManager
    {
        private static readonly string ConfigFileName = "config.json";

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

            return Path.Combine(folder, ConfigFileName);
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

