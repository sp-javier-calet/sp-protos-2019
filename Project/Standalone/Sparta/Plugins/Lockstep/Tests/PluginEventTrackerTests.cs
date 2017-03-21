using System;
using NUnit.Framework;
using NSubstitute;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    [TestFixture]
    [Category("SocialPoint.Events")]
    public class PluginEventTrackerTests
    {

        PluginEventTracker EventTracker;
        UpdateScheduler scheduler;

        [SetUp]
        public void SetUp()
        {
            scheduler = Substitute.For<UpdateScheduler>();
            PluginEventTracker = new PluginEventTracker(scheduler);
        }

        [Test]
        public void Start()
        {
            EventTracker.Start();
            Assert(scheduler.Received(1).Add);
        }
    }
}

