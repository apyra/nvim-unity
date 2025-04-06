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
        string quotedFile = $"\"{fullPath}\"";
        string lineArg = $"+{line}";

        Debug.Log($"[NvimUnity] Opening: {fullPath} at line {line}");

        try
        {
            string command = "nvr";
            string args = $"--remote-tab {quotedFile} {lineArg}";

            if (!CommandExists("nvr"))
            {
                Debug.LogWarning("[NvimUnity] nvr not found, falling back to nvim with --listen");
                command = "nvim";
                args = $"--listen \\\\.\\pipe\\nvim-pipe {quotedFile} {lineArg}";
            }

            var psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
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

    private static bool IsNvimUnityDefaultEditor()
    {
        string defaultApp = NormalizePath(EditorPrefs.GetString("kScriptsDefaultApp"));
        string expectedPath = NormalizePath(Process.GetCurrentProcess().MainModule.FileName); // Fallback to current app
        return defaultApp.Contains("nvim-unity"); // Mais permissivo
    }

    private static bool CommandExists(string command)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "where",
                Arguments = command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using (Process p = Process.Start(psi))
            {
                p.WaitForExit();
                return p.ExitCode == 0;
            }
        }
        catch
        {
            return false;
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
            Path = "nvim-unity" // Fictício, pois não usamos mais caminho exato
        }
    };

    public void SyncAll()
    {
        AssetDatabase.Refresh();
        SyncHelper.RegenerateProjectFiles();
    }

    public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
    {
        // Optional sync
    }

    public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
    {
        installation = new CodeEditor.Installation
        {
            Name = editorName,
            Path = "nvim-unity"
        };
        return true;
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

