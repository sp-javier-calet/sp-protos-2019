using System;
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.QualityStats;
using SocialPoint.Utils;
using SocialPoint.Hardware;
using SocialPoint.AppEvents;

public class QualityStatsInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Rebind<QualityStatsHttpClient>().ToSingleMethod<QualityStatsHttpClient>(CreateHttpClient);
        Container.Bind<IDisposable>().ToLookup<QualityStatsHttpClient>();
        Container.Rebind<IHttpClient>().ToLookup<QualityStatsHttpClient>();
        Container.Rebind<QualityStats>().ToSingleMethod<QualityStats>(CreateQualityStats);
        Container.Bind<IDisposable>().ToLookup<QualityStats>();
    }

    QualityStats CreateQualityStats()
    {
        return new QualityStats(
            Container.Resolve<IDeviceInfo>(),
            Container.Resolve<IAppEvents>());
    }

    QualityStatsHttpClient CreateHttpClient()
    {
        var client = new HttpClient(Container.Resolve<ICoroutineRunner>());
        return new QualityStatsHttpClient(client);
    }
}
