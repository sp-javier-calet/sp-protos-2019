using System;
using UnityEngine;

using SocialPoint.Hardware;
using SocialPoint.Attributes;
using SocialPoint.Utils;

using SocialPoint.AppEvents;

namespace SocialPoint.AppRater
{
    public class AppRater
    {
        private const string AppRaterInfoKey = "AppRaterInfo";
        private const string CurrentVersionKey = "CurrentVersion";
        private const string UsesUntilPromptKey = "usesUntilPrompt";
        private const string EventsUntilPromptKey = "eventsUntilPrompt";
        private const string FirstUseDateKey = "firstUseDate";
        private const string RatedCurrentVersionKey = "ratedCurrentVersion";
        private const string RatedAnyVersionKey = "ratedAnyVersion";
        private const string DeclineToRateKey = "declineToRate";
        private const string ReminderRequestDateKey = "reminderRequestDate";
        private const string PromptsLastDayKey = "promptsLastDay";
        private const string DateStartLastDayKey = "dateStartLastDay";

        private const int DayInSeconds = 86400;

        IDeviceInfo _deviceInfo;
        PersistentAttrStorage _storage;
        IAppEvents _appEvents;
        IAppRaterGUI _appRaterGUI;

        public string StoreUrl;

        public string ID{ protected set; get; }

        public int UsesUntilPrompt;
        public int EventsUntilPrompt;
        public long DaysUntilPrompt;
        public long DaysBeforeReminding;
        public int UserLevelUntilPrompt;
        public int CurrentUserLevel;
        /// <summary>
        /// if any version is rated will skip rating others
        /// </summary>
        public bool AnyVersionRateIsValid;

        public int MaxPromptsPerDay;

        public AppRater(IDeviceInfo deviceInfo, PersistentAttrStorage storage, IAppEvents appEvents, IAppRaterGUI appRaterGUI)
        {
            _deviceInfo = deviceInfo;
            _storage = storage;
            _appEvents = appEvents;
            _appRaterGUI = appRaterGUI;
            _appRaterGUI.setAppRater(this);
            Init();
        }

        public void Init()
        {
            ID = _deviceInfo.AppInfo.Id;

            CheckDayReset();

            //Default values, expected to be customized by developers
            UsesUntilPrompt = 20;
            EventsUntilPrompt = -1;
            DaysUntilPrompt = 30;
            DaysBeforeReminding = 1;
            UserLevelUntilPrompt = 20;
            CurrentUserLevel = 0;
            MaxPromptsPerDay = -1;
        }

        private void CheckDayReset()
        {
            if(!_storage.Has(AppRaterInfoKey))
                return;
            var appRaterInfo = _storage.Load(AppRaterInfoKey).AsDic;
            if((TimeUtils.Timestamp - appRaterInfo.GetValue(DateStartLastDayKey).ToDouble()) > DayInSeconds)
            {
                appRaterInfo.SetValue(PromptsLastDayKey, 0);
                appRaterInfo.SetValue(DateStartLastDayKey, TimeUtils.Timestamp);
                _storage.Save(AppRaterInfoKey, appRaterInfo);
            }
        }

        public void ShowRateView()
        {
            //increment prompts
            var appRaterInfo = _storage.Load(AppRaterInfoKey).AsDic;
            appRaterInfo.SetValue(PromptsLastDayKey, appRaterInfo.GetValue(PromptsLastDayKey).ToInt() + 1);
            _storage.Save(AppRaterInfoKey, appRaterInfo);
         
            _appRaterGUI.Show(true);
        }

        private void IncrementUsesAndRate(bool canPromptForRating)
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

        private void IncrementEventAndRate(bool canPromptForRating)
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

        private void IncrementCount(bool uses, bool events)
        {
            // actual app version
            var version = _deviceInfo.AppInfo.Version;

            if(!_storage.Has(AppRaterInfoKey))
            {
                // first time user launches app, set initial vaules
                SaveFirstDefaults(TimeUtils.Timestamp, 0, 0, false, false, 0, 0, TimeUtils.Timestamp);
            }

            var AppRaterInfo = _storage.Load(AppRaterInfoKey).AsDic;
            
            if(AppRaterInfo.GetValue(CurrentVersionKey) == version)
            {
                if(AppRaterInfo.GetValue(FirstUseDateKey) == 0)
                {
                    AppRaterInfo.SetValue(FirstUseDateKey, TimeUtils.Timestamp);
                }

                if(uses)
                {
                    AppRaterInfo.SetValue(UsesUntilPromptKey, AppRaterInfo.GetValue(UsesUntilPromptKey).ToInt() + 1);
                }

                if(events)
                {
                    // increment the event count
                    AppRaterInfo.SetValue(EventsUntilPromptKey, AppRaterInfo.GetValue(EventsUntilPromptKey).ToInt() + 1);
                }
                // save changes
                _storage.Save(AppRaterInfoKey, AppRaterInfo);
            }
            else
            {
                // new version of the app
                SaveFirstDefaults(TimeUtils.Timestamp, 1, 0, false, false, 0, 0, TimeUtils.Timestamp);
            }
        }

