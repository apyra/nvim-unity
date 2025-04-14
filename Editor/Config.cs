using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NvimUnity
{
    [Serializable]
    public class TerminalEntry
    {
        public string platform;
        public string command;
    }

    [Serializable]
    public class Config
    {
        public bool use_custom_terminal = false;

        public List<TerminalEntry> terminals = new()
        {
            new TerminalEntry { platform = "Windows", command = "wt" },
            new TerminalEntry { platform = "Linux", command = "gnome-terminal" },
            new TerminalEntry { platform = "OSX", command = "iTerm" }
        };

        public string GetTerminalForPlatform(string platform)
        {
            var match = terminals.Find(t => t.platform == platform);
            return match != null ? match.command : "";
        }

        public void SetTerminalForPlatform(string platform, string command)
        {
            var existing = terminals.Find(t => t.platform == platform);
            if (existing != null)
                existing.command = command;
            else
                terminals.Add(new TerminalEntry { platform = platform, command = command });
        }

        public string GetResolvedTerminalForPlatform(string platform)
        {
            var match = terminals.Find(t => t.platform == platform);
            if (match != null && !string.IsNullOrEmpty(match.command))
                return match.command;

            // fallback seguro para cada plataforma
            return platform switch
            {
                "Windows" => "cmd",
                "Linux"   => "x-terminal-emulator", // Debian/Ubuntu compatível
                "OSX"     => "open -a Terminal",    // abre o Terminal.app
                _         => "cmd" // fallback geral
            };
        }

        public void MergeMissingDefaults(Config defaults)
        {
            if (terminals == null || terminals.Count == 0)
            {
                terminals = defaults.terminals;
                return;
            }

            foreach (var defaultEntry in defaults.terminals)
            {
                var existing = terminals.Find(t => t.platform == defaultEntry.platform);
                if (existing == null)
                    terminals.Add(defaultEntry);
            }
        }
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
                SaveConfig(loaded); // atualiza com defaults se necessário
                return loaded;
            }
            catch (Exception e)
            {
                Debug.LogError($"[nvim-unity] Failed to load config: {e.Message}");
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
                Debug.LogError($"[nvim-unity] Failed to save config: {e.Message}");
            }
        }
    }
}

