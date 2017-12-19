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
        const string RatedAnyVersionKey = "ratedAnyVersion";
        const string DeclineToRateKey = "declineToRate";
        const string ReminderRequestDateKey = "reminderRequestDate";
        const string PromptsLastDayKey = "promptsLastDay";
        const string DateStartLastDayKey = "dateStartLastDay";

        const int DayInSeconds = 86400;

        readonly IDeviceInfo _deviceInfo;
        readonly IAttrStorage _storage;
        readonly IAppEvents _appEvents;
        AttrDic _appRaterInfo;

        IAppRaterGUI _gui;
        public IAppRaterGUI GUI
        {
            set
            {
                if(_gui != value)
                {
                    _gui = value;
                    _gui.AppRater = this;
                }
            }
        }

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
                _appEvents.WasOnBackground.Add(0, OnWasOnBackground);
            }

            _appRaterInfo = new AttrDic();
        }

        public void Dispose()
        {
            if(_appEvents != null)
            {
                _appEvents.WasOnBackground.Remove(OnWasOnBackground);
            }
        }

        public void Init()
        {
            CheckDayReset();
        }

        bool HasInfo()
        {
            return _storage.Has(AppRaterInfoKey);
        }

        void LoadInfo()
        {
            _appRaterInfo = _storage.Load(AppRaterInfoKey).AsDic;
        }

        void SaveInfo()
        {
            _storage.Save(AppRaterInfoKey, _appRaterInfo);
        }

        void CheckDayReset()
        {
            if(!HasInfo())
            {
                SaveFirstDefaults(TimeUtils.Timestamp, 0, 0, false, false, false, 0, 0, TimeUtils.Timestamp);
                return;
            }

            LoadInfo();

            if((TimeUtils.Timestamp - _appRaterInfo.GetValue(DateStartLastDayKey).ToLong()) > DayInSeconds)
            {
                _appRaterInfo.SetValue(PromptsLastDayKey, 0);
                _appRaterInfo.SetValue(DateStartLastDayKey, TimeUtils.Timestamp);
                SaveInfo();
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
                _appRaterInfo.SetValue(PromptsLastDayKey, _appRaterInfo.GetValue(PromptsLastDayKey).ToInt() + 1);
                SaveInfo();
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

            if(!HasInfo())
            {
                SaveFirstDefaults(TimeUtils.Timestamp, 0, 0, false, false, false, 0, 0, TimeUtils.Timestamp);
            }
            else
            {
                LoadInfo();
            }

            if(_appRaterInfo.GetValue(CurrentVersionKey) == version)
            {
                if(_appRaterInfo.GetValue(FirstUseDateKey).ToLong() == 0)
                {
                    _appRaterInfo.SetValue(FirstUseDateKey, TimeUtils.Timestamp);
                }

                if(uses)
                {
                    _appRaterInfo.SetValue(UsesUntilPromptKey, _appRaterInfo.GetValue(UsesUntilPromptKey).ToInt() + 1);
                }

                if(events)
                {
                    // increment the event count
                    _appRaterInfo.SetValue(EventsUntilPromptKey, _appRaterInfo.GetValue(EventsUntilPromptKey).ToInt() + 1);
                }
                // save changes
                SaveInfo();
            }
            else
            {
                // new version of the app
                SaveFirstDefaults(TimeUtils.Timestamp, 1, 0, false, false, false, 0, 0, TimeUtils.Timestamp);
            }
        }

        void SaveFirstDefaults(long firstUseDate, int usesUntilPrompt, int eventsUntilPrompt, bool ratedCurrentVersion,
                               bool ratedAnyVersion, bool declineToRate, long reminderRequestDate, int promptsPerDay, long lastDayDate)
        {
            _appRaterInfo.SetValue(CurrentVersionKey, _deviceInfo.AppInfo.Version);
            _appRaterInfo.SetValue(FirstUseDateKey, firstUseDate);
            _appRaterInfo.SetValue(UsesUntilPromptKey, usesUntilPrompt);
            _appRaterInfo.SetValue(EventsUntilPromptKey, eventsUntilPrompt);
            _appRaterInfo.SetValue(RatedCurrentVersionKey, ratedCurrentVersion);
            _appRaterInfo.SetValue(RatedAnyVersionKey, ratedAnyVersion);
            _appRaterInfo.SetValue(DeclineToRateKey, declineToRate);
            _appRaterInfo.SetValue(ReminderRequestDateKey, reminderRequestDate);
            _appRaterInfo.SetValue(PromptsLastDayKey, promptsPerDay);
            _appRaterInfo.SetValue(DateStartLastDayKey, lastDayDate);
            SaveInfo();
        }

        bool PreRatingConditionsHaveBeenMet()
        {
            // Has the user previously declined to rate this version of the app?
            if(_appRaterInfo.GetValue(DeclineToRateKey).ToBool())
            {
                return false;
            }

            // Has the user rated any version of the game?
            if(HasRatedAnyVersion)
            {
                return false;
            }

            return PreRatingCustomConditionsHaveBeenMet();
        }

        public bool HasRatedAnyVersion
        {
            get
            {
                if(AnyVersionRateIsValid && _appRaterInfo.GetValue(RatedAnyVersionKey).ToBool())
                {
                    return true;
                }

                return false;
            }
        }

        protected virtual bool PreRatingCustomConditionsHaveBeenMet()
        {

            // Check if the days passed from first use has passed
            var timeSinceFirstLaunch = TimeUtils.Timestamp - _appRaterInfo.GetValue(FirstUseDateKey).ToLong();
            var timeUntilRate = DayInSeconds * DaysUntilPrompt;
            if(timeSinceFirstLaunch < timeUntilRate)
            {
                return false;
            }

            // Check if the app has been used enough //FIXME:maybe <= ?? first session is uses 1
            if(_appRaterInfo.GetValue(UsesUntilPromptKey).ToInt() < UsesUntilPrompt)
            {
                return false;
            }

            // Check if the user has done enough significant events
            if(_appRaterInfo.GetValue(EventsUntilPromptKey).ToInt() < EventsUntilPrompt)
            {
                return false;
            }

            // If the user wanted to be reminded later, has enough time passed?
            var timeSinceReminderRequest = TimeUtils.Timestamp - _appRaterInfo.GetValue(ReminderRequestDateKey).ToLong();
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
            return !_appRaterInfo.GetValue(RatedCurrentVersionKey).ToBool();
        }

        bool RatingConditionsHaveBeenMet
        {
            get
            {
                CheckDayReset();

                if(!PreRatingConditionsHaveBeenMet())
                {
                    return false;
                }

                if(MaxPromptsPerDay >= 0 && _appRaterInfo.GetValue(PromptsLastDayKey).ToInt() >= MaxPromptsPerDay)
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
            SaveFirstDefaults(0, 0, 0, false, false, false, 0, 0, 0);
            LoadInfo();
        }

        public event Action OnRequestResultAction;

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

            OnRequestResultEvent();
        }

        void OnRequestResultEvent()
        {
            if(OnRequestResultAction != null)
            {
                OnRequestResultAction();
            }
        }

        void RequestDeclined()
        {
            _appRaterInfo.SetValue(DeclineToRateKey, true);
            SaveInfo();
        }

        void RequestAccepted()
        {
            _gui.Rate();
            _appRaterInfo.SetValue(RatedCurrentVersionKey, true);
            _appRaterInfo.SetValue(RatedAnyVersionKey, true);
            SaveInfo();
        }

        void RequestDelayed()
        {
            _appRaterInfo.SetValue(ReminderRequestDateKey, TimeUtils.Timestamp);
            SaveInfo();
        }

        public override string ToString()
        {
            return string.Format(
                "Config:\n" +
                "[UsesUntilPrompt={0}, EventsUntilPrompt={1}, DaysUntilPrompt={2}, DaysBeforeReminding={3}, UserLevelUntilPrompt={4}, CurrentUserLevel={5}, MaxPromptsPerDay={6}, AnyVersionRateIsValid={7}]\n" +
                "Statistics:\n" +
                "[{8}]",
                UsesUntilPrompt, EventsUntilPrompt, DaysUntilPrompt, DaysBeforeReminding, UserLevelUntilPrompt, GetUserLevel == null ? 0 : GetUserLevel(), MaxPromptsPerDay, AnyVersionRateIsValid, _appRaterInfo);
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
