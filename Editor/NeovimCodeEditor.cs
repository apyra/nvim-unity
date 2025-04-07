using System;
using System.Diagnostics;
using System.IO;
using System.Text;
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
        private static readonly string launcher = Path.GetFullPath(Utils.NormalizePath(Utils.GetLauncherPath()));

        static NeovimCodeEditor()
        {
            Debug.Log("[NvimUnity] Registering NeovimCodeEditor...");
            CodeEditor.Register(new NeovimCodeEditor());
            
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
            return NvimUnityServer.OpenFile(filePath, line);
        }

        private static bool IsNvimUnityDefaultEditor()
        {
            string defaultApp = Utils.NormalizePath(EditorPrefs.GetString("kScriptsDefaultApp"));
            string expectedPath = Utils.NormalizePath(Utils.GetLauncherPath());

            return defaultApp.Contains("nvim-unity") || 
                   defaultApp.Equals(expectedPath, StringComparison.OrdinalIgnoreCase);
        }

        public void OnGUI()
        {
            var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            rect.width = 252;

            if (GUI.Button(rect, "Regenerate project files"))
            {
                Utils.RegenerateProjectFiles();
            }

            GUILayout.Label("NvimUnity HTTP Server", EditorStyles.boldLabel);
            GUILayout.Label("Status: " + NvimUnityServer.GetStatus());
            var address = GUILayout.TextField(NvimUnityServer.ServerAddress);

            if (GUILayout.Button("Restart Server"))
            {
                NvimUnityServer.StopServer();
                NvimUnityServer.ServerAddress = address;
                NvimUnityServer.StartServer();
            }
        }

        public void Initialize(string editorInstallationPath)
        {
            // Not required, Unity will call this on registration
        }

        public CodeEditor.Installation[] Installations => new[]
        {
            new CodeEditor.Installation { Name = editorName, Path = Path.GetFullPath(Utils.NormalizePath(Utils.GetLauncherPath()))}
        };

        public void SyncAll()
        {
            AssetDatabase.Refresh();
            Utils.RegenerateProjectFiles();
        }

        public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
        {
            // Optional: add logic to regenerate only if relevant
        }

        public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
        {
            installation = new CodeEditor.Installation { Name = editorName, Path = launcher };
            return true;
        }

        
    }
}

