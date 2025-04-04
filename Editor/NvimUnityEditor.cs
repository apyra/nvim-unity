using UnityEditor;
using UnityEngine;
using System.IO;

public class NvimUnityEditor : EditorWindow
{
    [MenuItem("NvimUnity/Regenerate Project Files")]
    public static void RegenerateProjectFiles()
    {
        Debug.Log("Regenerating .csproj and .sln files...");
        UnityEditor.SyncVS.SyncSolution(); // Generate .sln
        GenerateVSCodeFolder(); // Create .vscode folder
        AssetDatabase.Refresh();
        Debug.Log("Done.");
    }

    private static void GenerateVSCodeFolder()
    {
        string vscodePath = Path.Combine(Directory.GetCurrentDirectory(), ".vscode");

        if (!Directory.Exists(vscodePath))
            Directory.CreateDirectory(vscodePath);

        string settingsJson = @"{
  ""omnisharp.useModernNet"": true,
  ""omnisharp.enableRoslynAnalyzers"": true,
  ""csharp.format.enable"": true
}";
        File.WriteAllText(Path.Combine(vscodePath, "settings.json"), settingsJson);

        string launchJson = @"{
  ""version"": ""0.2.0"",
  ""configurations"": []
}";
        File.WriteAllText(Path.Combine(vscodePath, "launch.json"), launchJson);
    }
}
