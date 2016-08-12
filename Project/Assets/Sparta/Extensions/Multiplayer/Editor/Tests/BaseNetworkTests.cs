using System;
using System.Collections;
using NUnit.Framework;
using NSubstitute;

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
            var sdlg = Substitute.For<INetworkServerDelegate>();
            _server.AddDelegate(sdlg);
            _server.Start();
            _client.Connect();

            var msg = _client.CreateMessage(new NetworkMessageDest {
                MessageType = 5,
                ChannelId = 1
            });

            msg.Writer.Write(42);
            msg.Writer.Write("test");

            msg.Send();

            WaitForEvents();
            sdlg.Received(1).OnMessageReceived(Arg.Any<byte>(),
                Arg.Is<ReceivedNetworkMessage>( rmsg => 
                    rmsg.MessageType == 5 &&
                    rmsg.ChannelId == 1 &&
                    rmsg.Reader.ReadInt32() == 42 &&
                    rmsg.Reader.ReadString() == "test"
            ));
        }

        [Test]
        public void SendMessageFromServerToClients()
        {
            var cdlg = Substitute.For<INetworkClientDelegate>();
            _client.AddDelegate(cdlg);
            _client2.AddDelegate(cdlg);
            _server.Start();
            _client.Connect();
            _client2.Connect();

            var msg = _server.CreateMessage(new NetworkMessageDest {
                MessageType = 5,
                ChannelId = 1
            });

            msg.Writer.Write(42);
            msg.Writer.Write("test");

            msg.Send();

            WaitForEvents();
            cdlg.Received(2).OnMessageReceived(Arg.Is<ReceivedNetworkMessage>( rmsg => 
                rmsg.MessageType == 5 &&
                rmsg.ChannelId == 1 &&
                rmsg.Reader.ReadInt32() == 42 &&
                rmsg.Reader.ReadString() == "test"
            ));
        }

        [Test]
        public void SendMessageFromServerToOneClient()
        {
            var cdlg1 = Substitute.For<INetworkClientDelegate>();
            var cdlg2 = Substitute.For<INetworkClientDelegate>();
            _client.AddDelegate(cdlg1);
            _client2.AddDelegate(cdlg2);
            _server.Start();
            _client.Connect();
            _client2.Connect();

            var msg = _server.CreateMessage(new NetworkMessageDest {
                MessageType = 5,
                ChannelId = 1,
                ClientId = 1
            });

            msg.Writer.Write(42);
            msg.Writer.Write("test");

            msg.Send();

            WaitForEvents();
            cdlg1.Received(1).OnMessageReceived(Arg.Is<ReceivedNetworkMessage>( rmsg => 
                rmsg.MessageType == 5 &&
                rmsg.ChannelId == 1 &&
                rmsg.Reader.ReadInt32() == 42 &&
                rmsg.Reader.ReadString() == "test"
            ));
            cdlg2.Received(0).OnMessageReceived(Arg.Any<ReceivedNetworkMessage>());
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
