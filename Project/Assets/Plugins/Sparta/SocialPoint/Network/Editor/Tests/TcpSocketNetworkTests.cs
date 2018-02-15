using System;
using NUnit.Framework;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    [TestFixture]
    [Category("SocialPoint.Network")]
    class TcpSocketNetworkTests : BaseSocketNetworkTests
    {
        Random _random = new Random();

        [SetUp]
        override protected void SetUp()
        {
            base.SetUp();

            var ip = "127.0.0.1";
            var port = _random.Next(3000, 5000);
            _scheduler = new UpdateScheduler();
            _server = new TcpSocketNetworkServer(_scheduler, ip, port);
            _client = new TcpSocketNetworkClient(_scheduler, ip, port);
            _client2 = new TcpSocketNetworkClient(_scheduler, ip, port);
            _server.AddDelegate(this);
            _client.AddDelegate(this);
            _client2.AddDelegate(this);
        }
    }
}
