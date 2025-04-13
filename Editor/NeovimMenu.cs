using UnityEditor;

namespace NvimUnity
{
    public static class NeovimMenu
    {
        [MenuItem("Assets/Neovim Code Editor/Regenerate Project Files")]
        public static void RegenerateProjectFiles()
        {
            //AssetDatabase.Refresh();
            Project.GenerateAll();
            //UnityEngine.Debug.Log("Project files regenerated.");
        }
    
    }
}

