using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Login;
using System;

namespace SocialPoint.Marketing
{
    public delegate void TrackEventDelegate(string eventName, AttrDic data = null, ErrorDelegate del = null);

    public interface IMarketingAttributionManager : IDisposable
    {
        void AddTracker(IMarketingTracker tracker);

        IMarketingTracker GetTracker(string name);

        void OnGameLoaded();

        void TrackInstall();

        bool DebugMode { get; set; }

        void OnTrackerReceivedData(TrackerAttributionData data);

        ILoginData LoginData { get; set; }

        TrackEventDelegate TrackEvent { get; set; }
    }
}
