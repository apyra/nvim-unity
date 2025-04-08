using System;
using System.Collections.Generic;
using UnityEngine;

namespace NvimUnity
{
    public static class TerminalCommandBuilder
    {
        public static List<string> GetCommands(Dictionary<string, string> terminalByOS, string os, string cmd)
        {
            List<string> terminals = new();

            if (terminalByOS.TryGetValue(os, out var raw))
            {
                try
                {
                    if (raw.TrimStart().StartsWith("["))
                    {
                        string json = "{\"templates\":" + raw + "}";
                        var parsed = JsonUtility.FromJson<ListWrapper>(json);
                        foreach (var tmpl in parsed.templates)
                            terminals.Add(tmpl.Replace("{cmd}", cmd));
                    }
                    else
                    {
                        terminals.Add(raw.Replace("{cmd}", cmd));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[NvimUnity] Failed to parse terminals for {os}: {e.Message}");
                }
            }

            if (terminals.Count == 0)
            {
#if UNITY_EDITOR_WIN
                terminals.Add($"wt -w 0 nt -d . cmd /c {cmd}");
#elif UNITY_EDITOR_OSX
                terminals.Add($"osascript -e 'tell app \"Terminal\" to do script \"{cmd}\"'");
#else
                terminals.Add($"x-terminal-emulator -e bash -c '{cmd}'");
#endif
            }

            return terminals;
        }

        [Serializable]
        private class ListWrapper { public List<string> templates; }
    }
}

