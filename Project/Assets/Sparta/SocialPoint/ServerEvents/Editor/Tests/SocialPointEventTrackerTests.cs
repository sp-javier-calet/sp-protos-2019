using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using System;
using SocialPoint.Attributes;
using SocialPoint.Network;
using SocialPoint.Login;
using SocialPoint.Hardware;
using SocialPoint.ServerSync;
using SocialPoint.AppEvents;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.ServerEvents
{

    [TestFixture]
    [Category("SocialPoint.Events")]
    class SocialPointEventTrackerTests
    {
        SocialPointEventTracker SocialPointEventTracker;
        GameObject GO;

        [SetUp]
        public void SetUp()
        {
            GO = new GameObject();
            var runner = GO.AddComponent<UnityUpdateRunner>();
            SocialPointEventTracker = new SocialPointEventTracker(runner);
            SocialPointEventTracker.HttpClient = Substitute.For<IHttpClient>();
            SocialPointEventTracker.DeviceInfo = Substitute.For<IDeviceInfo>();
            SocialPointEventTracker.LoginData = Substitute.For<ILoginData>();
            SocialPointEventTracker.CommandQueue = Substitute.For<ICommandQueue>();

            var appEvents = Substitute.For<IAppEvents>();
            appEvents.WillGoBackground.Returns(new PriorityAction());
            appEvents.GameWasLoaded.Returns(new PriorityAction());
            appEvents.GameWillRestart.Returns(new PriorityAction());
            SocialPointEventTracker.AppEvents = appEvents;
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
            SocialPointEventTracker.HttpClient.Received(1).Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void TrackEvent_Through_CommandQueue()
        {
            Start();
            SocialPointEventTracker.TrackEvent("Test event");
            SocialPointEventTracker.CommandQueue.Received(1).Add(Arg.Any<Command>(), Arg.Any<Action<Attr, Error>>());
        }

        [Test]
        public void TrackEvent_Through_Request()
        {
            Start();
            SocialPointEventTracker.TrackEvent("Test event");
            SocialPointEventTracker.Send();
            SocialPointEventTracker.HttpClient.Received(1).Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>());
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(GO); 
        }
    }
}
