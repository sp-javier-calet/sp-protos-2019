using System;
using System.Collections;
using NUnit.Framework;
using NSubstitute;
using SocialPoint.Utils;
using System.Net.Sockets;
using SocialPoint.IO;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace SocialPoint.Network
{
    class TestMessageReceiver : INetworkMessageReceiver
    {
        public NetworkMessageData Data;
        public string Body;

        public void OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            Data = data;
            Body = reader.ReadString();
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
            _server = new SimpleSocketNetworkServer(_scheduler, ip, port);
            _client = new SimpleSocketNetworkClient(_scheduler, ip, port);
            _client2 = new SimpleSocketNetworkClient(_scheduler, ip, port);
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

                Assert.Fail("no exception thrown");
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
            _client.Connect();


            NetworkMessageData data = new NetworkMessageData {
                MessageType = 5,
                ClientIds = new List<byte>(){ 1 }
            };

            var msg = _client.CreateMessage(data);
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
            var receiver = new TestMessageReceiver();
            _client.RegisterReceiver(receiver);
            _client2.RegisterReceiver(receiver);
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

            WaitForEvents();

            Assert.AreEqual(data.MessageType, receiver.Data.MessageType);
            Assert.AreEqual(1, receiver.Data.ClientIds[0]);
            Assert.AreEqual(2, receiver.Data.ClientIds[1]);
            Assert.AreEqual(2, receiver.Data.ClientIds.Count);
            Assert.AreEqual("test", receiver.Body);
//            receiver.Received(2).OnMessageReceived(data,
//                Arg.Is<IReader>( reader => 
//                    reader.ReadInt32() == 42 &&
//                    reader.ReadString() == "test"
//                ));
        }

    }
}
