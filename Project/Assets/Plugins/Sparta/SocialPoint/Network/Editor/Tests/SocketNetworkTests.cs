using System;
using NUnit.Framework;
using NSubstitute;
using SocialPoint.Utils;
using SocialPoint.IO;
using System.Collections.Generic;
using SocialPoint.Network;

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

    class TestClientDelegate : INetworkClientDelegate
    {
        public NetworkMessageData Data;
        public bool Connected;
        public bool Disconnected;
        public SocialPoint.Base.Error Error;

        public void OnClientConnected()
        {
            Connected = true;
        }

        public void OnClientDisconnected()
        {
            Disconnected = true;
        }

        public void OnMessageReceived(NetworkMessageData data)
        {
            Data = data;
        }

        public void OnNetworkError(SocialPoint.Base.Error err)
        {
            Error = err;
        }
    }

    class TestServerDelegate : INetworkServerDelegate
    {
        public byte ClientId;
        public NetworkMessageData Data;
        public bool Connected;
        public bool Disconnected;
        public bool Started;
        public bool Stopped;
        public SocialPoint.Base.Error Error;

       
        public void OnServerStarted()
        {
            Started = true;
        }

        public void OnServerStopped()
        {
            Stopped = true;
        }

        public void OnClientConnected(byte clientId)
        {
            Connected = true;
            ClientId = clientId;
        }

        public void OnClientDisconnected(byte clientId)
        {
            Disconnected = true;
            ClientId = clientId;
        }

        public void OnMessageReceived(NetworkMessageData data)
        {
            Data = data;
        }

        public void OnNetworkError(SocialPoint.Base.Error err)
        {
            Error = err;
        }
    }

    [TestFixture]
    [Category("SocialPoint.Network")]
    class SocketNetworkTests : BaseNetworkTests
    {
        UpdateScheduler _scheduler;

        [SetUp]
        public void SetUp()
        {
            var ip = "127.0.0.1";
            var random = new Random();
            var port = random.Next(3000, 5000);
            _scheduler = new UpdateScheduler();
            _server = new TcpSocketNetworkServer(_scheduler, ip, port);
            _client1 = new TcpSocketNetworkClient(_scheduler, ip, port);
            _client2 = new TcpSocketNetworkClient(_scheduler, ip, port);
        }

        override protected void WaitForEvents()
        {
            _scheduler.Update(100, 100);
        }

        [Test]
        public override void ClientConnectBeforeServerStart()
        {
            Exception expectedExcetpion = null;
            try
            {
                var cdlg = Substitute.For<INetworkClientDelegate>();
                var sdlg = Substitute.For<INetworkServerDelegate>();
                _client1.AddDelegate(cdlg);
                _server.AddDelegate(sdlg);
                _client1.Connect();

                WaitForEvents();
                cdlg.Received(0).OnClientConnected();
                sdlg.Received(0).OnClientConnected(Arg.Any<byte>());

                _server.Start();

                WaitForEvents();
                cdlg.Received(1).OnClientConnected();
                sdlg.Received(1).OnClientConnected(Arg.Any<byte>());

                Assert.Fail("No exception thrown");
            }
            catch(Exception e)
            {
                expectedExcetpion = e;
            }

            Assert.IsNotNull(expectedExcetpion);
        }

        [Test]
        public override void SendMessageFromClientToServer()
        {
            var receiver = new TestMessageReceiver();
            _server.Start();
            _server.RegisterReceiver(receiver);
            _client1.Connect();


            NetworkMessageData data = new NetworkMessageData {
                MessageType = 5,
                ClientIds = new List<byte>(){ 1 }
            };

            var msg = _client1.CreateMessage(data);
            msg.Writer.Write("test");
            msg.Send();

            WaitForEvents();

            Assert.AreEqual(data.MessageType, receiver.Data.MessageType);
            Assert.AreEqual(1, receiver.Data.ClientIds[0]);
            Assert.AreEqual(1, receiver.Data.ClientIds.Count);
            Assert.AreEqual("test", receiver.Body);
        }

        [Test]
        public override void SendMessageFromServerToClients()
        {
            var receiver1 = new TestMessageReceiver();
            var receiver2 = new TestMessageReceiver();
            _client1.RegisterReceiver(receiver1);
            _client2.RegisterReceiver(receiver2);
            _server.Start();

            _client1.Connect();
            _client2.Connect();

            WaitForEvents();

            var data = new NetworkMessageData {
                MessageType = 5
            };
            var msg = _server.CreateMessage(data);

            msg.Writer.Write("test");

            msg.Send();

            WaitForEvents();

            Assert.AreEqual(data.MessageType, receiver1.Data.MessageType);
            Assert.AreEqual(data.MessageType, receiver2.Data.MessageType);
            Assert.AreEqual(null, receiver1.Data.ClientIds);
            Assert.AreEqual(null, receiver2.Data.ClientIds);
            Assert.AreEqual("test", receiver1.Body);
            Assert.AreEqual("test", receiver2.Body);
        }

        [Test]
        public override void OnServerStartedCalledIfDelegateAddedAfterStart()
        {
            
            _server.Start();
            var sdlg = Substitute.For<INetworkServerDelegate>();
            _server.AddDelegate(sdlg);

            WaitForEvents();
            sdlg.DidNotReceive().OnServerStarted();
          
        }


        [Test]
        public override void SendMessageFromServerToOneClient()
        {
            var receiver1 = new TestMessageReceiver();
            var receiver2 = new TestMessageReceiver();
            _client1.RegisterReceiver(receiver1);
            _client2.RegisterReceiver(receiver2);
            _server.Start();
            _client1.Connect();
            _client2.Connect();

            WaitForEvents();

            var data = new NetworkMessageData {
                MessageType = 5,
                ClientIds = new List<byte>(){ 1 }
            };
            var msg = _server.CreateMessage(data);

            msg.Writer.Write("test");

            msg.Send();

            WaitForEvents();

            Assert.AreEqual(data.MessageType, receiver1.Data.MessageType);
            Assert.AreEqual(null, receiver1.Data.ClientIds);
            Assert.IsTrue(receiver1.Received);
            Assert.AreEqual("test", receiver1.Body);

            Assert.IsFalse(receiver2.Received);
            Assert.AreEqual(null, receiver2.Body);

        }

    }
}
