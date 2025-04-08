using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace NvimUnity
{
    public class HttpServer
    {
        private HttpListener _listener;
        private Thread _listenerThread;
        private bool _isRunning;

        public string Address { get; private set; }

        public HttpServer(string address)
        {
            Address = address;
        }

        public void Start()
        {
            if (_isRunning) return;

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(Address);
                _listener.Start();

                _isRunning = true;
                _listenerThread = new Thread(HandleRequests);
                _listenerThread.Start();

                //Debug.Log($"[NvimUnity] HTTP server started at {Address}");
            }
            catch (Exception e)
            {
                //Debug.LogError($"[NvimUnity] Failed to start HTTP server: {e.Message}");
            }
        }

        public void Stop()
        {
            try
            {
                _isRunning = false;
                _listener?.Stop();
                _listenerThread?.Join();
                //Debug.Log("[NvimUnity] HTTP server stopped.");
            }
            catch (Exception e)
            {
                //Debug.LogError($"[NvimUnity] Error stopping HTTP server: {e.Message}");
            }
        }

        private void HandleRequests()
        {
            while (_isRunning)
            {
                try
                {
                    var context = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(o => ProcessRequest(context));
                }
                catch (Exception e)
                {
                    if (_isRunning)
                        Debug.LogWarning($"[NvimUnity] Request handling failed: {e.Message}");
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath;
            string method = context.Request.HttpMethod;
            string responseText = "OK";

            if (path == "/status")
            {
                responseText = "OK";
            }
            else if (path == "/open" && method == "POST")
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                var data = reader.ReadToEnd();
                var parts = data.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out int line))
                {
                    string file = parts[0];
                    FileOpener.OpenFile(file, line);
                    responseText = $"Opening {file}:{line}";
                }
                else
                {
                    responseText = "Invalid input";
                    context.Response.StatusCode = 400;
                }
            }
            else if (path == "/regenerate" && method == "POST")
            {
                Utils.RegenerateProjectFiles();
                responseText = "Regenerated";
            }
            else
            {
                context.Response.StatusCode = 404;
                responseText = "Not Found";
            }

            byte[] buffer = Encoding.UTF8.GetBytes(responseText);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }
    }
}

