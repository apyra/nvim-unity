using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using Debug = UnityEngine.Debug;

namespace NvimUnity
{
    public static class Utils
    {
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
                    UnityEditor.AssetDatabase.Refresh();
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
                if (!keep.Any(k => Path.GetFileName(file).Equals(k)))
                {
                    File.Delete(file);
                }
            }
        }

        public static string GetLauncherPath()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string scriptPath = Path.Combine(projectRoot, "Packages/com.apyra.nvim-unity/Launcher/nvim-open");

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
                    Process.Start(new ProcessStartInfo
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

        public static string BuildLauncherCommand(string filePath, int line, string serverAddress)
        {
            string launcherPath = Path.GetFullPath(NormalizePath(GetLauncherPath()));
            return $"\"{launcherPath}\" \"{filePath}\" {line} \"{serverAddress.TrimEnd('/')}\"";
        }

        public static List<string> GetTerminalsForOS(Dictionary<string, string> terminalByOS, string os, string cmd)
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

        [Serializable]
        public class ListWrapper
        {
            public List<string> templates;
        }

        public static string FindProjectRoot(string path)
        {
            var dir = new DirectoryInfo(Path.GetDirectoryName(path));
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Assets")))
            {
                dir = dir.Parent;
            }
            return dir?.FullName;
        }

        public static string NormalizePath(string path)
        {
#if UNITY_EDITOR_WIN
            return path.Replace("/", "\\");
#else
            return path.Replace("\\", "/");
#endif
        }

    }
}

