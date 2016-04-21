using System;
using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.Marketing
{
    public delegate string GetUserIDDelegate();

    public delegate void TrackEventDelegate(string eventName,AttrDic data = null,ErrorDelegate del = null);

    public interface IMarketingAttributionManager : IDisposable
    {
        void AddTracker(IMarketingTracker tracker);

        void OnGameLoaded();

        void TrackInstall();

        bool DebugMode { get; set; }

        void OnTrackerReceivedData(TrackerAttributionData data);

        GetUserIDDelegate GetUserID { get; set; }

        TrackEventDelegate TrackEvent { get; set; }
    }
}
