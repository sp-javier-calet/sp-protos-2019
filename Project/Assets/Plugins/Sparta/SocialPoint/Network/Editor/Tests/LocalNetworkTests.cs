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

    }
}
