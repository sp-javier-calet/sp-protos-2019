using System;
using System.Collections;
using NUnit.Framework;
using NSubstitute;
using SocialPoint.Utils;

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
            var port = 55555;
            _scheduler = new UpdateScheduler();
            _server = new SimpleSocketNetworkServer(_scheduler, ip, port);
            _client = new SimpleSocketNetworkClient(_scheduler, ip, port);
            _client2 = new SimpleSocketNetworkClient(_scheduler, ip, port);
        }

        override protected void WaitForEvents()
        {
            _scheduler.Update(100, 100);
        }
    }
}
