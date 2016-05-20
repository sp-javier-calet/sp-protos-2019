using System;
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.QualityStats;
using SocialPoint.Utils;
using SocialPoint.Hardware;
using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;

public class QualityStatsInstaller : SubInstaller
{
    public override void InstallBindings()
    {
        Container.Rebind<QualityStatsHttpClient>().ToMethod<QualityStatsHttpClient>(CreateHttpClient);
        Container.Bind<IDisposable>().ToLookup<QualityStatsHttpClient>();
        Container.Rebind<IHttpClient>().ToLookup<QualityStatsHttpClient>();
        Container.Rebind<SocialPointQualityStats>().ToMethod<SocialPointQualityStats>(CreateQualityStats, SetupQualityStats);
        Container.Bind<IDisposable>().ToLookup<SocialPointQualityStats>();
    }

    SocialPointQualityStats CreateQualityStats()
    {
        return new SocialPointQualityStats(
            Container.Resolve<IDeviceInfo>(),
            Container.Resolve<IAppEvents>());
    }

    void SetupQualityStats(SocialPointQualityStats stats)
    {
        stats.AddQualityStatsHttpClient(Container.Resolve<QualityStatsHttpClient>());
        stats.TrackEvent = Container.Resolve<IEventTracker>().TrackSystemEvent;
    }

    QualityStatsHttpClient CreateHttpClient()
    {
        return new QualityStatsHttpClient(Container.Resolve<IHttpClient>("internal"));
    }
}
