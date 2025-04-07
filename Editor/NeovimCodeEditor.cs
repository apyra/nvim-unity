using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public class NeovimCodeEditor : IExternalCodeEditor
{
    private static readonly string editorName = "Neovim (NvimUnity)";
    private static readonly string launcher = NormalizePath(GetLauncherPath());

    static NeovimCodeEditor()
    {
        CodeEditor.Register(new NeovimCodeEditor());
        EnsureLauncherExecutable();
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
        string lineArg = $"+{line}";

        if (!File.Exists(launcher))
        {
            Debug.LogError($"[NvimUnity] Launcher not found: {launcher}");
            return false;
        }

        Debug.Log($"[NvimUnity] Using launcher: {launcher}");
        Debug.Log($"[NvimUnity] Opening: {fullPath} at line {line}");

        try
        {
            var psi = new ProcessStartInfo
            {
#if UNITY_EDITOR_WIN
                FileName = launcher,
                Arguments = $"\"{fullPath}\" {lineArg}",
#else
                FileName = "/bin/bash",
                Arguments = $"\"{launcher}\" \"{fullPath}\" {lineArg}",
#endif
                UseShellExecute = true,
            };

            Process.Start(psi);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[NvimUnity] Failed to launch Neovim: " + e.Message);
            return false;
        }
    }

    private static void EnsureLauncherExecutable()
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
                Debug.Log($"[NvimUnity] Ensured script executable: {path}");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[NvimUnity] Failed to set script as executable: " + e.Message);
        }
#endif
    }

    private static bool IsNvimUnityDefaultEditor()
    {
        string defaultApp = NormalizePath(EditorPrefs.GetString("kScriptsDefaultApp"));
        string expectedPath = NormalizePath(GetLauncherPath());
        return defaultApp.Contains("nvim-unity") || defaultApp.Equals(expectedPath, StringComparison.OrdinalIgnoreCase);
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
            Path = launcher
        }
    };

    public void SyncAll()
    {
        AssetDatabase.Refresh();
        SyncHelper.RegenerateProjectFiles();
    }

    public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles) {}

    public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
    {
        installation = new CodeEditor.Installation
        {
            Name = editorName,
            Path = launcher
        };
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
}

