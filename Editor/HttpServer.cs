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
                try
                {
                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        string content = reader.ReadToEnd().Trim();

                        if (!string.IsNullOrEmpty(content) && content.Contains(":"))
                        {
                            var parts = content.Split(':');
                            string file = parts[0];
                            int line = int.TryParse(parts[1], out int parsedLine) ? parsedLine : 1;

                            if (FileOpener.OpenInRunningServer(file, line))
                            {
                                WriteResponse(context, 200, "Opened in Neovim");
                            }
                            else
                            {
                                WriteResponse(context, 500, "Failed to open in running Neovim");
                            }
                        }
                        else
                        {
                            WriteResponse(context, 400, "Invalid format. Use 'file:line'");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[NvimUnity] Exception in /open: {ex}");
                    WriteResponse(context, 500, "Internal server error");
                }
            }
            else if (path == "/regenerate" && method == "POST")
            {
                Utils.RegenerateProjectFiles();
                WriteResponse(context, 200, "Regenerated");
            }
            else
            {
                context.Response.StatusCode = 404;
                WriteResponse(context, 400, "Not Found");
            }
        }

        private void WriteResponse(HttpListenerContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
            context.Response.ContentLength64 = buffer.Length;
            using (Stream output = context.Response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }
        }

    }
}

