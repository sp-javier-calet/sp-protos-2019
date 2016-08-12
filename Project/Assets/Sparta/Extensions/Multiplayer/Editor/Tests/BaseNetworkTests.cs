using System;
using System.Collections;
using NUnit.Framework;
using NSubstitute;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public abstract class BaseNetworkTests
    {
        protected INetworkServer _server;
        protected INetworkClient _client;
        protected INetworkClient _client2;

        virtual protected void WaitForEvents()
        {
        }

        [Test]
        public void ServerDelegateCalled()
        {
            var sdlg = Substitute.For<INetworkServerDelegate>();
            _server.AddDelegate(sdlg);
            _server.Start();
            _server.Start();

            WaitForEvents();
            sdlg.Received(1).OnStarted();

            _client.Connect();
            _client.Connect();

            WaitForEvents();
            sdlg.Received(1).OnClientConnected(1);

            _client.Disconnect();
            _client.Disconnect();

            WaitForEvents();
            sdlg.Received(1).OnClientDisconnected(1);

            _server.Stop();
            _server.Stop();

            WaitForEvents();
            sdlg.Received(1).OnStopped();
        }

        [Test]
        public void ClientDelegateCalled()
        {
            var cdlg = Substitute.For<INetworkClientDelegate>();
            _client.AddDelegate(cdlg);

            _server.Start();
            _server.Start();           

            _client.Connect();
            _client.Connect();

            WaitForEvents();
            cdlg.Received(1).OnConnected();

            _client.Disconnect();
            _client.Disconnect();

            WaitForEvents();
            cdlg.Received(1).OnDisconnected();
        }

        [Test]
        public void SendMessageFromClientToServer()
        {
            var receiver = Substitute.For<INetworkMessageReceiver>();
            _server.RegisterReceiver(receiver);
            _server.Start();
            _client.Connect();

            var data = new NetworkMessageData {
                MessageType = 5,
                ChannelId = 1
            };
            var msg = _client.CreateMessage(data);

            msg.Writer.Write(42);
            msg.Writer.Write("test");

            msg.Send();

            WaitForEvents();
            receiver.Received(1).OnMessageReceived(data,
                Arg.Is<IReader>( reader => 
                    reader.ReadInt32() == 42 &&
                    reader.ReadString() == "test"
            ));
        }

        [Test]
        public void SendMessageFromServerToClients()
        {
            var receiver = Substitute.For<INetworkMessageReceiver>();
            _client.RegisterReceiver(receiver);
            _client2.RegisterReceiver(receiver);
            _server.Start();
            _client.Connect();
            _client2.Connect();

            var data = new NetworkMessageData {
                MessageType = 5,
                ChannelId = 1
            };
            var msg = _server.CreateMessage(data);

            msg.Writer.Write(42);
            msg.Writer.Write("test");

            msg.Send();

            WaitForEvents();
            receiver.Received(2).OnMessageReceived(data,
                Arg.Is<IReader>( reader => 
                    reader.ReadInt32() == 42 &&
                    reader.ReadString() == "test"
            ));
        }

        [Test]
        public void SendMessageFromServerToOneClient()
        {
            var receiver1 = Substitute.For<INetworkMessageReceiver>();
            var receiver2 = Substitute.For<INetworkMessageReceiver>();
            _client.RegisterReceiver(receiver1);
            _client2.RegisterReceiver(receiver2);
            _server.Start();
            _client.Connect();
            _client2.Connect();

            var data = new NetworkMessageData {
                MessageType = 5,
                ChannelId = 1,
                ClientId = 1
            };
            var msg = _server.CreateMessage(data);

            msg.Writer.Write(42);
            msg.Writer.Write("test");

            msg.Send();

            WaitForEvents();
            receiver1.Received(1).OnMessageReceived(data,
                Arg.Is<IReader>( reader => 
                    reader.ReadInt32() == 42 &&
                    reader.ReadString() == "test"
            ));
            receiver2.Received(0).OnMessageReceived(Arg.Any<NetworkMessageData>(), Arg.Any<IReader>());
        }

        [Test]
        public void ClientConnectAfterServerStart()
        {
            var cdlg = Substitute.For<INetworkClientDelegate>();
            var sdlg = Substitute.For<INetworkServerDelegate>();
            _client.AddDelegate(cdlg);
            _server.AddDelegate(sdlg);
            _client.Connect();

            WaitForEvents();
            cdlg.Received(0).OnConnected();
            sdlg.Received(0).OnClientConnected(Arg.Any<byte>());

            _server.Start();

            WaitForEvents();
            cdlg.Received(1).OnConnected();
            sdlg.Received(1).OnClientConnected(Arg.Any<byte>());
        }

        [Test]
        public void ClientDisconnectOnServerStop()
        {
            var cdlg = Substitute.For<INetworkClientDelegate>();
            var sdlg = Substitute.For<INetworkServerDelegate>();
            _client.AddDelegate(cdlg);
            _server.AddDelegate(sdlg);

            _server.Start();
            _client.Connect();
            _server.Stop();

            WaitForEvents();
            cdlg.Received(1).OnDisconnected();
            sdlg.Received(1).OnClientDisconnected(Arg.Any<byte>());
        }

        [Test]
        public void OnServerStartedCalledIfDelegateAddedAfterStart()
        {
            _server.Start();
            var sdlg = Substitute.For<INetworkServerDelegate>();
            _server.AddDelegate(sdlg);

            WaitForEvents();
            sdlg.Received(1).OnStarted();
        }

        [Test]
        public void OnClientConnectedCalledIfDelegateAddedAfterConnect()
        {
            _server.Start();
            _client.Connect();
            var cdlg = Substitute.For<INetworkClientDelegate>();
            _client.AddDelegate(cdlg);

            WaitForEvents();
            cdlg.Received(1).OnConnected();
        }

    }
}
