using System;
using NSubstitute;
using NUnit.Framework;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Login;
using UnityEngine;

namespace SocialPoint.Marketing
{
    [TestFixture]
    [Category("SocialPoint.Marketing")]
    public sealed class SocialPointMarketingAttributionManagerTests
    {

        SocialPointMarketingAttributionManager manager;
        IAppEvents appEvents;
        IAttrStorage storage;
        IMarketingTracker tracker;
        GameObject gameObject;
        ILoginData LoginData;

        [SetUp]
        public void SetUp()
        {
            UnityEngine.Assertions.Assert.raiseExceptions = true;
            gameObject = new GameObject();
            appEvents = gameObject.AddComponent<UnityAppEvents>();

            LoginData = Substitute.For<ILoginData>();
            LoginData.UserId.Returns((ulong)1234);

            storage = Substitute.For<IAttrStorage>();

            manager = new SocialPointMarketingAttributionManager(appEvents, storage);
            manager.LoginData = LoginData; 

            tracker = Substitute.For<IMarketingTracker>();
        }

        [Test]
        public void Add_traker()
        {
            manager.AddTracker(tracker);
        }

        [Test]
        public void Add_traker_already_added_asserts()
        {
            manager.AddTracker(tracker);
            Assert.Throws<UnityEngine.Assertions.AssertionException>(() => manager.AddTracker(tracker));
        }

        [Test]
        public void When_GameLoaded_calls_OnGameLoaded()
        {
            manager.AddTracker(tracker);
            appEvents.TriggerGameWasLoaded();
            tracker.ReceivedWithAnyArgs(1).TrackInstall(Arg.Any<bool>());
        }

        [Test]
        public void Multiple_GameLoaded_calls_once_OnGameLoaded()
        {
            manager.AddTracker(tracker);
            appEvents.TriggerGameWasLoaded();
            appEvents.TriggerGameWasLoaded();
            tracker.ReceivedWithAnyArgs(1).TrackInstall(Arg.Any<bool>());
        }

        [Test]
        public void Loads_previously_installed_true()
        {
            storage.Has(SocialPointMarketingAttributionManager.AppPreviouslyInstalledForMarketing).Returns(true);
            storage.Load(SocialPointMarketingAttributionManager.AppPreviouslyInstalledForMarketing).Returns(new AttrBool(true));
            manager = new SocialPointMarketingAttributionManager(appEvents, storage);
            manager.LoginData = LoginData;
            manager.AddTracker(tracker);
            appEvents.TriggerGameWasLoaded();
            tracker.Received(1).TrackInstall(false);
        }

        [Test]
        public void Loads_previously_installed_false()
        {
            storage.Has(SocialPointMarketingAttributionManager.AppPreviouslyInstalledForMarketing).Returns(false);
            manager = new SocialPointMarketingAttributionManager(appEvents, storage);
            manager.LoginData = LoginData;
            manager.AddTracker(tracker);
            appEvents.TriggerGameWasLoaded();
            tracker.Received(1).TrackInstall(true);
        }

        [Test]
        public void Stores_previously_installed()
        {
            storage = Substitute.For<IAttrStorage>();
            storage.Has(SocialPointMarketingAttributionManager.AppPreviouslyInstalledForMarketing).Returns(false);
            manager = new SocialPointMarketingAttributionManager(appEvents, storage);
            storage.Received(1).Save(SocialPointMarketingAttributionManager.AppPreviouslyInstalledForMarketing, new AttrBool(true));
        }

        [Test]
        public void Trackers_receive_userId()
        {
            manager.AddTracker(tracker);
            appEvents.TriggerGameWasLoaded();
            tracker.Received(1).SetUserID("1234");
        }

        [Test]
        #if !DEBUG
        [Ignore("Only on Debug Mode")]
        #endif
        public void DebugMode_set_to_trackers()
        {
            manager.DebugMode = true;
            manager.AddTracker(tracker);
            appEvents.TriggerGameWasLoaded();
            tracker.Received(1).SetDebugMode(true);
        }

        [Test]
        public void TrackEventDelegate_called_on_attribution()
        {
            manager.DebugMode = true;
            var trackEventDelegate = Substitute.For<TrackEventDelegate>();
            manager.TrackEvent = trackEventDelegate;
            tracker.TrackInstall(Arg.Do<bool>(b => tracker.OnDataReceived += Raise.Event<Action<TrackerAttributionData>>(new TrackerAttributionData {
                trackerName = "test2",
                data = "data"
            })));
            manager.AddTracker(tracker);
            appEvents.TriggerGameWasLoaded();
            trackEventDelegate.ReceivedWithAnyArgs(1).Invoke(Arg.Any<string>(), Arg.Any<AttrDic>(), null);
        }

        [TearDown]
        public void TearDown()
        {
            manager.Dispose();
            UnityEngine.Object.DestroyImmediate(gameObject);
            UnityEngine.Assertions.Assert.raiseExceptions = false;
        }
    }
}
