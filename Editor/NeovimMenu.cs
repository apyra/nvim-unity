using UnityEditor;

namespace NvimUnity
{
    public static class NeovimMenu
    {
        [MenuItem("Assets/NvimUnity/Regenerate Project Files")]
        public static void RegenerateProjectFiles()
        {
            AssetDatabase.Refresh();
            Utils.RegenerateProjectFiles();
            //UnityEngine.Debug.Log("Project files regenerated.");
        }
    
    }
}

