using UnityEditor;

public static class NeovimMenu
{
    [MenuItem("Assets/NvimUnity/Regenerate Project Files")]
    public static void RegenerateProjectFiles()
    {
        AssetDatabase.Refresh();
        UnityEditor.SyncVS.SyncSolution();
        UnityEngine.Debug.Log("Project files regenerated.");
    }
    
}

