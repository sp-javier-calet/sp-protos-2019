using System;
using NUnit.Framework;

namespace SocialPoint.Network
{
    [TestFixture]
    [Category("SocialPoint.Network")]
    class UdpSocketNetworkTests : BaseSocketNetworkTests
    {
        const int SleepThread = 10;

        Random _random = new Random();

        [SetUp]
        override protected void SetUp()
        {
            base.SetUp();
            var ip = "127.0.0.1";
            var port = _random.Next(3000, 5000);
            var peerLimit = 100;
            var connectionKey = "TestConnectionKey";
            var updateTime = 10;
            _server = new UdpSocketNetworkServer(_scheduler, peerLimit, connectionKey);
            _client = new UdpSocketNetworkClient(_scheduler, connectionKey, updateTime);
            _client2 = new UdpSocketNetworkClient(_scheduler, connectionKey, updateTime);

            (_server as UdpSocketNetworkServer).Port = port;
            (_client as UdpSocketNetworkClient).ServerAddress = ip;
            (_client2 as UdpSocketNetworkClient).ServerAddress = ip;
            (_client as UdpSocketNetworkClient).ServerPort = port;
            (_client2 as UdpSocketNetworkClient).ServerPort = port;

            _server.AddDelegate(this);
            _client.AddDelegate(this);
            _client2.AddDelegate(this);
        }

        [Test]
        public void SendUnreliableMessage()
        {
            var receiver1 = new TestMessageReceiver();
            var receiver2 = new TestMessageReceiver();
            var receiver3 = new TestMessageReceiver();
            _client.RegisterReceiver(receiver1);
            _client2.RegisterReceiver(receiver2);
            _server.RegisterReceiver(receiver3);

            _server.Start();

            _client.Connect();
            _client2.Connect();

            WaitForEvents();

            var data = new NetworkMessageData {
                MessageType = 5,
                Unreliable = true
            };

            var clientMsg = _client.CreateMessage(data);
            clientMsg.Writer.Write("test");
            clientMsg.Send();
            
            var serverMsg = _server.CreateMessage(data);
            serverMsg.Writer.Write("test");
            serverMsg.Send();

            WaitForEvents(NetworkDelegateType.MessageServerReceived, NetworkDelegateType.MessageClientReceived1, NetworkDelegateType.MessageClientReceived2);

            Assert.AreEqual(data.MessageType, receiver1.Data.MessageType);
            Assert.AreEqual(data.MessageType, receiver2.Data.MessageType);
            Assert.AreEqual(null, receiver1.Data.ClientIds);
            Assert.AreEqual(null, receiver2.Data.ClientIds);
            Assert.AreEqual("test", receiver1.Body);
            Assert.AreEqual("test", receiver2.Body);

            Assert.AreEqual(data.MessageType, receiver3.Data.MessageType);
            Assert.AreEqual(1, receiver3.Data.ClientIds[0]);
            Assert.AreEqual(1, receiver3.Data.ClientIds.Count);
            Assert.AreEqual("test", receiver3.Body);
        }
    }
}
