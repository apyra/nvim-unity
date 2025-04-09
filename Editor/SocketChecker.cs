using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace NvimUnity
{
    public static class SocketChecker
    {
        public static bool IsSocketActive(string socketPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return CheckNamedPipe(socketPath);
            }
            else
            {
                return CheckUnixSocket(socketPath);
            }
        }

        private static bool CheckNamedPipe(string pipePath)
        {
            // Ex: \\.\pipe\nvim-socket
            if (!pipePath.StartsWith(@"\\.\pipe\")) return false;

            string pipeName = pipePath.Replace(@"\\.\pipe\", "");
            try
            {
                using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
                client.Connect(100); // timeout de 100ms
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool CheckUnixSocket(string socketPath)
        {
            if (!File.Exists(socketPath)) return false;

            try
            {
                var endPoint = new UnixDomainSocketEndPoint(socketPath);
                using var client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
                var connectTask = client.ConnectAsync(endPoint);
                bool connected = connectTask.Wait(100); // timeout de 100ms
                return connected && client.Connected;
            }
            catch
            {
                return false;
            }
        }
    }
}

