using System;
using NUnit.Framework;
using NSubstitute;
using SocialPoint.Utils;
using SocialPoint.IO;
using System.Collections.Generic;
using SocialPoint.Network;

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
            _server = new TcpSocketNetworkServer(_scheduler, ip, port);
            _client = new TcpSocketNetworkClient(_scheduler, ip, port);
            _client2 = new TcpSocketNetworkClient(_scheduler, ip, port);
        }

        override protected void WaitForEvents()
        {
            _scheduler.Update(100, 100);
        }
    }
}
