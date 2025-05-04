using System;
using System.Net.Sockets;
using System.IO.Pipes;
using UnityEngine; // To use Debug.Log

namespace NvimUnity
{
    public static class SocketChecker
    {
        public static bool IsSocketActive(string socketPath)
        {
            if (string.IsNullOrWhiteSpace(socketPath))
            {
                return false;
            }

            try
            {
                if (NeovimEditor.OS == "Windows")
                {
                    string pipeName = socketPath.Replace(@"\\.\pipe\", "").Replace(@"\", "");
                    using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                    client.Connect(100);
                    return client.IsConnected;
                }
                else
                {
                    if (!System.IO.File.Exists(socketPath))
                    {
						Debug.Log($"[NvimUnity] Socket file does not exist: {socketPath}");
                        return false;
                    }

                    var endPoint = new UnixDomainSocketEndPoint(socketPath);
                    using var sock = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                    sock.Connect(endPoint);
                    return sock.Connected;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}

