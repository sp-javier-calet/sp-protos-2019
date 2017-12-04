using System;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Login;

namespace SocialPoint.Marketing
{
    public sealed class SocialPointMarketingAttributionManager : IMarketingAttributionManager
    {
        const string MarketingLogEventName = "log.marketing";

        public const string AppPreviouslyInstalledForMarketing = "sparta_app_previously_installed_for_marketing";

        readonly List<IMarketingTracker> _trackers;
        readonly IAppEvents _appEvents;

        IAttrStorage _storage;
        bool _appPreviouslyInstalled;
        #pragma warning disable 0414
        bool _tracked;
        //only used on DebugUtils.Assert, disabled warning of "unused" when compiling to iOS
        #pragma warning restore 0414

        public SocialPointMarketingAttributionManager(IAppEvents appEvents, IAttrStorage storage)
        {
            _trackers = new List<IMarketingTracker>();
            _appEvents = appEvents;
            _appEvents.GameWasLoaded.Add(0, OnGameLoaded);

            _storage = storage;
            if(_storage.Has(AppPreviouslyInstalledForMarketing))
            {
                _appPreviouslyInstalled = _storage.Load(AppPreviouslyInstalledForMarketing).AsValue.ToBool();
            }

            if(!_appPreviouslyInstalled)
            {
                _storage.Save(AppPreviouslyInstalledForMarketing, new AttrBool(true));    
            }
        }

        #region IMarketingAttributionManager implementation

        public void AddTracker(IMarketingTracker tracker)
        {
            DebugUtils.Assert(!_tracked, "SocialPointMarketingAttributionManager Track already done, add the tracker before");
            DebugUtils.Assert(!_trackers.Contains(tracker), "SocialPointMarketingAttributionManager tracker already added");
            tracker.OnDataReceived += OnTrackerReceivedData;
            _trackers.Add(tracker);
        }

        public void OnGameLoaded()
        {
            TrackInstall();
            _appEvents.GameWasLoaded.Remove(OnGameLoaded);
        }

        public void TrackInstall()
        {
            bool isNewInstall = !_appPreviouslyInstalled;
            DebugUtils.Assert(LoginData != null, "SocialPointMarketingAttributionManager UserId is null");
            DebugUtils.Assert(!String.IsNullOrEmpty(LoginData.UserId.ToString()), "SocialPointMarketingAttributionManager UserId returns empty");
            for(var i = 0; i < _trackers.Count; i++)
            {
                var tracker = _trackers[i];
                #if DEBUG
                tracker.SetDebugMode(DebugMode);
                #endif
                tracker.SetUserID(LoginData.UserId.ToString());

                tracker.TrackInstall(isNewInstall);
            }
            _tracked = true;
        }

        public bool DebugMode
        {
            get;
            set;
        }

        public void OnTrackerReceivedData(TrackerAttributionData data)
        {
            if(DebugMode)
            {
                var handler = TrackEvent;
                if(handler != null)
                {
                    handler(MarketingLogEventName, data.ToAttrDic());
                }
            }
        }

        public ILoginData LoginData { get; set; }

        public TrackEventDelegate TrackEvent
        {
            get;
            set;
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            for(int i = 0; i < _trackers.Count; i++)
            {
                _trackers[i].OnDataReceived -= OnTrackerReceivedData;
            }
            _appEvents.GameWasLoaded.Remove(OnGameLoaded);
        }

        #endregion
        
    }
}
