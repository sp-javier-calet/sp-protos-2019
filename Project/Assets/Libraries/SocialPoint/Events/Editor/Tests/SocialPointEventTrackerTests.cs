using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using System;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.ServerSync;
using SocialPoint.AppEvents;
using SocialPoint.Base;

namespace SocialPoint.Events
{

    [TestFixture]
    [Category("SocialPoint.Events")]
    internal class SocialPointEventTrackerTests
    {
        SocialPointEventTracker SocialPointEventTracker;
        GameObject GO;
        [SetUp]
        public void SetUp()
        {
            GO = new GameObject();
            var monobh = GO.AddComponent<MonoBehaviour>();
            SocialPointEventTracker = new SocialPointEventTracker(monobh);
            SocialPointEventTracker.HttpClient = Substitute.For<IHttpClient>();
            SocialPointEventTracker.DeviceInfo = Substitute.For<IDeviceInfo>();
            SocialPointEventTracker.RequestSetup = Substitute.For<SocialPointEventTracker.RequestSetupDelegate>();
            SocialPointEventTracker.GetSessionId = Substitute.For<SocialPointEventTracker.GetSessionIdDelegate>();
            SocialPointEventTracker.CommandQueue = Substitute.For<ICommandQueue>();
            SocialPointEventTracker.AppEvents = Substitute.For<IAppEvents>();
        }

        [Test]
        public void Start()
        {
            SocialPointEventTracker.Start();
        }

        [Test]
        public void Stop()
        {
            Start();
            SocialPointEventTracker.Stop();
        }

        [Test]
        public void Reset()
        {
            Start();
            SocialPointEventTracker.Reset();
        }

        [Test]
        /// <summary>
        /// Start already sets the SystemEvent TrackGameStart
        /// </summary>
        public void TrackSystemEvent()
        {
            Start();
            SocialPointEventTracker.Send();
            SocialPointEventTracker.HttpClient.Received(1).Send(Arg.Any<HttpRequest>(),Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void TrackEvent_Through_CommandQueue()
        {
            Start();
            SocialPointEventTracker.GetSessionId().Returns("testID");
            SocialPointEventTracker.TrackEvent("Test event");
            SocialPointEventTracker.CommandQueue.Received(1).Add(Arg.Any<Command>(),Arg.Any<ErrorDelegate>());
        }

        [Test]
        public void TrackEvent_Through_Request()
        {
            Start();
            SocialPointEventTracker.TrackEvent("Test event");
            SocialPointEventTracker.Send();
            SocialPointEventTracker.HttpClient.Received(2).Send(Arg.Any<HttpRequest>(),Arg.Any<HttpResponseDelegate>());
        }
        
        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(GO); 
        }
    }
}
