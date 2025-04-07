using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;
using Debug = UnityEngine.Debug;

namespace NvimUnity
{
    [InitializeOnLoad]
    public class NeovimCodeEditor : IExternalCodeEditor
    {
        private static readonly string editorName = "Neovim (NvimUnity)";
        private static readonly string launcher = Path.GetFullPath(Utils.NormalizePath(GetLauncherPath()));

        static NeovimCodeEditor()
        {
            Debug.Log("[NvimUnity] Registering NeovimCodeEditor...");
            CodeEditor.Register(new NeovimCodeEditor());
            EnsureLauncherExecutable();
        }

        public string GetDisplayName()
        {
            Debug.Log("[NvimUnity] GetDisplayName called");
            return editorName;
        }

        public bool OpenProject(string path, int line, int column)
        {
            Debug.Log($"[NvimUnity] OpenProject called with path: {path}, line: {line}, column: {column}");
            if (string.IsNullOrEmpty(path)) return false;
            return OpenFileAtLine(path, line);
        }

        public bool OpenFileAtLine(string filePath, int line)
        {
            Debug.Log($"[NvimUnity] OpenFileAtLine called with filePath: {filePath}, line: {line}");

            if (!IsNvimUnityDefaultEditor())
            {
                Debug.LogWarning("[NvimUnity] Not default editor, aborting");
                return false;
            }

            if (string.IsNullOrEmpty(filePath)) return false;
            if (line < 1) line = 1;

            string fullPath = Path.GetFullPath(filePath);
            string quotedFile = $"\"{fullPath}\"";
            string lineArg = $"+{line}";

            if (!File.Exists(launcher))
            {
                Debug.LogWarning("[NvimUnity] Launcher not found at path: " + launcher);
                return false;
            }

            try
            {
#if UNITY_EDITOR_WIN
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"\"{launcher}\" {quotedFile} {lineArg}\"",
#else
                var psi = new ProcessStartInfo
                {
                    FileName = launcher,
                    Arguments = $"{quotedFile} {lineArg}",
#endif
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Debug.Log("[NvimUnity] Starting process: " + psi.FileName + " " + psi.Arguments);
                Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[NvimUnity] Failed to start launcher: " + ex.Message);
                return false;
            }
        }

        private static void EnsureLauncherExecutable()
        {
#if !UNITY_EDITOR_WIN
            try
            {
                string path = GetLauncherPath();
                Debug.Log("[NvimUnity] Ensuring launcher is executable: " + path);
                if (File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "/bin/chmod",
                        Arguments = $"+x \"{path}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[NvimUnity] Failed to chmod launcher: " + e.Message);
            }
#endif
        }

        private static bool IsNvimUnityDefaultEditor()
        {
            string defaultApp = Utils.NormalizePath(EditorPrefs.GetString("kScriptsDefaultApp"));
            string expectedPath = Utils.NormalizePath(GetLauncherPath());
            Debug.Log($"[NvimUnity] Checking default editor: {defaultApp} vs expected {expectedPath}");

            return defaultApp.Contains("nvim-unity") || defaultApp.Equals(expectedPath, StringComparison.OrdinalIgnoreCase);
        }

        public void OnGUI()
        {
            var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            rect.width = 252;
            if (GUI.Button(rect, "Regenerate project files"))
            {
                Debug.Log("[NvimUnity] Regenerate button clicked");
                Utils.RegenerateProjectFiles();
            }
        }

        public void Initialize(string editorInstallationPath)
        {
            Debug.Log("[NvimUnity] Initialize called with: " + editorInstallationPath);
        }

        public CodeEditor.Installation[] Installations => new[]
        {
            new CodeEditor.Installation { Name = editorName, Path = launcher }
        };

        public void SyncAll()
        {
            Debug.Log("[NvimUnity] SyncAll called");
            AssetDatabase.Refresh();
            Utils.RegenerateProjectFiles();
        }

        public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
        {
            Debug.Log("[NvimUnity] SyncIfNeeded called (currently noop)");
        }

        public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
        {
            Debug.Log("[NvimUnity] TryGetInstallationForPath called with: " + editorPath);
            installation = new CodeEditor.Installation { Name = editorName, Path = launcher };
            return true;
        }

        private static string GetLauncherPath()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string scriptPath = Path.Combine(projectRoot, "Packages/com.apyra.nvim-unity/Launcher/nvim-open");

#if UNITY_EDITOR_WIN
            return scriptPath + ".bat";
#else
            return scriptPath + ".sh";
#endif
        }
    }
}

