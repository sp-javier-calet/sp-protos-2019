using System;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Hardware;
using SocialPoint.Utils;
using UnityEngine;

namespace SocialPoint.Rating
{
    public class AppRater : IAppRater, IDisposable
    {
        const string AppRaterInfoKey = "AppRaterInfo";
        const string CurrentVersionKey = "CurrentVersion";
        const string UsesUntilPromptKey = "usesUntilPrompt";
        const string EventsUntilPromptKey = "eventsUntilPrompt";
        const string FirstUseDateKey = "firstUseDate";
        const string RatedCurrentVersionKey = "ratedCurrentVersion";
        protected const string RatedAnyVersionKey = "ratedAnyVersion";
        const string DeclineToRateKey = "declineToRate";
        const string ReminderRequestDateKey = "reminderRequestDate";
        const string PromptsLastDayKey = "promptsLastDay";
        const string DateStartLastDayKey = "dateStartLastDay";

        const int DayInSeconds = 86400;

        IDeviceInfo _deviceInfo;
        readonly IAttrStorage _storage;
        IAppEvents _appEvents;

        IAppRaterGUI _gui;

        public IAppRaterGUI GUI
        {
            set
            {
                if(_gui != value)
                {
                    _gui = value;
                    _gui.SetAppRater(this);
                }
            }
        }

        public string StoreUrl = "http://www.socialpoint.es/";

        public const int DefaultUsesUntilPrompt = 20;
        public const int DefaultEventsUntilPrompt = -1;
        public const long DefaultDaysUntilPrompt = 30;
        public const long DefaultDaysBeforeReminding = 1;
        public const int DefaultUserLevelUntilPrompt = 20;
        public const int DefaultMaxPromptsPerDay = -1;

        public int UsesUntilPrompt = DefaultUsesUntilPrompt;
        public int EventsUntilPrompt = DefaultEventsUntilPrompt;
        public long DaysUntilPrompt = DefaultDaysUntilPrompt;
        public long DaysBeforeReminding = DefaultDaysBeforeReminding;
        public int UserLevelUntilPrompt = DefaultUserLevelUntilPrompt;
        public int MaxPromptsPerDay = DefaultMaxPromptsPerDay;

        public GetUserLevelDelegate GetUserLevel
        {
            set;
            get;
        }

        /// <summary>
        /// if any version is rated will skip rating others
        /// </summary>
        public bool AnyVersionRateIsValid;


        public AppRater(IDeviceInfo deviceInfo, IAttrStorage storage, IAppEvents appEvents)
        {
            _deviceInfo = deviceInfo;
            _storage = storage;

            //In some games, we don't want showRate when you come back from background
            if(appEvents != null)
            {
                _appEvents = appEvents;
                _appEvents.WasOnBackground += OnWasOnBackground;
            }
        }

        virtual public void Dispose()
        {
            if(_appEvents != null)
            {
                _appEvents.WasOnBackground -= OnWasOnBackground;
            }
        }

        public void Init()
        {
            CheckDayReset();
        }

        protected bool HasInfo()
        {
            return _storage.Has(AppRaterInfoKey);
        }

        protected AttrDic LoadInfo()
        {
            return _storage.Load(AppRaterInfoKey).AsDic;
        }

        protected void SaveInfo(AttrDic data)
        {
            _storage.Save(AppRaterInfoKey, data);
        }

        void CheckDayReset()
        {
            if(!HasInfo())
            {
                SaveFirstDefaults(TimeUtils.Timestamp, 0, 0, false, false, 0, 0, TimeUtils.Timestamp);
                return;
            }
            var appRaterInfo = LoadInfo();
            if((TimeUtils.Timestamp - appRaterInfo.GetValue(DateStartLastDayKey).ToDouble()) > DayInSeconds)
            {
                appRaterInfo.SetValue(PromptsLastDayKey, 0);
                appRaterInfo.SetValue(DateStartLastDayKey, TimeUtils.Timestamp);
                SaveInfo(appRaterInfo);
            }
        }

        public void ShowRateView()
        {
            //Avoiding problems in the splash screen
            if(_gui == null)
            {
                return;
            }

            if(_gui.Show(true))
            {
                //increment prompts only if the popup can be shown
                var appRaterInfo = LoadInfo();
                appRaterInfo.SetValue(PromptsLastDayKey, appRaterInfo.GetValue(PromptsLastDayKey).ToInt() + 1);
                SaveInfo(appRaterInfo);
            }
        }

