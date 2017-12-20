using System;
using System.Collections;
using NUnit.Framework;
using NSubstitute;
using SocialPoint.IO;
using System.Collections.Generic;

namespace SocialPoint.Network
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

            WaitForEvents();
            sdlg.Received(1).OnServerStarted();

            _client.Connect();

            WaitForEvents();
            sdlg.Received(1).OnClientConnected(1);

            _client.Disconnect();

            WaitForEvents();
            sdlg.Received(1).OnClientDisconnected(1);

            _server.Stop();

            WaitForEvents();
            sdlg.Received(1).OnServerStopped();
        }

        [Test]
        public void ClientDelegateCalled()
        {
            var cdlg = Substitute.For<INetworkClientDelegate>();
            _client.AddDelegate(cdlg);

            _server.Start();

            _client.Connect();

            WaitForEvents();
            cdlg.Received(1).OnClientConnected();

            _client.Disconnect();

            WaitForEvents();
            cdlg.Received(1).OnClientDisconnected();
        }

        [Test]
        public virtual void SendMessageFromClientToServer()
        {
            var receiver = Substitute.For<INetworkMessageReceiver>();
            _server.RegisterReceiver(receiver);
            _server.Start();
            _client.Connect();

            var msg = _client.CreateMessage(new NetworkMessageData {
                MessageType = 5,
            });

            msg.Writer.Write(42);
            msg.Writer.Write("test");

            msg.Send();

            WaitForEvents();
            receiver.Received(1).OnMessageReceived(
                Arg.Is<NetworkMessageData>( data => 
                    data.MessageType == 5),
                Arg.Is<IReader>( reader => 
                    reader.ReadInt32() == 42 &&
                    reader.ReadString() == "test"
            ));
        }

        [Test]
        public virtual void SendMessageFromServerToClients()
        {
            var receiver = Substitute.For<INetworkMessageReceiver>();
            _client.RegisterReceiver(receiver);
            _client2.RegisterReceiver(receiver);
            _server.Start();
            _client.Connect();
            _client2.Connect();

            var data = new NetworkMessageData {
                MessageType = 5
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
                ClientIds = new List<byte>(){ 1 }
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

            _server.Start();
            _client.Connect();

            WaitForEvents();

            cdlg.Received(1).OnClientConnected();
            sdlg.Received(1).OnClientConnected(Arg.Any<byte>());

            _server.Stop();
        }

        [Test]
        public virtual void ClientConnectBeforeServerStart()
        {
            var cdlg = Substitute.For<INetworkClientDelegate>();
            var sdlg = Substitute.For<INetworkServerDelegate>();
            _client.AddDelegate(cdlg);
            _server.AddDelegate(sdlg);
            _client.Connect();

            WaitForEvents();
            cdlg.Received(0).OnClientConnected();
            sdlg.Received(0).OnClientConnected(Arg.Any<byte>());

            _server.Start();

            WaitForEvents();
            cdlg.Received(1).OnClientConnected();
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

            WaitForEvents();
            _server.Stop();

            WaitForEvents();
//            cdlg.Received(1).OnClientDisconnected();
            sdlg.Received(1).OnClientDisconnected(Arg.Any<byte>());
        }

        [Test]
        public void OnServerStartedCalledIfDelegateAddedAfterStart()
        {
            _server.Start();
            var sdlg = Substitute.For<INetworkServerDelegate>();
            _server.AddDelegate(sdlg);

            WaitForEvents();
            sdlg.Received(1).OnServerStarted();
        }

        [Test]
        public void OnClientConnectedCalledIfDelegateAddedAfterConnect()
        {
            _server.Start();
            _client.Connect();
            var cdlg = Substitute.For<INetworkClientDelegate>();
            _client.AddDelegate(cdlg);

            WaitForEvents();
            cdlg.Received(1).OnClientConnected();
        }

        [Test]
        public void ClientReceivedMessageSendOnClientConnected()
        {
            _server.Start();
            var sdlg = Substitute.For<INetworkServerDelegate>();
            sdlg.WhenForAnyArgs(x => x.OnClientConnected(Arg.Any<byte>())).Do(x => _server.SendMessage(new NetworkMessageData { ClientIds = new List<byte>(){ 1 }, MessageType = 1} , Substitute.For<INetworkShareable>()));
            _server.AddDelegate(sdlg);
            var cdlg = Substitute.For<INetworkClientDelegate>();
            _client.AddDelegate(cdlg);
            _client.Connect();
            cdlg.Received().OnMessageReceived(new NetworkMessageData {ClientIds = new List<byte>(){ 1 }, MessageType = 1});
        }
    }
}
