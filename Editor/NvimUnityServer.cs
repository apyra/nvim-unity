using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class NvimUnityServer
{
    private static HttpListener _listener;
    private static Thread _listenerThread;
    private static bool _isRunning = false;

    static NvimUnityServer()
    {
        StartServer();
    }

    public static void StartServer()
    {
        if (_isRunning) return;

        _isRunning = true;
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:5005/");

        _listenerThread = new Thread(() =>
        {
            _listener.Start();
            Debug.Log("[nvim-unity] HTTP Server started on http://localhost:5005");

            while (_isRunning)
            {
                try
                {
                    var context = _listener.GetContext();
                    HandleRequest(context);
                }
                catch { }
            }
        });

        _listenerThread.Start();
    }

    private static void HandleRequest(HttpListenerContext context)
    {
        string path = context.Request.Url.AbsolutePath;
        if (path == "/regenerate")
        {
            Debug.Log("[nvim-unity] Regenerate command received from Neovim.");
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            UnityEditorInternal.InternalEditorUtility.RequestScriptReload();
            RegenerateProjectFiles();
        }

        string response = "OK";
        byte[] buffer = Encoding.UTF8.GetBytes(response);
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();
    }

    private static void RegenerateProjectFiles()
    {
        // Igual ao botão da UI
        SyncHelper.RegenerateProjectFiles();
    }

    public static void StopServer()
    {
        _isRunning = false;
        _listener?.Stop();
        _listenerThread?.Abort();
        Debug.Log("[nvim-unity] HTTP Server stopped.");
    }
}

