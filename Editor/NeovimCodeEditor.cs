using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Unity.CodeEditor;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public class NeovimCodeEditor : IExternalCodeEditor
{
    private static readonly string editorName = "Neovim (NvimUnity)";

    static NeovimCodeEditor()
    {
        CodeEditor.Register(new NeovimCodeEditor());
        //EnsureProjectFiles();
    }

    public string GetDisplayName() => editorName;

    public bool OpenProject(string path, int line, int column)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("[NvimUnity] OpenProject received empty path!");
            return false;
        }

        return OpenFileAtLine(path, line);
    }

    public bool OpenFileAtLine(string filePath, int line)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("[NvimUnity] filePath is null or empty!");
            return false;
        }

        if (line < 1) line = 1;

        string fullPath = Path.GetFullPath(filePath);
        string launchScript = NormalizePath(GetLauncherPath());

        Debug.Log($"[NvimUnity] Opening file: {fullPath} at line {line}");
        Debug.Log($"[NvimUnity] Launching script: {launchScript}");
        Debug.Log($"[NvimUnity] File exists? {File.Exists(launchScript)}");

        if (!File.Exists(launchScript))
        {
            Debug.LogError($"[NvimUnity] Launch script not found: {launchScript}");
            return false;
        }

        try
        {
            EnsureProjectFiles();

	    Debug.Log($"[NvimUnity] Running command: cmd.exe /c \"\"{launchScript}\" \"{fullPath}\" +{line}\"");


            Process.Start(new ProcessStartInfo
{
    FileName = "cmd.exe",
    Arguments = $"/c \"\"{launchScript}\" \"{fullPath}\" +{line}\"",
    CreateNoWindow = true,
    UseShellExecute = false
});

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[NvimUnity] Failed to launch Neovim: " + e.Message);
            return false;
        }
    }

    public void OnGUI() { }

    public void Initialize(string editorInstallationPath) { }

    public CodeEditor.Installation[] Installations => new[]
    {
        new CodeEditor.Installation
        {
            Name = editorName,
            Path = GetLauncherPath()
        }
    };

    public void SyncAll()
    {
        AssetDatabase.Refresh();
        SyncHelper.RegenerateProjectFiles();
    }

    public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
    {
        // Not implemented
    }

    public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
    {
        var installations = Installations;

        if (!installations.Any())
        {
            installation = new CodeEditor.Installation
            {
                Name = editorName,
                Path = GetLauncherPath()
            };
        }
        else
        {
            try
            {
                installation = installations.First(inst => inst.Path == editorPath);
            }
            catch (InvalidOperationException)
            {
                installation = new CodeEditor.Installation
                {
                    Name = editorName,
                    Path = editorPath
                };
            }
        }

        return true;
    }

    private static string GetLauncherPath()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string scriptPath = Path.Combine(projectRoot, "Packages/com.apyra.nvim-unity/Launch/nvim-open");

#if UNITY_EDITOR_WIN
        return scriptPath + ".bat";
#else
        return scriptPath + ".sh";
#endif
    }

    private static string NormalizePath(string path)
    {
#if UNITY_EDITOR_WIN
        return path.Replace("/", "\\");
#else
        return path.Replace("\\", "/");
#endif
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
            Debug.Log("[NvimUnity] Generating missing project files...");
            SyncHelper.RegenerateProjectFiles();
        }

        string vscodePath = Path.Combine(rootPath, ".vscode");
        if (!Directory.Exists(vscodePath))
        {
            Directory.CreateDirectory(vscodePath);
            Debug.Log("[NvimUnity] .vscode folder created.");
        }
    }
}

