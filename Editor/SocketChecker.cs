using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Pipes;

namespace NvimUnity
{
    public static class SocketChecker
    {
        public static bool IsSocketActive(string socketPath)
        {
            if (string.IsNullOrWhiteSpace(socketPath) || !File.Exists(socketPath))
                return false;

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Named pipe em Windows
                    using var client = new NamedPipeClientStream(".", socketPath, PipeDirection.InOut);
                    client.Connect(100); // tenta conectar por 100ms
                    return true;
                }
                else
                {
                    // Unix socket
                    var endPoint = new UnixDomainSocketEndPoint(socketPath);
                    using var sock = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
                    sock.Connect(endPoint);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}

