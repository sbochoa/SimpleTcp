using System.Net;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Shouldly;

namespace SimpleTcp.Tests
{
    [TestFixture]
    public class SimpleTcpServerTests
    {
        private const int Port = 8888;
        private static readonly ManualResetEvent Completion = new ManualResetEvent(false);
        private static readonly string IpAddress = IPAddress.Loopback.ToString();

        [SetUp]
        public void SetUp()
        {
            Completion.Reset();
        }

        [Test]
        public void Receive_Message_From_Client_Correctly()
        {
            var messageSentToServer = "This is not what it looks like";
            var messageSentToServerBytes = Encoding.ASCII.GetBytes(messageSentToServer);
            var messageReceived = string.Empty;

            var server = SimpleTcpServer.Configure()
                .WithPort(Port)
                .OnClientRequest((data, send) =>
                {
                    messageReceived = Encoding.ASCII.GetString(data);
                    Completion.Set();
                })
                .Start();


            var client = SimpleTcpClient.Configure()
                .WithEndpoint(IpAddress, server.Port)
                .Connect();

            client.Send(messageSentToServerBytes);

            Completion.WaitOne();

            messageSentToServer.ShouldBe(messageReceived);
            server.Stop();
        }

        [Test]
        public void Trigger_OnClientConnected_Event()
        {
            var onClientConnectedTriggerd = false;
            var server = SimpleTcpServer.Configure()
                .WithPort(Port)
                .OnClientConnected(tcpClient =>
                {
                    onClientConnectedTriggerd = true;
                    Completion.Set();
                })
                .Start();

            SimpleTcpClient.Configure()
                .WithEndpoint(IpAddress, server.Port)
                .Connect();

            Completion.WaitOne();

            onClientConnectedTriggerd.ShouldBe(true);
            server.Stop();
        }

        [Test]
        public void Trigger_OnStart_Event()
        {
            var onStartTriggered = false;
            var server = SimpleTcpServer.Configure()
                .WithPort(Port)
                .OnStart(tcpServer =>
                {
                    onStartTriggered = true;
                    Completion.Set();
                })
                .Start();

            Completion.WaitOne();

            onStartTriggered.ShouldBe(true);
            server.Stop();
        }

        [Test]
        public void Listen_Many_Clients_Correctly()
        {
            var expectedResult = string.Empty;
            var actualResult = string.Empty;
            var clientQuantity = 1000;
            var messagesReceived = 0;
            var lockObject = new object();

            var server = SimpleTcpServer.Configure()
                .WithPort(Port)
                .OnClientRequest((data, send) =>
                {
                    lock (lockObject)
                    {
                        var messageReceived = Encoding.ASCII.GetString(data);
                        actualResult += messageReceived;
                        send(Encoding.ASCII.GetBytes($"RECEIVED:{messageReceived}"));

                        messagesReceived++;

                        if (messagesReceived == clientQuantity)
                        {
                            Completion.Set();
                        }
                    }
                })
                .Start();


            for (var i = 0; i < clientQuantity; i++)
            {
                var client = SimpleTcpClient.Configure()
                .WithEndpoint(IpAddress, server.Port)
                .Connect();

                var message = $"{i}";
                expectedResult += message;

                var data = Encoding.ASCII.GetBytes(message);

                client.Send(data);
            }

            Completion.WaitOne();

            server.Stop();

            actualResult.Length.ShouldBe(expectedResult.Length);
        }
    }
}
