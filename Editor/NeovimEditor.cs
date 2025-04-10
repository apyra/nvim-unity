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
    public class NeovimEditor : IExternalCodeEditor
    {
        public static readonly string App = Utils.GetLauncherPath();
        public static readonly string OS = Utils.GetCurrentOS();
        public static readonly string RootFolder = Utils.FindProjectRoot();
        public static string Terminal = "";

        private static readonly string editorName = "Neovim Editor";
        private static Config config;

        static NeovimEditor()
        {
            CodeEditor.Register(new NeovimEditor());
            config = Utils.GetConfig();
            config.terminals.TryGetValue(OS, out Terminal);
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
            return FileOpener.OpenFile(filePath, line);
        }

        private static bool IsNvimUnityDefaultEditor()
        {
            string defaultApp = Utils.NormalizePath(EditorPrefs.GetString("kScriptsDefaultApp"));
            return defaultApp.Contains("nvim-unity", StringComparison.OrdinalIgnoreCase)
                   || defaultApp.Equals(FileOpener.LauncherPath, StringComparison.OrdinalIgnoreCase);
        }

        public void OnGUI()
        {
            var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            rect.width = 252;

            GUILayout.Space(20);
            GUILayout.Label("Project Files", EditorStyles.boldLabel);
            GUILayout.Space(20);

            if (GUI.Button(rect, "Regenerate project files"))
            {
                Utils.RegenerateProjectFiles();
            }
            GUILayout.Space(20);
            GUILayout.Label("Terminal Settings", EditorStyles.boldLabel);
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            Terminal = EditorGUILayout.TextField($"{OS} Terminal:", Terminal);
            if (GUILayout.Button("Save"))
            {
                config.terminals[OS] = Terminal;
                Utils.SaveConfig(config);
                EditorUtility.DisplayDialog("Saved", $"Terminal for {OS} saved!", "OK");
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20);
        }

        public void Initialize(string editorInstallationPath)
        {
            // Not used by NvimUnity, but required by interface
        }

        public CodeEditor.Installation[] Installations => new[]
        {
            new CodeEditor.Installation { Name = editorName, Path = App }
        };

        public void SyncAll()
        {
            AssetDatabase.Refresh();
            Utils.RegenerateProjectFiles();
        }

        public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
        {
            // Optional: can implement file-based filtering if needed
        }

        public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
        {
            Utils.EnsureLauncherExecutable();
            installation = new CodeEditor.Installation { Name = editorName, Path = App };
            return true;
        }
    }
}

