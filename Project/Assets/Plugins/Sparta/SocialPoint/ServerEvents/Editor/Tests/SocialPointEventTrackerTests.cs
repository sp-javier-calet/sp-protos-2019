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
        SocialPointEventTracker _tracker;
        GameObject GO;

        [SetUp]
        public void SetUp()
        {
            GO = new GameObject();
            var runner = GO.AddComponent<UnityUpdateRunner>();
            _tracker = new SocialPointEventTracker(runner);
            _tracker.HttpClient = Substitute.For<IHttpClient>();
            _tracker.DeviceInfo = Substitute.For<IDeviceInfo>();
            _tracker.LoginData = Substitute.For<ILoginData>();
            _tracker.CommandQueue = Substitute.For<ICommandQueue>();

            var appEvents = Substitute.For<IAppEvents>();
            appEvents.WillGoBackground.Returns(new PriorityAction());
            appEvents.GameWasLoaded.Returns(new PriorityAction());
            appEvents.GameWillRestart.Returns(new PriorityAction());
            _tracker.AppEvents = appEvents;
        }

        [Test]
        public void Start()
        {
            _tracker.Start();
        }

        [Test]
        public void Stop()
        {
            Start();
            _tracker.Stop();
        }

        [Test]
        public void Reset()
        {
            Start();
            _tracker.Reset();
        }

        [Test]
        /// <summary>
        /// Start already sets the SystemEvent TrackGameStart
        /// </summary>
        public void TrackSystemEvent()
        {
            Start();
            _tracker.Send();
            _tracker.HttpClient.Received(1).Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void TrackEvent_Through_CommandQueue()
        {
            Start();
            _tracker.TrackEvent("Test event");
            _tracker.CommandQueue.Received(1).Add(Arg.Any<Command>(), Arg.Any<Action<Attr, Error>>());
        }

        [Test]
        public void TrackEvent_Through_Request()
        {
            Start();
            _tracker.TrackEvent("Test event");
            _tracker.Send();
            _tracker.HttpClient.Received(1).Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void Avoid_GeneralError_If_Old_Session()
        {
            HttpResponseDelegate respDelegate = null;
            _tracker.HttpClient.Send(Arg.Any<HttpRequest>(), Arg.Do<HttpResponseDelegate>(dlg => {
                respDelegate = dlg;
            }));
            var resp = new HttpResponse(482);
            resp.Error = new Error(resp.StatusCode, "HTTP Server responded with error code.");

            var gotError = false;
            _tracker.GeneralError += (EventTrackerErrorType type, Error err) => {
                gotError = type == EventTrackerErrorType.SessionLost;
            };
            _tracker.Send();
            respDelegate(resp);

            Assert.IsTrue(gotError);
            gotError = false;

            _tracker.LoginData.SessionId.Returns("old session");

            _tracker.TrackSystemEvent("Test event");
            _tracker.TrackSystemEvent("Other event");
            _tracker.Send();

            _tracker.LoginData.SessionId.Returns("new session");

            string logMsg = null;
            Application.logMessageReceived += (condition, stackTrace, type) => {
                Assert.AreEqual(LogType.Warning, type);
                logMsg = condition;
            };

            respDelegate(resp);

            Assert.IsFalse(gotError);
            Assert.AreEqual("Tried to send authorized track 'Test event', 'Other event' with old session id.", logMsg);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(GO);
        }
    }
}
