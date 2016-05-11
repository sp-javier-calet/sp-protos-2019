using System;
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.QualityStats;
using SocialPoint.Utils;
using SocialPoint.Hardware;
using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;

public class QualityStatsInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Rebind<QualityStatsHttpClient>().ToSingleMethod<QualityStatsHttpClient>(CreateHttpClient);
        Container.Bind<IDisposable>().ToLookup<QualityStatsHttpClient>();
        Container.Rebind<IHttpClient>().ToLookup<QualityStatsHttpClient>();
        Container.Rebind<SocialPointQualityStats>().ToSingleMethod<SocialPointQualityStats>(CreateQualityStats);
        Container.Bind<IDisposable>().ToLookup<SocialPointQualityStats>();
    }

    SocialPointQualityStats CreateQualityStats()
    {
        var stats = new SocialPointQualityStats(
            Container.Resolve<IDeviceInfo>(),
            Container.Resolve<IAppEvents>());

        stats.AddQualityStatsHttpClient(Container.Resolve<QualityStatsHttpClient>());
        stats.TrackEvent = Container.Resolve<IEventTracker>().TrackSystemEvent;

        return stats;
    }

    QualityStatsHttpClient CreateHttpClient()
    {
        var client = new HttpClient(Container.Resolve<ICoroutineRunner>());
        return new QualityStatsHttpClient(client);
    }
}
