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
            CodeEditor.Register(new NeovimCodeEditor());
            EnsureLauncherExecutable();
        }

        public string GetDisplayName() => editorName;

        public bool OpenProject(string path, int line, int column)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return OpenFileAtLine(path, line);
        }

        public bool OpenFileAtLine(string filePath, int line)
        {
            if (!IsNvimUnityDefaultEditor()) return false;
            if (string.IsNullOrEmpty(filePath)) return false;
            if (line < 1) line = 1;

            string fullPath = Path.GetFullPath(filePath);
            string quotedFile = $"\"{fullPath}\"";
            string lineArg = $"+{line}";

            if (!File.Exists(launcher)) return false;

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

                Process.Start(psi);
                return true;
            }
            catch (Exception) { return false; }
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
                }
            }
            catch (Exception) { }
#endif
        }

        private static bool IsNvimUnityDefaultEditor()
        {
            string defaultApp = Utils.NormalizePath(EditorPrefs.GetString("kScriptsDefaultApp"));
            string expectedPath = Utils.NormalizePath(GetLauncherPath());
            return defaultApp.Contains("nvim-unity") || defaultApp.Equals(expectedPath, StringComparison.OrdinalIgnoreCase);
        }

        public void OnGUI()
        {
            var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            rect.width = 252;
            if (GUI.Button(rect, "Regenerate project files"))
            {
                Utils.RegenerateProjectFiles();
            }
        }

        public void Initialize(string editorInstallationPath) { }

        public CodeEditor.Installation[] Installations => new[]
        {
            new CodeEditor.Installation { Name = editorName, Path = launcher }
        };

        public void SyncAll()
        {
            AssetDatabase.Refresh();
            Utils.RegenerateProjectFiles();
        }

        public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles) {}

        public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
        {
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
