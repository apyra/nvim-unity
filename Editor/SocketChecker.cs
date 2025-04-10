using System;
using System.Net.Sockets;
using System.IO.Pipes;

namespace NvimUnity
{
    public static class SocketChecker
    {
        public static bool IsSocketActive(string socketPath)
        {
            if (string.IsNullOrWhiteSpace(socketPath))
                return false;

            try
            {
                if (FileOpener.OS() == "Windows")
                {
                    // Remover o prefixo \\.\pipe\ para passar só o nome do pipe
                    string pipeName = socketPath.Replace(@"\\.\pipe\", "");

                    using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
                    client.Connect(100); // timeout em milissegundos
                    return true;
                }
                else
                {
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

