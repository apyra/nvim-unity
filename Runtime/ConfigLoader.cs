using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace NvimUnity
{
    public static class ConfigLoader
    {
        public static Dictionary<string, string> LoadTerminalConfig()
        {
            var config = new Dictionary<string, string>();
            string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Packages/com.apyra.nvim-unity/Launcher/config.json");

            if (!File.Exists(path)) return config;

            try
            {
                var json = File.ReadAllText(path);
                var parsed = JsonUtility.FromJson<Wrapper>("{\"terminals\":" + json + "}");
                return parsed.terminals ?? config;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[NvimUnity] Failed to load config.json: " + e.Message);
                return config;
            }
        }

        [Serializable]
        private class Wrapper { public Dictionary<string, string> terminals; }
    }
}

