using UnityEditor;

public static class NeovimMenu
{
    [MenuItem("Assets/NvimUnity/Regenerate Project Files")]
    public static void RegenerateProjectFiles()
    {
        AssetDatabase.Refresh();
        SyncHelper.RegenerateProjectFiles();
        UnityEngine.Debug.Log("Project files regenerated.");
    }
    
}

