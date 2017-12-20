using System;
using System.Collections;
using NUnit.Framework;
using NSubstitute;

namespace SocialPoint.Network
{
    [TestFixture]
    [Category("SocialPoint.Network")]
    class LocalNetworkTests : BaseNetworkTests
    {
        [SetUp]
        public void SetUp()
        {
            var localServer = new LocalNetworkServer();
            _server = localServer;
            _client = new LocalNetworkClient(localServer);
            _client2 = new LocalNetworkClient(localServer);
        }

        [Test]
        public void ReceivedNetworkMessageData()
        {
            var msg = new LocalNetworkMessage(new NetworkMessageData{}, new LocalNetworkClient[0]);
            msg.Writer.Write("test");
            msg.Send();
            var reader = msg.Receive();
            Assert.That(reader.ReadString() == "test");
        }


        [Test]
        public void ClientConnectBeforeServerStart()
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
        public void OnServerStartedCalledIfDelegateAddedAfterStart()
        {
            _server.Start();
            var sdlg = Substitute.For<INetworkServerDelegate>();
            _server.AddDelegate(sdlg);

            WaitForEvents();
            sdlg.Received(1).OnServerStarted();
        }
    }
}
