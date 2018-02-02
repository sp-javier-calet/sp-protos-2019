using NUnit.Framework;
using NSubstitute;
using SocialPoint.IO;
using System.Collections.Generic;
using System;

namespace SocialPoint.Network
{
    class TestMessageReceiver : INetworkMessageReceiver
    {
        public bool Received;
        public NetworkMessageData Data;
        public string Body;

        public void OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            Received = true;
            Data = data;
            Body = reader.ReadString();
        }
    }

    public abstract class BaseNetworkTests
    {
        protected enum NetworkDelegateType
        {
            //Server
            ServerStarted,
            ServerStopped,
            ClientConnectedInServer,
            ClientDisconectedInServer,
            MessageServerReceived,
            //Client 1
            ClientConnected1,
            ClientDisconnected1,
            MessageClientReceived1,
            //Client 2
            ClientConnected2,
            ClientDisconnected2,
            MessageClientReceived2
        }
        protected INetworkServer _server;
        protected INetworkClient _client;
        protected INetworkClient _client2;

        virtual protected void WaitForEvents(params NetworkDelegateType[] typeEvent)
        {
        }

        [TearDown]
        public void CleanUp()
        {
            var dclient = _client as IDisposable;
            if(dclient != null)
            {
                dclient.Dispose();
            }
            var dclient2 = _client2 as IDisposable;
            if(dclient2 != null)
            {
                dclient2.Dispose();
            }
            var dserver = _server as IDisposable;
            if(dserver != null)
            {
                dserver.Dispose();
            }
        }

        [Test]
        public void ServerDelegateCalled()
        {
            var sdlg = Substitute.For<INetworkServerDelegate>();
            _server.AddDelegate(sdlg);
            _server.Start();

            WaitForEvents(NetworkDelegateType.ServerStarted);
            sdlg.Received(1).OnServerStarted();

            _client.Connect();

            WaitForEvents(NetworkDelegateType.ClientConnectedInServer);
            sdlg.Received(1).OnClientConnected(1);

            _client.Disconnect();

            WaitForEvents(NetworkDelegateType.ClientDisconectedInServer);
            sdlg.Received(1).OnClientDisconnected(1);

            _server.Stop();

            WaitForEvents(NetworkDelegateType.ServerStopped);
            sdlg.Received(1).OnServerStopped();
        }

        [Test]
        public void ClientDelegateCalled()
        {
            var cdlg = Substitute.For<INetworkClientDelegate>();
            _client.AddDelegate(cdlg);

            _server.Start();

            _client.Connect();

            WaitForEvents(NetworkDelegateType.ClientConnected1);
            cdlg.Received(1).OnClientConnected();

            _client.Disconnect();

            WaitForEvents(NetworkDelegateType.ClientDisconnected1);
            cdlg.Received(1).OnClientDisconnected();
        }

        [Test]
        public void SendMessageFromClientToServer()
        {
            var receiver = new TestMessageReceiver();
            _server.RegisterReceiver(receiver);

            _server.Start();
            _client.Connect();

            WaitForEvents();

            NetworkMessageData data = new NetworkMessageData {
                MessageType = 5,
                ClientIds = new List<byte>(){ 1 }
            };

            var msg = _client.CreateMessage(data);
            msg.Writer.Write("test");
            msg.Send();

            WaitForEvents(NetworkDelegateType.MessageClientReceived1);

            Assert.AreEqual(data.MessageType, receiver.Data.MessageType);
            Assert.AreEqual(1, receiver.Data.ClientIds[0]);
            Assert.AreEqual(1, receiver.Data.ClientIds.Count);
            Assert.AreEqual("test", receiver.Body);
        }

        [Test]
        public void SendMessageFromServerToClients()
        {
            var receiver1 = new TestMessageReceiver();
            var receiver2 = new TestMessageReceiver();

            _client.RegisterReceiver(receiver1);
            _client2.RegisterReceiver(receiver2);

            _server.Start();
            _client.Connect();
            _client2.Connect();

            WaitForEvents();

            var data = new NetworkMessageData {
                MessageType = 5
            };

            var msg = _server.CreateMessage(data);
            msg.Writer.Write("test");
            msg.Send();

            WaitForEvents(NetworkDelegateType.MessageClientReceived1,NetworkDelegateType.MessageClientReceived2);

            Assert.AreEqual(data.MessageType, receiver1.Data.MessageType);
            Assert.AreEqual(data.MessageType, receiver2.Data.MessageType);
            Assert.AreEqual(null, receiver1.Data.ClientIds);
            Assert.AreEqual(null, receiver2.Data.ClientIds);
            Assert.AreEqual("test", receiver1.Body);
            Assert.AreEqual("test", receiver2.Body);
        }

        [Test]
        public void SendMessageFromServerToOneClient()
        {
            var receiver1 = new TestMessageReceiver();
            var receiver2 = new TestMessageReceiver();

            _client.RegisterReceiver(receiver1);
            _client2.RegisterReceiver(receiver2);

            _server.Start();
            _client.Connect();
            _client2.Connect();

            WaitForEvents();

            var data = new NetworkMessageData {
                MessageType = 5,
                ClientIds = new List<byte>(){ 1 }
            };
            var msg = _server.CreateMessage(data);

            msg.Writer.Write("test");

            msg.Send();

            WaitForEvents(NetworkDelegateType.MessageClientReceived1, NetworkDelegateType.MessageClientReceived2);

            Assert.AreEqual(data.MessageType, receiver1.Data.MessageType);
            Assert.AreEqual(null, receiver1.Data.ClientIds);
            Assert.IsTrue(receiver1.Received);
            Assert.AreEqual("test", receiver1.Body);

            Assert.IsFalse(receiver2.Received);
            Assert.AreEqual(null, receiver2.Body);
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

            WaitForEvents(NetworkDelegateType.ClientConnected1,NetworkDelegateType.ClientConnectedInServer);

            sdlg.Received(1).OnClientConnected(Arg.Any<byte>());
            cdlg.Received(1).OnClientConnected();
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
            WaitForEvents(NetworkDelegateType.ClientDisconnected1,NetworkDelegateType.ClientDisconectedInServer);
            sdlg.Received(1).OnClientDisconnected(Arg.Any<byte>());
            cdlg.Received(1).OnClientDisconnected();
        }

      

        [Test]
        public void OnClientConnectedCalledIfDelegateAddedAfterConnect()
        {
            _server.Start();
            _client.Connect();
            var cdlg = Substitute.For<INetworkClientDelegate>();
            _client.AddDelegate(cdlg);

            WaitForEvents(NetworkDelegateType.ClientConnected1);
            cdlg.Received(1).OnClientConnected();
        }

        [Test]
        public void ClientReceivedMessageSendOnClientConnected()
        {
            var sdlg = Substitute.For<INetworkServerDelegate>();
            _server.Start();
            sdlg.WhenForAnyArgs(x => x.OnClientConnected(Arg.Any<byte>())).Do(x => _server.SendMessage(new NetworkMessageData {
                ClientIds = new List<byte>(){ 1 },
                MessageType = 1
            }, Substitute.For<INetworkShareable>()));
            _server.AddDelegate(sdlg);
            var cdlg = Substitute.For<INetworkClientDelegate>();
            _client.AddDelegate(cdlg);
            _client.Connect();

            WaitForEvents(NetworkDelegateType.MessageClientReceived1);

            cdlg.Received().OnMessageReceived(new NetworkMessageData {
                ClientIds = new List<byte>(){ 1 },
                MessageType = 1
            });
        }
    }
}
