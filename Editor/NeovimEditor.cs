using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;

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
        private static bool debugging = false;

        private static string EditorName = "Neovim Code Editor";
        private static string Socket =>
            OS == "Windows" ? @"\\.\pipe\unity2025" :
            $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.cache/nvimunity.sock";

        static NeovimEditor()
        {
            CodeEditor.Register(new NeovimEditor());
            config = ConfigManager.LoadConfig();
            config.last_project = RootFolder;
            ConfigManager.SaveConfig(config);
        }

        public string GetDisplayName() => EditorName;

        public static bool IsNvimUnityDefaultEditor()
        {
            return string.Equals(defaultApp, Utils.GetLauncherPath());
        }

        public bool OpenProject(string path, int line, int column)
        {
            if (string.IsNullOrEmpty(path) || 
               !IsNvimUnityDefaultEditor() ||
               !Project.SupportsFile(path)
                    ) return false;

            if (!Project.Exists())
                SyncAll();

            bool IsRunnigInNeovim = SocketChecker.IsSocketActive(Socket);

            if (line <= 0) line = 1;

            if (!IsRunnigInNeovim)
            {
                try
                {
                    if (OS == "Windows")
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = defaultApp,
                            Arguments = $"{path} {line}",
                            UseShellExecute = true,
                            CreateNoWindow = false,
                        };

                        if(debugging)
                        UnityEngine.Debug.Log($"[NvimUnity] Executing: {psi.FileName} {psi.Arguments}");
                        Process.Start(defaultApp, $"{path} {line}");
                    }
                    else
                    {
                        // Original behavior for other OSes
                        ProcessStartInfo psi = Utils.BuildProcessStartInfo(defaultApp, path, line);
                        if(debugging)
                        UnityEngine.Debug.Log($"[NvimUnity] Executing in terminal: {psi.FileName} {psi.Arguments}");
                        Process.Start(psi);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[NvimUnity] Failed to start App: {ex.Message}");
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
                string cmd = $"<CMD>e +{line} {filePath}<CR>";
                string nvimArgs = $"--server {Socket} --remote-send \"{cmd}\"";
                string nvimPath = Utils.GetNeovimPath();

                var psi = new ProcessStartInfo
                {
                    FileName = nvimPath,
                    Arguments = nvimArgs,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                Process.Start(nvimPath, nvimArgs);
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[NvimUnity] Failed to start App: {ex.Message}");
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
        }

        public void Initialize(string editorInstallationPath)
        {
            // Not used by NvimUnity, but required by interface
        }
        
        public CodeEditor.Installation[] Installations => new[]
        {
            new CodeEditor.Installation
            {
                Name = EditorName,
                Path = Utils.GetLauncherPath()
            }
        };

        public void SyncAll()
        {
            AssetDatabase.Refresh();
            Project.GenerateAll();
        }

        public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
        {
            if (!Project.Exists())
            {
                SyncAll();
                return;
            }

            if (Project.HasFilesBeenDeletedOrMoved())
            {
                Project.GenerateCompileIncludes();
                return;
            }

            var fileList = addedFiles.Concat(importedFiles);

            bool hasCsInAssets =
            fileList.Any(path =>
                    path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                    Utils.IsInAssetsFolder(path));

            if (hasCsInAssets)
            {
                if (Project.NeedRegenerateCompileIncludes(fileList.ToList()))
                    Project.GenerateCompileIncludes();
            }
        }

        public bool TryGetInstallationForPath(string path, out CodeEditor.Installation installation)
        {
            if (path == Utils.GetLauncherPath())
            {
                installation = new CodeEditor.Installation
                {
                    Name = EditorName,
                    Path = Utils.GetLauncherPath()
                };
                return true;
            }

            installation = default;
            return false;
        }


        public void Save()
        {
            if (needSaveConfig)
            {
                ConfigManager.SaveConfig(config);
                needSaveConfig = false;
            }
        }
    }
}

