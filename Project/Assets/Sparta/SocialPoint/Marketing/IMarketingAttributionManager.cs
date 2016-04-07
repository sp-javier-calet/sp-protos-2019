using System;

namespace SocialPoint.Marketing
{
    public interface IMarketingAttributionManager : IDisposable
    {
        void AddTracker(IMarketingTracker tracker);

        void OnGameLoaded();

        void TrackInstall();

        bool DebugMode { get; set; }

        void OnTrackerReceivedData(TrackerAttributionData data);
    }
}