        void IncrementUsesAndRate(bool canPromptForRating)
        {
            IncrementCount(true, false);
            if(canPromptForRating && RatingConditionsHaveBeenMet && IsConnectedToNetwork)
            {
                ShowRateView();
            }
        }

        public void IncrementUsesCounts(bool canPromptForRating)
        {
            // increment uses count
            IncrementUsesAndRate(canPromptForRating);
        }

        void IncrementEventAndRate(bool canPromptForRating)
        {
            IncrementCount(false, true);
            if(canPromptForRating && RatingConditionsHaveBeenMet && IsConnectedToNetwork)
            {
                ShowRateView();
            }
        }

        public void IncrementEventCounts(bool canPromptForRating)
        {
            // increment uses count
            IncrementEventAndRate(canPromptForRating);
        }

        void IncrementCount(bool uses, bool events)
        {
            // current app version
            var version = _deviceInfo.AppInfo.Version;

            AttrDic appRaterInfo = !HasInfo() ? SaveFirstDefaults(TimeUtils.Timestamp, 0, 0, false, false, 0, 0, TimeUtils.Timestamp) : LoadInfo();

            if(appRaterInfo.GetValue(CurrentVersionKey) == version)
            {
                if(appRaterInfo.GetValue(FirstUseDateKey) == 0)
                {
                    appRaterInfo.SetValue(FirstUseDateKey, TimeUtils.Timestamp);
                }

                if(uses)
                {
                    appRaterInfo.SetValue(UsesUntilPromptKey, appRaterInfo.GetValue(UsesUntilPromptKey).ToInt() + 1);
                }

                if(events)
                {
                    // increment the event count
                    appRaterInfo.SetValue(EventsUntilPromptKey, appRaterInfo.GetValue(EventsUntilPromptKey).ToInt() + 1);
                }
                // save changes
                SaveInfo(appRaterInfo);
            }
            else
            {
                // new version of the app
                SaveFirstDefaults(TimeUtils.Timestamp, 1, 0, false, false, 0, 0, TimeUtils.Timestamp);
            }
        }

        AttrDic SaveFirstDefaults(double firstUseDate, int usesUntilPrompt, int eventsUntilPrompt, bool ratedCurrentVersion,
                                  bool declineToRate, double reminderRequestDate, int promptsPerDay, double lastDayDate)
        {
            var defaults = new AttrDic();
            defaults.SetValue(CurrentVersionKey, _deviceInfo.AppInfo.Version);
            defaults.SetValue(FirstUseDateKey, firstUseDate);
            defaults.SetValue(UsesUntilPromptKey, usesUntilPrompt);
            defaults.SetValue(EventsUntilPromptKey, eventsUntilPrompt);
            defaults.SetValue(RatedCurrentVersionKey, ratedCurrentVersion);
            defaults.SetValue(DeclineToRateKey, declineToRate);
            defaults.SetValue(ReminderRequestDateKey, reminderRequestDate);
            defaults.SetValue(PromptsLastDayKey, promptsPerDay);
            defaults.SetValue(DateStartLastDayKey, lastDayDate);
            SaveInfo(defaults);
            return defaults;
        }

        bool PreRatingConditionsHaveBeenMet(AttrDic appRaterInfo)
        {
            // Has the user previously declined to rate this version of the app?
            if(appRaterInfo.GetValue(DeclineToRateKey).ToBool())
            {
                return false;
            }

            // Has the user rated any version of the game?
            if(AnyVersionRateIsValid && appRaterInfo.GetValue(RatedAnyVersionKey).ToBool())
            {
                return false;
            }

            return PreRatingCustomConditionsHaveBeenMet(appRaterInfo);
        }

