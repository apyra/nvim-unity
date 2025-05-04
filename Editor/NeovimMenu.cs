using UnityEditor;
using Unity.CodeEditor;

namespace NvimUnity
{
    public static class NeovimMenu
    {
        [MenuItem("Tools/Neovim Code Editor/Regenerate Project Files")]
        public static void RunSyncAll()
        {
            if (CodeEditor.CurrentEditor is NeovimEditor editor)
            {
                editor.SyncAll();
            }
        }

        [MenuItem("Tools/Neovim Code Editor/Regenerate Project Files", true)]
        public static bool ValidateRunSyncAll()
        {
            return CodeEditor.CurrentEditor is NeovimEditor;
        }
    }
}

