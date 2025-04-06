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
        UnityEditor.SyncVS.SyncSolution();
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
            UnityEditor.SyncVS.SyncSolution();
        }

        string vscodePath = Path.Combine(rootPath, ".vscode");
        if (!Directory.Exists(vscodePath))
        {
            Directory.CreateDirectory(vscodePath);
            UnityEngine.Debug.Log(".vscode folder created.");
        }
    }
}

