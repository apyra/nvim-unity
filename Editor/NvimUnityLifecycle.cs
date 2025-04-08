using UnityEditor;

namespace NvimUnity
{
    [InitializeOnLoad]
    public static class NvimUnityLifecycle
    {
        static NvimUnityLifecycle()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        private static void OnBeforeAssemblyReload()
        {
            NvimUnityServer.StopServer();
            //UnityEngine.Debug.Log("[NvimUnity] Server stopped before assembly reload.");
        }
    }
}

