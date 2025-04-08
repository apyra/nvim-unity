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
            return FileOpener.OpenFile(filePath, line);
        }

        private static bool IsNvimUnityDefaultEditor()
        {
            string defaultApp = Utils.NormalizePath(EditorPrefs.GetString("kScriptsDefaultApp"));
            return defaultApp.Contains("nvim-unity", StringComparison.OrdinalIgnoreCase)
                   || defaultApp.Equals(FileOpener.launcher, StringComparison.OrdinalIgnoreCase);
        }

        public void OnGUI()
        {
            var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            rect.width = 252;

            if (GUI.Button(rect, "Regenerate project files"))
            {
                Utils.RegenerateProjectFiles();
            }

            /*GUILayout.Label("NvimUnity HTTP Server", EditorStyles.boldLabel);*/
            /*GUILayout.Label("Status: " + NvimUnityServer.GetStatus());*/
            /*var address = GUILayout.TextField(NvimUnityServer.ServerAddress);*/

            /*if (GUILayout.Button("Restart Server"))*/
            /*{*/
            /*    NvimUnityServer.StopServer();*/
            /*    NvimUnityServer.ServerAddress = address;*/
            /*    NvimUnityServer.StartServer();*/
            /*}*/
        }

        public void Initialize(string editorInstallationPath)
        {
            // Not used by NvimUnity, but required by interface
        }

        public CodeEditor.Installation[] Installations => new[]
        {
            new CodeEditor.Installation { Name = editorName, Path = FileOpener.launcher }
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
            installation = new CodeEditor.Installation { Name = editorName, Path = FileOpener.launcher };
            return true;
        }
    }
}

