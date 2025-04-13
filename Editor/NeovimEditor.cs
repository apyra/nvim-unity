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
        public static string App => EditorPrefs.GetString("kScriptsDefaultApp");
        public static string Terminal = "";
        public static string OS = Utils.GetCurrentOS();
        public static string RootFolder = Utils.GetProjectRoot();

        private static bool UseCustomTerminal = false;
        
        private static string EditorName = "Neovim Code Editor";
        private static string Socket = @"\\.\pipe\nvim-unity2025";
        private static Config config;

        static NeovimEditor()
        {
            CodeEditor.Register(new NeovimEditor());
            config = ConfigManager.LoadConfig();
            config.terminals.TryGetValue(OS, out Terminal);
            UseCustomTerminal = config.use_custom_terminal;
        }

        public string GetDisplayName() => EditorName;

        private static bool IsNvimUnityDefaultEditor()
        {
            return  string.Equals(App,Utils.GetLauncherPath());
        }

        public bool OpenProject(string path, int line, int column)
        {
            if (string.IsNullOrEmpty(path) || !IsNvimUnityDefaultEditor()) return false;
            bool IsRunnigInNeovim = SocketChecker.IsSocketActive(Socket);

            if( line<=0 ) line = 1;

            if(!IsRunnigInNeovim)
            {
                bool openRoot =true;

                string nvimArgs = $"--listen \"{Socket}\" \"+cd {RootFolder}\" \"+{line}\" {path}";
                string usingApp;
                string args = nvimArgs;

                if(UseCustomTerminal)
                {
                    usingApp = App;
                    string safeTerminal = Terminal.Replace(" ", "^ ");

                    if(string.IsNullOrEmpty(safeTerminal))
                    {
                        Debug.LogError("[NvimUnity] You have to provide a valid terminal in Preferences>ExternalTools>TerminalSettings");
                        return false;
                    }

                    args = $"\"{Terminal}\" {nvimArgs}";
                }
                else
                {
                    usingApp = Utils.GetNeovimPath();
                }

                Debug.Log($"Sending args: {args}");

                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = usingApp,
                        Arguments = args,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                    Process.Start(psi);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[NvimUnity] Failed to start App: {ex.Message}");
                    return false;
                }
            }
            else
            {
                return OpenFileAtLine(path, line);
            }
        }

        public bool OpenFileAtLine(string filePath, int line)
        {
            if (!IsNvimUnityDefaultEditor()) return false;
            
            try
            {
                string cmd = $"<Esc>:e {filePath}<CR>{line}G";
                string args = $"--server {Socket} --remote-send \"{cmd}\"";
                string neovimPath = Utils.GetNeovimPath();

                var psi = new ProcessStartInfo
                {
                    FileName = neovimPath,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                Process.Start(psi);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NvimUnity] Failed to start App: {ex.Message}");
                return false;
            }
        }

        public void OnGUI()
        {   
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Project Files", EditorStyles.boldLabel);

            if (GUILayout.Button("Regenerate project files"))
            {
                Project.GenerateAll();
            }

            EditorGUILayout.EndHorizontal();

            // Opção config.use_custom_terminal

            GUILayout.Space(10);
            GUILayout.Label("Terminal Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);

            var prevValue = UseCustomTerminal;
            var newValue = EditorGUILayout.Toggle(new GUIContent("Use a custom terminal", 
                        "Let you choose a terminal to run neovim with"), prevValue);
            
            if (newValue != prevValue)
            {
                UseCustomTerminal = newValue;
                config.use_custom_terminal = UseCustomTerminal;
                ConfigManager.SaveConfig(config);
            }

            if(UseCustomTerminal)
            {
                EditorGUILayout.BeginHorizontal();
                Terminal = EditorGUILayout.TextField($"{OS} Terminal:", Terminal);
                
                if (GUILayout.Button("Save"))
                {
                    config.terminals[OS] = Terminal;

                    ConfigManager.SaveConfig(config);
                    EditorUtility.DisplayDialog("Saved", $"Terminal for {OS} saved!", "OK");
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
            
        }

        public void Initialize(string editorInstallationPath)
        {
            // Not used by NvimUnity, but required by interface
        }

        public CodeEditor.Installation[] Installations => new[]
        {
            new CodeEditor.Installation { Name = EditorName, Path = App }
        };

        public void SyncAll() {}

        public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
        {
            Project.GenerateProject();
        }

        public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
        {
            Utils.EnsureLauncherExecutable();
            installation = new CodeEditor.Installation { Name = EditorName, Path = Utils.GetLauncherPath() };
            return true;
        }
    }
}

