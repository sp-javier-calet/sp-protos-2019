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

        readonly Dictionary<string, IMarketingTracker> _trackers;
        readonly IAppEvents _appEvents;

        IAttrStorage _storage;
        bool _appPreviouslyInstalled;
        #pragma warning disable 0414
        bool _tracked;
        //only used on DebugUtils.Assert, disabled warning of "unused" when compiling to iOS
        #pragma warning restore 0414

        public SocialPointMarketingAttributionManager(IAppEvents appEvents, IAttrStorage storage)
        {
            _trackers = new Dictionary<string, IMarketingTracker>();
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
            DebugUtils.Assert(!_trackers.ContainsValue(tracker), "SocialPointMarketingAttributionManager tracker already added");
            tracker.OnDataReceived += OnTrackerReceivedData;
            _trackers.Add(tracker.Name, tracker);
        }

        public IMarketingTracker GetTracker(string name)
        {
            IMarketingTracker tracker = null;
            if(_trackers.ContainsKey(name))
            {
                tracker = _trackers[name];
            }
            return tracker;
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

            var itr = _trackers.Values.GetEnumerator();
            while(itr.MoveNext())
            {
                var tracker = itr.Current;
                #if DEBUG
                tracker.SetDebugMode(DebugMode);
                #endif
                tracker.SetUserID(LoginData.UserId.ToString());

                tracker.TrackInstall(isNewInstall);
            }
            itr.Dispose();
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
            var itr = _trackers.Values.GetEnumerator();
            while(itr.MoveNext())
            {
                itr.Current.OnDataReceived -= OnTrackerReceivedData;
            }
            itr.Dispose();
            _appEvents.GameWasLoaded.Remove(OnGameLoaded);
        }

        #endregion
        
    }
}
