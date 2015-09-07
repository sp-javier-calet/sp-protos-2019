using System;
using System.Collections;
using Zenject;
using SocialPoint.QualityStats;
using SocialPoint.Hardware;
using SocialPoint.AppEvents;
using SocialPoint.Events;
using SocialPoint.Network;

public class QualityStatsInstaller : MonoInstaller
{

    [Inject]
    IDeviceInfo DeviceInfo;

    [Inject]
    IAppEvents AppEvents;

    [Inject]
    IHttpClient HttpClient;
        
    [Inject]
    public IEventTracker EventTracker;

    public override void InstallBindings()
    {
        if(Container.HasBinding<SocialPointQualityStats>())
        {
            return;
        }
        var httpClient = new QualityStatsHttpClient(HttpClient);
        Container.Rebind<IHttpClient>().ToInstance(httpClient);

        var stats = new SocialPointQualityStats(DeviceInfo, AppEvents);
        stats.TrackEvent = EventTracker.TrackEvent;
        stats.AddQualityStatsHttpClient(httpClient);
        Container.BindInstance(stats);
    }
}
