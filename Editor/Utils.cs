using System;
using System.Reflection;
using UnityEditor;
using Debug = UnityEngine.Debug;

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
}

