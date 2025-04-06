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
    private static readonly string launcherPath;
    private static readonly bool isWindows;

    static NeovimCodeEditor()
    {
        isWindows = Application.platform == RuntimePlatform.WindowsEditor;
        launcherPath = NormalizePath(GetLauncherPath());

        if (!File.Exists(launcherPath))
        {
            Debug.LogWarning($"[NvimUnity] Launcher script not found at: {launcherPath}");
        }

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

        Debug.Log($"[NvimUnity] Opening file: {fullPath} at line {line}");
        Debug.Log($"[NvimUnity] Using launcher: {launcherPath}");

        if (!File.Exists(launcherPath))
        {
            Debug.LogError($"[NvimUnity] Launcher script not found: {launcherPath}");
            return false;
        }

        Debug.Log($"[NvimUnity] Lançando Neovim com:");
        Debug.Log($"  Launcher: {launcherPath}");
        Debug.Log($"  File exists? {File.Exists(launcherPath)}");
        Debug.Log($"  Arguments: \"{fullPath}\" +{line}");


        try
        {
            /*var psi = new ProcessStartInfo*/
            /*{*/
            /*    FileName = isWindows ? launcherPath : "/bin/bash",*/
            /*    Arguments = isWindows ? $"\"{fullPath}\" {line}" : $"\"{launcherPath}\" \"{fullPath}\" {line}",*/
            /*    UseShellExecute = isWindows,*/
            /*    CreateNoWindow = false*/
            /*};*/

            var psi = new ProcessStartInfo
{
    FileName = "cmd.exe",
    Arguments = $"/c \"\"{launcherPath}\" \"{fullPath}\" +{line}\"",
    UseShellExecute = false,
    CreateNoWindow = false,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
//    WorkingDirectory = Path.GetDirectoryName(launcherPath)
};

Debug.Log($"[NvimUnity] ProcessStartInfo:");
Debug.Log($"  FileName: {psi.FileName}");
Debug.Log($"  Arguments: {psi.Arguments}");
Debug.Log($"  UseShellExecute: {psi.UseShellExecute}");



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
        return string.Equals(defaultApp, launcherPath, StringComparison.OrdinalIgnoreCase);
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
            Path = launcherPath
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
        var installations = Installations;

        if (!installations.Any())
        {
            installation = new CodeEditor.Installation
            {
                Name = editorName,
                Path = launcherPath
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
}

