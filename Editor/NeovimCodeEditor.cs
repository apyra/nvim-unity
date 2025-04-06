using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
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
        if (!IsNvimUnityDefaultEditor())
        {
            Debug.LogWarning("[NvimUnity] Skipped OpenFileAtLine: Neovim is not set as the default external script editor.");
            return false;
        }

        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("[NvimUnity] filePath is null or empty!");
            return false;
        }

        if (line < 1) line = 1;

        string fullPath = Path.GetFullPath(filePath);
        string launchScript = NormalizePath(GetLauncherPath());

        Debug.Log($"[NvimUnity] Opening file: {fullPath} at line {line}");
        Debug.Log($"[NvimUnity] Using launch script: {launchScript}");

        if (!File.Exists(launchScript))
        {
            Debug.LogError($"[NvimUnity] Launch script not found: {launchScript}");
            return false;
        }

        try
        {
            string command = $"\"{launchScript}\" \"{fullPath}\" +{line}";
            RunShellCommand(command);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[NvimUnity] Failed to launch Neovim: " + e.Message);
            return false;
        }
    }

    private void RunShellCommand(string command)
    {
        var psi = new ProcessStartInfo();

#if UNITY_EDITOR_WIN
        psi.FileName = "cmd.exe";
        psi.Arguments = $"/k {command}";
#else
        psi.FileName = "/bin/bash";
        psi.Arguments = $"-c \"{command}\"";
#endif

        psi.UseShellExecute = false;
        psi.CreateNoWindow = false;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;

        using (var process = new Process { StartInfo = psi })
        {
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrEmpty(output))
                Debug.Log("[NvimUnity] Output: " + output);

            if (!string.IsNullOrEmpty(error))
                Debug.LogError("[NvimUnity] Error: " + error);
        }
    }

    public void OnGUI()
    {
        var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
        rect.width = 252;
        if (GUI.Button(rect, "Regenerate project files"))
        {
            SyncHelper.RegenerateProjectFiles();
        }
    }

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
        // Optional: implement sync strategy here
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

    private static bool IsNvimUnityDefaultEditor()
    {
        var current = EditorPrefs.GetString("kScriptsDefaultApp");
        string expected = NormalizePath(GetLauncherPath());
        
        Debug.Log($"[NvimUnity] EditorPrefs current: {current}");
        Debug.Log($"[NvimUnity] Expected launcher: {expected}");

        return string.Equals(current, expected, StringComparison.OrdinalIgnoreCase);
    }

}

