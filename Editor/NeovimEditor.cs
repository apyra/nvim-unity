using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;
using Debug = UnityEngine.Debug;

namespace NvimUnity
{
    [InitializeOnLoad]
    public class NeovimEditor : IExternalCodeEditor
    {
        public static string defaultApp => EditorPrefs.GetString("kScriptsDefaultApp");
        public static string OS = Utils.GetCurrentOS();
        public static string RootFolder = Utils.GetProjectRoot();

        private static Config config;
        private static bool needSaveConfig = false;
        private static bool useCustomTerminal = false;
        public static string Terminal = "";

        private static string EditorName = "Neovim Code Editor";
        private static string Socket = @"\\.\pipe\nvim-unity2025";

        static NeovimEditor()
        {
            CodeEditor.Register(new NeovimEditor());
            config = ConfigManager.LoadConfig();
            Terminal = config.GetResolvedTerminalForPlatform(OS); 
            useCustomTerminal = config.use_custom_terminal;
        }

        public string GetDisplayName() => EditorName;

        public static bool IsNvimUnityDefaultEditor()
        {
            return  string.Equals(defaultApp,Utils.GetLauncherPath());
        }

        public bool OpenProject(string path, int line, int column)
        {
            if (string.IsNullOrEmpty(path) || !IsNvimUnityDefaultEditor()) return false;
           
            bool IsRunnigInNeovim = SocketChecker.IsSocketActive(Socket);

            if( line<=0 ) line = 1;

            if(!IsRunnigInNeovim)
            {
                string nvimArgs = $"--listen \"{Socket}\" \"+lcd {RootFolder}\" \"+{line}\" {path}";
                string usingApp = defaultApp;
                string args = nvimArgs;

                if(useCustomTerminal)
                {
                    usingApp = defaultApp;
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
                return OpenFile(path, line);
            }
        }

        public bool OpenFile(string filePath, int line)
        {
            try
            {
                string cmd = $"<Esc>:e {filePath}<CR>{line}G";
                string nvimArgs = $"--server {Socket} --remote-send \"{cmd}\"";
                string nvimPath = Utils.GetNeovimPath();

                var psi = new ProcessStartInfo
                {
                    FileName = nvimPath,
                    Arguments = nvimArgs,
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
                SyncAll();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("Terminal Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);

            var prevValue = useCustomTerminal;
            var newValue = EditorGUILayout.Toggle(new GUIContent("Use a custom terminal", 
                        "Let you choose a terminal to run neovim with"), prevValue);
            
            if (newValue != prevValue)
            {
                useCustomTerminal = newValue;
                config.use_custom_terminal = useCustomTerminal;
                needSaveConfig = true;
            }

            if(useCustomTerminal)
            {
                EditorGUILayout.BeginHorizontal();
                Terminal = EditorGUILayout.TextField($"{OS} Terminal:", Terminal);
                
                if (GUILayout.Button("Save"))
                {
                    config.SetTerminalForPlatform(OS,Terminal);

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
            new CodeEditor.Installation { Name = EditorName, Path = defaultApp }
        };

        public void SyncAll() {
            Project.GenerateAll();
        }

        public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
        {
            if (!Project.Exists())
            {
                SyncAll();
                return;
            }

            // Verifica se há algum arquivo .cs dentro da pasta Assets
            bool hasCsInAssets =
            addedFiles.Concat(deletedFiles)
                      .Concat(movedFiles)
                      .Concat(movedFromFiles)
                    //  .Concat(importedFiles)
                      .Any(path =>
                          path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                          path.Replace('\\', Path.DirectorySeparatorChar)
                              .Contains("Assets" + Path.DirectorySeparatorChar));

            if (hasCsInAssets)
            {
                Project.GenerateCompileIncludes();
            }
        }

        public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
        {
            Utils.EnsureLauncherExecutable();
            installation = new CodeEditor.Installation { Name = EditorName, Path = Utils.GetLauncherPath() };
            return true;
        }

        public void Save()
        {
            if(needSaveConfig)
            {
                ConfigManager.SaveConfig(config);           
                needSaveConfig = false;
            }
        }
    }
}

