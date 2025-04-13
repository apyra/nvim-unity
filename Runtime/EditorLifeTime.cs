using UnityEngine;

namespace NvimUnity
{
    public class EditorLifeTime : MonoBehaviour
    {
        void OnApplicationQuit()
        {
            if(NeovimEditor.IsNvimUnityDefaultEditor())
                NeovimEditor.Save();
        }

    }
}

