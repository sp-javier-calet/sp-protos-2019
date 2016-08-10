using System;
using System.Collections;
using NUnit.Framework;
using NSubstitute;

namespace SocialPoint.Multiplayer
{
    [TestFixture]
    [Category("SocialPoint.Multiplayer")]
    class LocalNetworkTests
    {
        LocalNetworkServer _server;
        LocalNetworkClient _client;
        LocalNetworkClient _client2;

        [SetUp]
        public void SetUp()
        {
            _server = new LocalNetworkServer();
            _client = new LocalNetworkClient(_server);
            _client2 = new LocalNetworkClient(_server);
        }

        [Test]
        public void ReceivedNetworkMessageData()
        {
            var msg = new LocalNetworkMessage(new NetworkMessageInfo{}, new LocalNetworkClient[0]);
            msg.Writer.Write("test");
            msg.Send();
            var rmsg = msg.Receive();
            Assert.That(rmsg.Reader.ReadString() == "test");
        }

        [Test]
        public void ServerDelegateCalled()
        {
            var sdlg = Substitute.For<INetworkServerDelegate>();
            _server.AddDelegate(sdlg);
            _server.Start();

            sdlg.Received(1).OnStarted();

            _client.Connect();

            sdlg.Received(1).OnClientConnected(1);

            _client.Disconnect();

            sdlg.Received(1).OnClientDisconnected(1);


            _server.Stop();

            sdlg.Received(1).OnStopped();
        }

        [Test]
        public void ClientDelegateCalled()
        {
            var cdlg = Substitute.For<INetworkClientDelegate>();
            _client.AddDelegate(cdlg);

            _server.Start();

            _client.Connect();

            cdlg.Received(1).OnConnected();

            _client.Disconnect();

            cdlg.Received(1).OnDisconnected();
        }

        [Test]
        public void SendMessageFromClientToServer()
        {
            var sdlg = Substitute.For<INetworkServerDelegate>();
            _server.AddDelegate(sdlg);
            _server.Start();
            _client.Connect();

            var msg = _client.CreateMessage(new NetworkMessageInfo {
                MessageType = 5,
                ChannelId = 1
            });

            msg.Writer.Write(42);
            msg.Writer.Write("test");

            msg.Send();

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

            var msg = _server.CreateMessage(new NetworkMessageInfo {
                MessageType = 5,
                ChannelId = 1
            });

            msg.Writer.Write(42);
            msg.Writer.Write("test");

            msg.Send();

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

            var msg = _server.CreateMessage(new NetworkMessageInfo {
                MessageType = 5,
                ChannelId = 1,
                ClientId = 1
            });

            msg.Writer.Write(42);
            msg.Writer.Write("test");

            msg.Send();

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

            cdlg.Received(0).OnConnected();
            sdlg.Received(0).OnClientConnected(Arg.Any<byte>());

            _server.Start();

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

            cdlg.Received(1).OnDisconnected();
            sdlg.Received(1).OnClientDisconnected(Arg.Any<byte>());
        }

    }
}
