using System;
using System.Collections.Generic;
using UnityEngine;

namespace NvimUnity
{
    public static class TerminalCommandBuilder
    {
        public static List<string> GetCommands(Dictionary<string, List<string>> config, string os, string cmd)
        {
            var commands = new List<string>();
            if (config.ContainsKey(os))
            {
                foreach (var terminal in config[os])
                {
                    commands.Add($"{terminal} \"{cmd}\"");
                }
            }
            return commands;
        }

    }
}

