using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public static class NvimUnityEditor
{
    private static string GetNvimLauncherPath()
    {
        string basePath = "Packages/nvim-unity/bin/";
#if UNITY_EDITOR_WIN
        return Path.Combine(basePath, "nvim-open.bat");
#else
        return Path.Combine(basePath, "nvim-open.sh");
#endif
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
                Debug.LogError("nvim launcher script not found: " + nvimPath);
            }
        }
        return false;
    }

    [MenuItem("Assets/NvimUnity/Regenerate Project Files")]
    public static void RegenerateProjectFiles()
    {
        AssetDatabase.Refresh();
        UnityEditor.SyncVS.SyncSolution();
        Debug.Log("Project files regenerated.");
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
            Debug.Log("Generating missing project files...");
            UnityEditor.SyncVS.SyncSolution();
        }

        string vscodePath = Path.Combine(rootPath, ".vscode");
        if (!Directory.Exists(vscodePath))
        {
            Directory.CreateDirectory(vscodePath);
            Debug.Log(".vscode folder created.");
        }
    }
}

