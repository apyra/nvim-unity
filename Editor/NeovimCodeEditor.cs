using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[InitializeOnLoad]
public class NeovimCodeEditor : IExternalCodeEditor
{
    private static readonly string editorName = "Neovim (nvim-unity)";
    private static readonly string launcherPath = "Packages/com.apyra.nvim-unity/Launch/nvim-launch";

    static NeovimCodeEditor()
    {
        CodeEditor.Register(editorName, new NeovimCodeEditor());
    }

    public string GetDisplayName()
    {
        return editorName;
    }

    public bool OpenProject(string path, int line, int column)
    {
        return OpenFileAtLine(path, line);
    }

    public bool OpenFileAtLine(string filePath, int line)
    {
        string fullPath = Path.GetFullPath(filePath);
        string launchScript = GetLauncherPath();

        if (!File.Exists(launchScript))
        {
            Debug.LogError($"[nvim-unity] Launch script not found: {launchScript}");
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = launchScript,
                Arguments = $"\"{fullPath}\" +{line}",
                UseShellExecute = true
            });
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[nvim-unity] Failed to launch Neovim: " + e.Message);
            return false;
        }
    }

    public void OnGUI()
    {
        // Optional: show settings in Preferences window
    }

    public void Initialize(string editorInstallationPath) { }

    public CodeEditor.Installation[] Installations =>
        new[]
        {
            new CodeEditor.Installation
            {
                Name = editorName,
                Path = GetLauncherPath()
            }
        };

    private static string GetLauncherPath()
    {
#if UNITY_EDITOR_WIN
        return launcherPath + ".bat";
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
        return launcherPath + ".sh";
#else
        return launcherPath;
#endif
    }
}

