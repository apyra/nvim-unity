using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Unity.CodeEditor;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public class NeovimCodeEditor : IExternalCodeEditor
{
    private static readonly string editorName = "Neovim (NvimUnity)";
    private static readonly string launcherPath = GetPackagesFolderPath() + "/NvimUnity/Launch/nvim-open";

    static NeovimCodeEditor()
    {
        CodeEditor.Register(new NeovimCodeEditor());
        EnsureProjectFiles();
    }

    public string GetDisplayName()
    {
        return editorName;
    }

    public bool OpenProject(string path, int line, int column)
    {
        return OpenFileAtLine(path, line);
    }

    public bool OpenFileAtLine(string filePath, int line)
    {
        string fullPath = Path.GetFullPath(filePath);
        string launchScript = GetLauncherPath();
        Debug.Log(launchScript);

        if (!File.Exists(launchScript))
        {
            Debug.LogError($"[NvimUnity] Launch script not found: {launchScript}");
            return false;
        }

        try
        {
            EnsureProjectFiles();

            Process.Start(new ProcessStartInfo
            {
                FileName = launchScript,
                Arguments = $"\"{fullPath}\" +{line}",
                UseShellExecute = true
            });
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[NvimUnity] Failed to launch Neovim: " + e.Message);
            return false;
        }
    }

    public void OnGUI()
    {
        // Optional: show settings in Preferences window
    }

    public void Initialize(string editorInstallationPath) { }

    public CodeEditor.Installation[] Installations =>
        new[]
        {
            new CodeEditor.Installation
            {
                Name = editorName,
                Path = GetLauncherPath()
            }
        };

     // Unity calls this function during initialization in order to sync the Project. This is different from SyncIfNeeded in that it does not get a list of changes.
  public void SyncAll()
  {
    AssetDatabase.Refresh();
    SyncHelper.RegenerateProjectFiles();
  }

  // When you change Assets in Unity, this method for the current chosen instance of IExternalCodeEditor parses the new and changed Assets.
  public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
  {
    /*(_projectGeneration.AssemblyNameProvider as IPackageInfoCache)?.ResetPackageInfoCache();*/
    /*_projectGeneration.SyncIfNeeded(addedFiles.Union(deletedFiles).Union(movedFiles).Union(movedFromFiles).ToList(), importedFiles);*/
    /**/
    /**/
  }


    public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
  {
    var lowerCasePath = editorPath.ToLower();
    var filename = Path.GetFileName(lowerCasePath).Replace(" ", "");
    var installations = Installations;

    /*if (!_supportedFileNames.Contains(filename))*/
    /*{*/
    /*  installation = default;*/
    /*  return false;*/
    /*}*/

    if (!installations.Any())
    {
      installation = new CodeEditor.Installation
      {
        Name = editorName,
        Path =  NormalizePath(GetLauncherPath())
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
          Path = NormalizePath(editorPath)
        };
      }
    }

    return true;
  }

    private static string GetLauncherPath()
    {
#if UNITY_EDITOR_WIN
        return launcherPath + ".bat";
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
        return launcherPath + ".sh";
#else
        return launcherPath;
#endif
    }

    private static void EnsureProjectFiles()
    {
        string rootPath = Directory.GetCurrentDirectory();
        string projectName = new DirectoryInfo(rootPath).Name;
        string slnPath = Path.Combine(rootPath, $"{projectName}.sln");

        bool hasCsproj = Directory.GetFiles(rootPath, "*.csproj", SearchOption.TopDirectoryOnly).Length > 0;
        bool hasSln = File.Exists(slnPath);

        if (!hasCsproj || !hasSln)
        {
            Debug.Log("Generating missing project files...");
            SyncHelper.RegenerateProjectFiles();
        }

        string vscodePath = Path.Combine(rootPath, ".vscode");
        if (!Directory.Exists(vscodePath))
        {
            Directory.CreateDirectory(vscodePath);
            Debug.Log(".vscode folder created.");
        }
    }

    private static string GetPackagesFolderPath()
    {
        string assetsPath = Application.dataPath;
        var parent = Directory.GetParent(assetsPath);
        if (parent == null)
        {
            Debug.LogError("[nvim-unity] Could not resolve Packages folder.");
            return null;
        }
        return Path.Combine(parent.FullName, "Packages");
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

