using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
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
        private static readonly string launcher = Path.GetFullPath(Utils.NormalizePath(GetLauncherPath()));
        private static readonly HttpClient httpClient = new HttpClient();

        static NeovimCodeEditor()
        {
            Debug.Log("[NvimUnity] Registering NeovimCodeEditor...");
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
            if (!IsNvimUnityDefaultEditor() || string.IsNullOrEmpty(filePath)) return false;
            if (line < 1) line = 1;

            string fullPath = Path.GetFullPath(filePath).Replace("\\", "/");
            string data = $"{fullPath}:{line}";

            try
            {
                var content = new StringContent(data, Encoding.UTF8, "text/plain");
                var result = httpClient.PostAsync("http://localhost:5005/open", content).Result;

                if (!result.IsSuccessStatusCode)
                {
                    Debug.LogWarning($"[NvimUnity] Server returned error: {result.StatusCode}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[NvimUnity] Failed to call /open endpoint: " + ex.Message);
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
        }

        public void Initialize(string editorInstallationPath)
        {
            // Not required, Unity will call this on registration
        }

        public CodeEditor.Installation[] Installations => new[]
        {
            new CodeEditor.Installation { Name = editorName, Path = launcher }
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
    }
}

