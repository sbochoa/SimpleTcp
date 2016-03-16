using System;
using System.Net.Sockets;

namespace SimpleTcp
{
    public delegate void SendResponse(byte[] data);

    public delegate void OnClientConnected(TcpClient client);

    public delegate void OnServerStart(TcpListener server);

    public delegate void OnClientRequest(byte[] data, SendResponse sendResponse);

    public delegate void OnConnectionError(Exception exception);

    public delegate void OnServerResponse(byte[] data);
}
