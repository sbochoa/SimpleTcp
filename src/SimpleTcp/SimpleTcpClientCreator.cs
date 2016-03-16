using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTcp
{
    public class SimpleTcpClientCreator
    {
        private int _bufferSize = 1024;
        private OnServerResponse _onServerResponse;
        private OnConnectionError _onConnectionError;
        private string _hostName;
        private int _port;

        public SimpleTcpClient Create()
        {
            return new SimpleTcpClient(_hostName, _port, _bufferSize, _onServerResponse, _onConnectionError);
        }

        public SimpleTcpClient Connect()
        {
            var client = new SimpleTcpClient(_hostName, _port, _bufferSize, _onServerResponse, _onConnectionError);
            client.Connect();
            return client;
        }

        public SimpleTcpClientCreator WithEndpoint(string hostName, int port)
        {
            _hostName = hostName;
            _port = port;
            return this;
        }

        public SimpleTcpClientCreator WithBufferSize(int bufferSize)
        {
            _bufferSize = bufferSize;
            return this;
        }

        public SimpleTcpClientCreator OnServerResponse(OnServerResponse onServerResponse)
        {
            _onServerResponse = onServerResponse;
            return this;
        }

        public SimpleTcpClientCreator OnConnectionError(OnConnectionError onConnectionError)
        {
            _onConnectionError = onConnectionError;
            return this;
        }
    }
}
