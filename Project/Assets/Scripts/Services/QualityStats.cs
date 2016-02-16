using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;
using SocialPoint.Hardware;
using SocialPoint.QualityStats;
using Zenject;

public class QualityStats : SocialPointQualityStats
{
    [Inject]
    QualityStatsHttpClient injectQualityStatsHttpClient
    {
        set
        {
            AddQualityStatsHttpClient(value);
        }
    }

    [Inject]
    IEventTracker injectEventTracker
    {
        set
        {
            TrackEvent = value.TrackSystemEvent;
        }
    }

    public QualityStats(IDeviceInfo devInfo, IAppEvents appEvents) :
        base(devInfo, appEvents)
    {
    }
}
