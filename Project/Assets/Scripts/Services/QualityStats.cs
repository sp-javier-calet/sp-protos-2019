using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;
using SocialPoint.Hardware;
using SocialPoint.QualityStats;
using SocialPoint.Dependency;

public class QualityStats : SocialPointQualityStats
{
    public QualityStats(IDeviceInfo devInfo, IAppEvents appEvents) :
        base(devInfo, appEvents)
    {
        AddQualityStatsHttpClient(ServiceLocator.Instance.Resolve<QualityStatsHttpClient>());
        TrackEvent = ServiceLocator.Instance.Resolve<IEventTracker>().TrackSystemEvent;
    }
}
