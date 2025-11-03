using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace NvimUnity
{
    public static class NeovimPreferences
    {
        private static bool prefsLoaded = false;
        private static string preferredTerminal = "";
        private static Dictionary<string, string> availableTerminals = new Dictionary<string, string>
        {
            ["ghostty"] = "", // Binary
#if UNITY_EDITOR_LINUX
            ["xdg-terminal-exec"] = "",
            ["kitty"] = "",
#else   
            ["xdg-terminal-exec"] = "xdg-terminal-exec -e \"{0}\""
#endif
            // More terminals can be added
        };

        [SettingsProvider]
        public static SettingsProvider CreateNvimUnitySettingsProvider()
        {
            return new SettingsProvider("Preferences/NvimUnity", SettingsScope.User)
            {
                label = "Neovim Settings",
                guiHandler = (searchContext) =>
                {
                    if (!prefsLoaded)
                    {
                        preferredTerminal = EditorPrefs.GetString("NvimUnity_PreferredTerminal", "");
                        prefsLoaded = true;
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Terminal Settings", EditorStyles.boldLabel);

                    // Get available terminals
                    var terminalNames = availableTerminals.Keys;
                    var options = new List<string> { "Autodetect" };
                    options.AddRange(terminalNames);

                    // Get current selection
                    int selectedIndex = Mathf.Max(0, options.IndexOf(
                        string.IsNullOrEmpty(preferredTerminal) ?
                        "Autodetect" : preferredTerminal));

                    // Draw the dropdown
                    selectedIndex = EditorGUILayout.Popup("Preferred Terminal", selectedIndex, options.ToArray());

                    // Save if changed
                    string newSelection = selectedIndex == 0 ? "" : options[selectedIndex];
                    if (newSelection != preferredTerminal)
                    {
                        preferredTerminal = newSelection;
                        EditorPrefs.SetString("NvimUnity_PreferredTerminal", preferredTerminal);
                    }
                },

                keywords = new HashSet<string>(new[] { "Neovim", "Terminal", "Editor" })
            };
        }

        public static string GetPreferredTerminal()
        {
            return EditorPrefs.GetString("NvimUnity_PreferredTerminal", "");
        }

        public static List<string> GetAvailableTerminalNames()
        {
            var terminalNames = new List<string>(availableTerminals.Keys);
            terminalNames.Sort();
            return terminalNames;
        }

        public static Dictionary<string, string> GetAvailableTerminals()
        {
            return availableTerminals;
        }
    }
}