        private void SaveFirstDefaults(double firstUseDate, int usesUntilPrompt, int eventsUntilPrompt, bool ratedCurrentVersion,
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
            _storage.Save(AppRaterInfoKey, defaults);
        }

        private bool preRatingConditionsHaveBeenMet
        {
            get 
            {
                // Check if the days passed from first use has passed
                var appRaterInfo = _storage.Load(AppRaterInfoKey).AsDic;
                var timeSinceFirstLaunch = TimeUtils.Timestamp - appRaterInfo.GetValue(FirstUseDateKey).ToDouble();
                var timeUntilRate = DayInSeconds * DaysUntilPrompt;
                if(timeSinceFirstLaunch < timeUntilRate)
                    return false;

                // Check if the app has been used enough //FIXME:maybe <= ?? first session is uses 1
                if(appRaterInfo.GetValue(UsesUntilPromptKey).ToInt() < UsesUntilPrompt)
                    return false;

                // Check if the user has done enough significant events
                if(appRaterInfo.GetValue(EventsUntilPromptKey).ToInt() < EventsUntilPrompt)
                    return false;

                // Has the user rated any version of the game?
                if(AnyVersionRateIsValid && appRaterInfo.GetValue(RatedAnyVersionKey).ToBool())
                    return false;

                // Has the user previously declined to rate this version of the app?
                if(appRaterInfo.GetValue(DeclineToRateKey).ToBool())
                    return false;

                // Has the user already rated the app?
                if(appRaterInfo.GetValue(RatedCurrentVersionKey).ToBool())
                    return false;

                // If the user wanted to be reminded later, has enough time passed?
                var timeSinceReminderRequest = TimeUtils.Timestamp - appRaterInfo.GetValue(ReminderRequestDateKey).ToDouble();
                var timeUntilReminder = DayInSeconds * DaysBeforeReminding;
                if(timeSinceReminderRequest < timeUntilReminder)
                    return false;

                if(UserLevelUntilPrompt > CurrentUserLevel)
                    return false;

                return true;
            }
        }

        private bool RatingConditionsHaveBeenMet
        {
            get
            {
                CheckDayReset();

                if(!preRatingConditionsHaveBeenMet)
                    return false;

                var appRaterInfo = _storage.Load(AppRaterInfoKey).AsDic;
                if(MaxPromptsPerDay >= 0 && appRaterInfo.GetValue(PromptsLastDayKey).ToInt() >= MaxPromptsPerDay)
                {
                    return false;
                }

                return true;
            }
        }

        private bool IsConnectedToNetwork
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

        public void Register()
        {
            _appEvents.WasOnBackground += OnWasOnBackground;
        }

        public void Unregister()
        {
            _appEvents.WasOnBackground -= OnWasOnBackground;
        }

        private void OnWasOnBackground()
        {
            IncrementUsesCounts(true);
        }

        public void RequestDeclined()
        {
            var appRaterInfo = _storage.Load(AppRaterInfoKey).AsDic;
            appRaterInfo.SetValue(DeclineToRateKey, true);
            _storage.Save(AppRaterInfoKey, appRaterInfo);
        }

        public void RequestAccepted()
        {
            Application.OpenURL(StoreUrl);
            var appRaterInfo = _storage.Load(AppRaterInfoKey).AsDic;
            appRaterInfo.SetValue(RatedCurrentVersionKey, true);
            appRaterInfo.SetValue(RatedAnyVersionKey, true);
            _storage.Save(AppRaterInfoKey, appRaterInfo);
        }

        public void RequestDelayed()
        {
            var appRaterInfo = _storage.Load(AppRaterInfoKey).AsDic;
            appRaterInfo.SetValue(ReminderRequestDateKey, TimeUtils.Timestamp);
            _storage.Save(AppRaterInfoKey, appRaterInfo);
        }

        /*
        TODO:
        void setText(AppRaterText type, const std::string& text);
         */
    }
}

