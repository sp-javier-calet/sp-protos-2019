using System;
using System.Collections;
using NUnit.Framework;
using NSubstitute;
using SocialPoint.Utils;
using System.Net.Sockets;

namespace SocialPoint.Network
{
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
        public void ReceivedNetworkMessageData()
        {
            var networkStream = _client.Client.GetStream();
            var msg = new SimpleSocketNetworkMessage(new NetworkMessageData{}, new LocalNetworkClient[0]);
            msg.Writer.Write("test");
            msg.Send();
            var reader = msg.Receive();
            Assert.That(reader.ReadString() == "test");
        }
    }
}
