using NSubstitute;
using NUnit.Framework;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Hardware;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Rating
{

    [TestFixture]
    [Category("SocialPoint.AppRater")]
    class AppRaterTests
    {
        AppRater AppRater;
        FileAttrStorage storage;
        IAppRaterGUI AppRaterGUI;

        [SetUp]
        public void SetUp()
        {
            var deviceInfo = Substitute.For<IDeviceInfo>();
            var appInfo = Substitute.For<IAppInfo>();
            appInfo.Version.Returns("test");
            deviceInfo.AppInfo.Returns(appInfo);
            var netWorkInfo = Substitute.For<INetworkInfo>();
            netWorkInfo.Connectivity.Returns(INetworkInfoStatus.ReachableViaWiFi);
            deviceInfo.NetworkInfo.Returns(netWorkInfo);
            PathsManager.Init();
            storage = new FileAttrStorage(FileUtils.Combine(PathsManager.AppPersistentDataPath, "AppRaterTests"));
            var appEvents = Substitute.For<IAppEvents>();
            appEvents.WasOnBackground.Returns(new PriorityAction());
            AppRater = new AppRater(deviceInfo, storage, appEvents);
            AppRaterGUI = Substitute.For<IAppRaterGUI>();
            AppRaterGUI.Show(Arg.Any<bool>()).Returns(true);
            AppRater.GUI = AppRaterGUI;
            //default test values
            AppRater.UsesUntilPrompt = -1;
            AppRater.EventsUntilPrompt = -1;
            AppRater.DaysUntilPrompt = 0;
            AppRater.DaysBeforeReminding = 0;
            AppRater.UserLevelUntilPrompt = -1;
            AppRater.MaxPromptsPerDay = 2;
            AppRater.Init();
            storage.Remove(AppRaterInfoKey);
            storage.Remove(CurrentVersionKey);
            storage.Remove(UsesUntilPromptKey);
            storage.Remove(EventsUntilPromptKey);
            storage.Remove(RatedCurrentVersionKey);
            storage.Remove(RatedAnyVersionKey);
            storage.Remove(DeclineToRateKey);
            storage.Remove(ReminderRequestDateKey);
            storage.Remove(PromptsLastDayKey);
            storage.Remove(DateStartLastDayKey);
        }

        [Test]
        public void FirstTime_Calls_SaveFirstDefaults()
        {
            Assert.That(!storage.Has(AppRaterInfoKey));
            AppRater.IncrementUsesCounts(true);
            Assert.That(storage.Has(AppRaterInfoKey));
        }

        public void ExpectedShow(int i)
        {
            AppRaterGUI.Received(i).Show(Arg.Any<bool>());
        }

        public void IncrementUses_Expected_Show(int i)
        {
            AppRater.IncrementUsesCounts(true);
            ExpectedShow(i);
        }

        [Test]
        public void MeetCondition_Uses_Calls_ShowRateView()
        {
            ExpectedShow(0);
            IncrementUses_Expected_Show(1);
        }

        //UsesUntilPropt
        [Test]
        public void MissCondition_UsesUntilPrompt_Uses_DoesntCall_ShowRateView()
        {
            AppRater.UsesUntilPrompt = 2;
            ExpectedShow(0);
            IncrementUses_Expected_Show(0);
        }

        //maxpromptday
        [Test]
        public void MeetCondition_MaxPrompts_Uses_Call_ShowRateView()
        {
            AppRater.MaxPromptsPerDay = 2;
            IncrementUses_Expected_Show(1);
            IncrementUses_Expected_Show(2);
        }

        [Test]
        public void MissCondition_MaxPrompts_Uses_DoesntCall_ShowRateView()
        {
            AppRater.MaxPromptsPerDay = 1;
            IncrementUses_Expected_Show(1);
            IncrementUses_Expected_Show(1);
        }

        //dayuntilprompt
        [Test]
        public void MissCondition_DaysUntilPrompt_DoesntCall_ShowRateView()
        {
            AppRater.DaysUntilPrompt = 1;
            IncrementUses_Expected_Show(0);
        }

        //userlevel
        [Test]
        public void MissCondition_UserLevel_DoesntCall_ShowRateView()
        {
            AppRater.GetUserLevel = () => 0;
            AppRater.UserLevelUntilPrompt = 1;
            IncrementUses_Expected_Show(0);
        }
        
        //anyversionrate
        /*FIXME:
        [Test]
        public void MissCondition_AnyVersionRated_DoesntCall_ShowRateView()
        {
            AppRater.AnyVersionRateIsValid = true;
            IncrementUses_Expected_Show(1);
            AppRater.RequestAccepted();
            IncrementUses_Expected_Show(1);
        }

        [Test]
        public void MeetCondition_AnyVersionRated_Call_ShowRateView()
        {
            AppRater.AnyVersionRateIsValid = true;
            IncrementUses_Expected_Show(1);
            AppRater.RequestAccepted();
            IncrementUses_Expected_Show(2);
        }
        */

        public void IncrementEvents_Expected_Show(int i)
        {
            AppRater.IncrementEventCounts(true);
            ExpectedShow(i);
        }

        [Test]
        public void MeetCondition_Events_Calls_ShowRateView()
        {
            ExpectedShow(0);
            IncrementEvents_Expected_Show(1);
        }

        [Test]
        public void MissCondition_MaxPrompts_Events_DoesntCall_ShowRateView()
        {
            AppRater.MaxPromptsPerDay = 1;
            IncrementEvents_Expected_Show(1);
            IncrementEvents_Expected_Show(1);
        }

        //requests
        [Test]
        public void On_RequestDeclined_DoesntCall_ShowRateView()
        {
            IncrementUses_Expected_Show(1);
            AppRater.OnRequestResult(RateRequestResult.Decline);
            IncrementUses_Expected_Show(1);
        }

        [Test]
        public void On_RequestDelayed_DoesntCall_ShowRateView()
        {
            AppRater.DaysBeforeReminding = 1;
            IncrementUses_Expected_Show(1);
            AppRater.OnRequestResult(RateRequestResult.Delay);
            IncrementUses_Expected_Show(1);
        }

        [TearDown]
        public void TearDown()
        {
            storage.Remove(AppRaterInfoKey);
        }

        const string AppRaterInfoKey = "AppRaterInfo";
        const string CurrentVersionKey = "CurrentVersion";
        const string UsesUntilPromptKey = "usesUntilPrompt";
        const string EventsUntilPromptKey = "eventsUntilPrompt";
        const string FirstUseDateKey = "firstUseDate";
        const string RatedCurrentVersionKey = "ratedCurrentVersion";
        const string RatedAnyVersionKey = "ratedAnyVersion";
        const string DeclineToRateKey = "declineToRate";
        const string ReminderRequestDateKey = "reminderRequestDate";
        const string PromptsLastDayKey = "promptsLastDay";
        const string DateStartLastDayKey = "dateStartLastDay";


        void SaveFirstDefaults(string version, double firstUseDate, int usesUntilPrompt, int eventsUntilPrompt, bool ratedCurrentVersion,
                               bool declineToRate, double reminderRequestDate, int promptsPerDay, double lastDayDate)
        {
            var defaults = new AttrDic();
            defaults.SetValue(CurrentVersionKey, version);
            defaults.SetValue(FirstUseDateKey, firstUseDate);
            defaults.SetValue(UsesUntilPromptKey, usesUntilPrompt);
            defaults.SetValue(EventsUntilPromptKey, eventsUntilPrompt);
            defaults.SetValue(RatedCurrentVersionKey, ratedCurrentVersion);
            defaults.SetValue(DeclineToRateKey, declineToRate);
            defaults.SetValue(ReminderRequestDateKey, reminderRequestDate);
            defaults.SetValue(PromptsLastDayKey, promptsPerDay);
            defaults.SetValue(DateStartLastDayKey, lastDayDate);
            storage.Save(AppRaterInfoKey, defaults);
        }
    }
}
