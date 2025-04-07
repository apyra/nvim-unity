using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;
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

