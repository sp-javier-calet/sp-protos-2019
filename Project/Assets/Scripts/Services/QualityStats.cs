using Zenject;
using UnityEngine;
using SocialPoint.Network;
using SocialPoint.AppEvents;
using SocialPoint.Events;
using SocialPoint.Hardware;
using SocialPoint.QualityStats;

public class QualityStats : SocialPoint.QualityStats.SocialPointQualityStats
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
            TrackEvent = value.TrackEvent;
        }
    }


    public QualityStats(IDeviceInfo devInfo, IAppEvents appEvents):
    base(devInfo, appEvents)
    {
    }
}
