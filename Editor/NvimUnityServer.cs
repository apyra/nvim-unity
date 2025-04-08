using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NvimUnity
{
    [InitializeOnLoad]
    public static class NvimUnityServer
    {
        private static HttpServer server;
        public static string ServerAddress = "http://localhost:42069/";
        private static Dictionary<string, string> terminalConfig;

        static NvimUnityServer()
        {
            StartServer();
        }

        public static void StartServer()
        {
            if (server != null)
            {
                Debug.Log("[NvimUnity] Server already running.");
                return;
            }

            try
            {
                server = new HttpServer(ServerAddress){};
                server.Start();

                Debug.Log($"[NvimUnity] Server started at {ServerAddress}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NvimUnity] Failed to start server: {ex.Message}");
            }
        }

        public static void StopServer()
        {
            if (server != null)
            {
                server.Stop();
                server = null;
                Debug.Log("[NvimUnity] Server stopped.");
            }
        }

        public static string GetStatus()
        {
            return server == null ? "Stopped" : $"Running at {ServerAddress}";
        }
    }
}

