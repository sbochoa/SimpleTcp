namespace SimpleTcp
{
    public class SimpleTcpServerCreator
    {
        private int _port;
        private OnServerStart _onStart;
        private int _bufferSize = 1024;
        private OnClientRequest _onClientRequest;
        private OnConnectionError _onConnectionError;
        private string _endOfFileTag;
        private OnClientConnected _onClientConnected;

        public SimpleTcpServer Create()
        {
            return new SimpleTcpServer(_port, _onStart, _bufferSize, _onClientConnected, _onConnectionError
                , _onClientRequest, _endOfFileTag);
        }

        public SimpleTcpServer Start()
        {
            var server = Create();

            server.Start();

            return server;
        }

        public SimpleTcpServerCreator WithPort(int port)
        {
            _port = port;
            return this;
        }

        public SimpleTcpServerCreator OnStart(OnServerStart onStart)
        {
            _onStart = onStart;
            return this;
        }

        public SimpleTcpServerCreator BufferSize(int bufferSize)
        {
            _bufferSize = bufferSize;
            return this;
        }

        public SimpleTcpServerCreator OnClientConnected(OnClientConnected onClientConnected)
        {
            _onClientConnected = onClientConnected;
            return this;
        }

        public SimpleTcpServerCreator OnClientRequest(OnClientRequest onClientRequest)
        {
            _onClientRequest = onClientRequest;
            return this;
        }

        public SimpleTcpServerCreator OnClientConnectionError(OnConnectionError onConnectionError)
        {
            _onConnectionError = onConnectionError;
            return this;
        }

        
        public SimpleTcpServerCreator WithEndOfFileTag(string endOfFileTag)
        {
            _endOfFileTag = endOfFileTag;
            return this;
        }

    }
}
