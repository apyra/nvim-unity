using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NvimUnity
{
    [Serializable]
    public class Config
    {
        public string last_project = "";
        public void MergeMissingDefaults(Config defaults) { }
    }

    public static class ConfigManager
    {
        private static readonly string ConfigFileName = "config.json";

        public static string GetConfigPath()
        {
#if UNITY_EDITOR_WIN
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NvimUnity");
#elif UNITY_EDITOR_OSX
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/NvimUnity");
#else
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".config/NvimUnity");
#endif
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return Path.Combine(folder, ConfigFileName);
        }

        public static Config LoadConfig()
        {
            string path = GetConfigPath();
            var defaultConfig = new Config();

            if (!File.Exists(path))
            {
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            try
            {
                string json = File.ReadAllText(path);
                var loaded = JsonUtility.FromJson<Config>(json) ?? new Config();

                loaded.MergeMissingDefaults(defaultConfig);
                SaveConfig(loaded); // Updates with defaults if necessary
                return loaded;
            }
            catch (Exception e)
            {
                Debug.LogError($"[NvimUnity] Failed to load config: {e.Message}");
                return defaultConfig;
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
                Debug.LogError($"[NvimUnity] Failed to save config: {e.Message}");
            }
        }
    }
}

