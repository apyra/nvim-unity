using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class SyncHelper
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
                Debug.Log("Project files regenerated via reflection.");
                CleanExtraCsprojFiles();
            }
            else
            {
                Debug.LogError("Failed to find SyncSolution method.");
            }
        }
        else
        {
            Debug.LogError("Failed to find UnityEditor.SyncVS class.");
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
                Debug.Log($"[NvimUnity] Deleting extra .csproj: {Path.GetFileName(file)}");
                File.Delete(file);
            }
        }
    }
}

