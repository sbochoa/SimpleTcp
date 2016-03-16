using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SimpleTcp
{
    public class SimpleTcpClient : ITcpClient
    {
        private readonly string _hostName;
        private readonly int _port;
        private TcpClient _tcpClient;
        private readonly int _bufferSize;
        private readonly OnServerResponse _onServerResponse;
        private readonly OnConnectionError _onConnectionError;
        private readonly IList<Task> _tasks;

        public SimpleTcpClient(string hostName, int port, int bufferSize, OnServerResponse onServerResponse, OnConnectionError onConnectionError)
        {
            _hostName = hostName;
            _port = port;
            _bufferSize = bufferSize;
            _onServerResponse = onServerResponse;
            _onConnectionError = onConnectionError;
            _tasks = new List<Task>();
        }

        public static SimpleTcpClientCreator Configure()
        {
            return new SimpleTcpClientCreator();
        }

        public void Close()
        {
            _tcpClient.Close();
        }

        public void Connect()
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect(_hostName, _port);

            StartListen();
        }

        private void StartListen()
        {
            _tasks.Add(BeginReceivingFromAsync(_tcpClient));
        }

        private async Task BeginReceivingFromAsync(TcpClient tcpClient)
        {
            try
            {
                var stream = tcpClient.GetStream();
                var buffer = new byte[_bufferSize];
                var memoryStream = new MemoryStream();

                while (tcpClient.Connected)
                {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    memoryStream.Write(buffer, 0, bytesRead);

                    if (!stream.DataAvailable)
                    {
                        _onServerResponse?.Invoke(memoryStream.ToArray());
                        ClearMemoryStream(memoryStream);
                    }
                }
            }
            catch (Exception ex)
            {
                _onConnectionError?.Invoke(ex);
                throw;
            }
        }

        private static void ClearMemoryStream(MemoryStream memoryStream)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.SetLength(0);
        }

        public void Send(byte[] data)
        {
            _tcpClient.GetStream().Write(data, 0, data.Length);
        }

        public async Task SendAsync(byte[] data)
        {
            await _tcpClient.GetStream().WriteAsync(data, 0, data.Length);
        }
    }
}
