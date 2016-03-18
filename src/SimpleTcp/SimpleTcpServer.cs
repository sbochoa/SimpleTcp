using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleTcp
{

    public class SimpleTcpServer
    {
        private TcpListener _listener;
        private readonly IList<TcpClient> _activeClients;
        private readonly OnServerStart _onStart;
        private readonly int _bufferSize;
        private readonly OnClientConnected _onClientConnected;
        private readonly OnConnectionError _onConnectionError;
        private readonly OnClientRequest _onClientRequest;
        private readonly string _endOfFileTag;
        private volatile bool _stopping;
        
        private readonly IList<Task> _tasks;
        public IEnumerable<TcpClient> ActiveClients => _activeClients;

        public int Port { get; }
        public SimpleTcpServer(int port, OnServerStart onStart, int bufferSize, OnClientConnected onClientConnected
            , OnConnectionError onConnectionError, OnClientRequest onClientRequest, string endOfFileTag)
        {
            _onStart = onStart;
            _bufferSize = bufferSize;
            _onClientConnected = onClientConnected;
            _onConnectionError = onConnectionError;
            _onClientRequest = onClientRequest;
            _endOfFileTag = endOfFileTag;
            Port = port;
            _activeClients = new List<TcpClient>();
            _tasks = new List<Task>();
        }

        public static SimpleTcpServerCreator Configure()
        {
            return new SimpleTcpServerCreator();
        }

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();

            _onStart?.Invoke(_listener);

            StartListen();
        }

        private void StartListen()
        {
            _tasks.Add(ListenAsync());
        }

        private async Task ListenAsync()
        {
            while (true)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();

                    _activeClients.Add(client);
                    _onClientConnected?.Invoke(client);

                    _tasks.Add(BeginReceivingFromAsync(client));
                }
                catch (ObjectDisposedException) when(_stopping)
                {
                    return;
                }
            }
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
                    try
                    {
                        await BeginReceivingFromCoreAsync(stream, buffer, memoryStream);
                    }
                    catch (ObjectDisposedException) when(_stopping)
                    {
                        return;
                    }
                    
                }
            }
            catch(Exception ex)
            {
                _onConnectionError?.Invoke(ex);
                throw;
            }
        }

        private async Task BeginReceivingFromCoreAsync(NetworkStream stream, byte[] buffer, MemoryStream memoryStream)
        {
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            memoryStream.Write(buffer, 0, bytesRead);

            var memoryStreamBytes = memoryStream.ToArray();

            if (IsEndOfFileTagConfigured())
            {
                var message = Encoding.ASCII.GetString(memoryStreamBytes);

                if (message.IndexOf(_endOfFileTag, StringComparison.Ordinal) > -1)
                {
                    _onClientRequest?.Invoke(memoryStreamBytes, data => SendResponse(stream, data));
                    ClearMemoryStream(memoryStream);
                }
            }
            else if (!stream.DataAvailable)
            {
                _onClientRequest?.Invoke(memoryStreamBytes, data => SendResponse(stream, data));
                ClearMemoryStream(memoryStream);
            }
        }

        private static void ClearMemoryStream(MemoryStream memoryStream)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.SetLength(0);
        }

        private void SendResponse(NetworkStream stream, byte[] data)
        {
            if (data != null)
            {
                stream.Write(data, 0, data.Length);
            }
        }

        public void Stop()
        {
            _stopping = true;

            _listener.Stop();

            foreach (var activeClient in _activeClients)
            {
                activeClient.GetStream().Close();
            }

            Task.WhenAll(_tasks).Wait();
            _activeClients.Clear();
        }

        private bool IsEndOfFileTagConfigured()
        {
            return !string.IsNullOrEmpty(_endOfFileTag);
        }
    }
}