        protected virtual bool PreRatingCustomConditionsHaveBeenMet(AttrDic appRaterInfo)
        {

            // Check if the days passed from first use has passed
            var timeSinceFirstLaunch = TimeUtils.Timestamp - appRaterInfo.GetValue(FirstUseDateKey).ToDouble();
            var timeUntilRate = DayInSeconds * DaysUntilPrompt;
            if(timeSinceFirstLaunch < timeUntilRate)
            {
                return false;
            }

            // Check if the app has been used enough //FIXME:maybe <= ?? first session is uses 1
            if(appRaterInfo.GetValue(UsesUntilPromptKey).ToInt() < UsesUntilPrompt)
            {
                return false;
            }

            // Check if the user has done enough significant events
            if(appRaterInfo.GetValue(EventsUntilPromptKey).ToInt() < EventsUntilPrompt)
            {
                return false;
            }

            // If the user wanted to be reminded later, has enough time passed?
            var timeSinceReminderRequest = TimeUtils.Timestamp - appRaterInfo.GetValue(ReminderRequestDateKey).ToDouble();
            var timeUntilReminder = DayInSeconds * DaysBeforeReminding;
            if(timeSinceReminderRequest < timeUntilReminder)
            {
                return false;
            }
            if(GetUserLevel != null)
            {
                if(UserLevelUntilPrompt > GetUserLevel())
                {
                    return false;
                }
            }

            // Has the user already rated the app?
            return !appRaterInfo.GetValue(RatedCurrentVersionKey).ToBool();

        }

        bool RatingConditionsHaveBeenMet
        {
            get
            {
                CheckDayReset();
                var appRaterInfo = LoadInfo();

                if(!PreRatingConditionsHaveBeenMet(appRaterInfo))
                {
                    return false;
                }

                if(MaxPromptsPerDay >= 0 && appRaterInfo.GetValue(PromptsLastDayKey).ToInt() >= MaxPromptsPerDay)
                {
                    return false;
                }

                return true;
            }
        }

        bool IsConnectedToNetwork
        {
            get
            {
                return ((_deviceInfo.NetworkInfo.Connectivity != INetworkInfoStatus.NotReachable)
                && (_deviceInfo.NetworkInfo.Connectivity != INetworkInfoStatus.Unknown));
            }
        }

        public void ResetStatistics()
        {
            SaveFirstDefaults(0, 0, 0, false, false, 0, 0, 0);
            //TODO:  load info, set ratedAny to false, store it.
        }

        void OnWasOnBackground()
        {
            IncrementUsesCounts(true);
        }

        public void OnRequestResult(RateRequestResult result)
        {
            switch(result)
            {
            case RateRequestResult.Accept:
                RequestAccepted();
                break;
            case RateRequestResult.Decline:
                RequestDeclined();
                break;
            case RateRequestResult.Delay:
                RequestDelayed();
                break;
            }
        }

        void RequestDeclined()
        {
            var appRaterInfo = LoadInfo();
            appRaterInfo.SetValue(DeclineToRateKey, true);
            SaveInfo(appRaterInfo);
        }

        void RequestAccepted()
        {
            Application.OpenURL(StoreUrl);
            var appRaterInfo = LoadInfo();
            appRaterInfo.SetValue(RatedCurrentVersionKey, true);
            appRaterInfo.SetValue(RatedAnyVersionKey, true);
            SaveInfo(appRaterInfo);
        }

        void RequestDelayed()
        {
            var appRaterInfo = LoadInfo();
            appRaterInfo.SetValue(ReminderRequestDateKey, TimeUtils.Timestamp);
            SaveInfo(appRaterInfo);
        }

        public override string ToString()
        {
            return string.Format(
                "Config:\n" +
                "[UsesUntilPrompt={0}, EventsUntilPrompt={1}, DaysUntilPrompt={2}, DaysBeforeReminding={3}, UserLevelUntilPrompt={4}, CurrentUserLevel={5}, MaxPromptsPerDay={6}, AnyVersionRateIsValid={7}]\n" +
                "Statistics:\n" +
                "[{8}]",
                UsesUntilPrompt, EventsUntilPrompt, DaysUntilPrompt, DaysBeforeReminding, UserLevelUntilPrompt, GetUserLevel == null ? 0 : GetUserLevel(), MaxPromptsPerDay, AnyVersionRateIsValid, HasInfo() ? LoadInfo().ToString() : "");
        }

        /*
        TODO:
        void setText(AppRaterText type, const std::string& text);
         *
         */

        public virtual void RemoveKeys()
        {
            _storage.Remove(AppRaterInfoKey);
            _storage.Remove(CurrentVersionKey);
            _storage.Remove(UsesUntilPromptKey);
            _storage.Remove(EventsUntilPromptKey);
            _storage.Remove(RatedCurrentVersionKey);
            _storage.Remove(RatedAnyVersionKey);
            _storage.Remove(DeclineToRateKey);
            _storage.Remove(ReminderRequestDateKey);
            _storage.Remove(PromptsLastDayKey);
            _storage.Remove(DateStartLastDayKey);
        }

        protected void RemoveKey(string key)
        {
            _storage.Remove(key);
        }
    }
}
