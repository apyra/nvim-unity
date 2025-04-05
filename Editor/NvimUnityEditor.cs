using System;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public static class NvimUnityEditor
{
    private static string GetNvimLauncherPath()
    {
        // Arquivo dispatcher sem extensão
        string launcher = "nvim-launch"; 
        string basePath = "Packages/com.apyra.nvim-unity/Launch/";
        return Path.GetFullPath(Path.Combine(basePath, launcher));
    }

    [OnOpenAsset(0)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        string assetPath = AssetDatabase.GetAssetPath(instanceID);
        if (assetPath.EndsWith(".cs"))
        {
            string fullPath = Path.GetFullPath(assetPath);
            EnsureProjectFiles();

            string nvimPath = GetNvimLauncherPath();
            if (File.Exists(nvimPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = nvimPath,
                    Arguments = $"\"{fullPath}\"",
                    UseShellExecute = true
                });
                return true;
            }
            else
            {
                UnityEngine.Debug.LogError("nvim launcher script not found: " + nvimPath);
            }
        }
        return false;
    }

    [MenuItem("Assets/NvimUnity/Regenerate Project Files")]
    public static void RegenerateProjectFiles()
    {
        AssetDatabase.Refresh();
        SyncHelper.RegenerateProjectFiles();
        UnityEngine.Debug.Log("Project files regenerated.");
    }

    private static void EnsureProjectFiles()
    {
        string rootPath = Directory.GetCurrentDirectory();
        string projectName = new DirectoryInfo(rootPath).Name;
        string slnPath = Path.Combine(rootPath, $"{projectName}.sln");

        bool hasCsproj = Directory.GetFiles(rootPath, "*.csproj", SearchOption.TopDirectoryOnly).Length > 0;
        bool hasSln = File.Exists(slnPath);

        if (!hasCsproj || !hasSln)
        {
            UnityEngine.Debug.Log("Generating missing project files...");
            SyncHelper.RegenerateProjectFiles();
        }

        string vscodePath = Path.Combine(rootPath, ".vscode");
        if (!Directory.Exists(vscodePath))
        {
            Directory.CreateDirectory(vscodePath);
            UnityEngine.Debug.Log(".vscode folder created.");
        }
    }
}

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
                UnityEngine.Debug.Log("Project files regenerated via reflection.");
            }
            else
            {
                UnityEngine.Debug.LogError("Failed to find SyncSolution method.");
            }
        }
        else
        {
            UnityEngine.Debug.LogError("Failed to find UnityEditor.SyncVS class.");
        }
    }
}

