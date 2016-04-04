using System;
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.QualityStats;
using SocialPoint.Utils;

public class QualityStatsInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Rebind<QualityStatsHttpClient>().ToSingleMethod<QualityStatsHttpClient>(CreateHttpClient);
        Container.Bind<IDisposable>().ToLookup<QualityStatsHttpClient>();
        Container.Rebind<IHttpClient>().ToLookup<QualityStatsHttpClient>();
        Container.Rebind<QualityStats>().ToSingle<QualityStats>();
        Container.Bind<IDisposable>().ToSingle<QualityStats>();
    }

    QualityStatsHttpClient CreateHttpClient()
    {
        var client = new HttpClient(Container.Resolve<ICoroutineRunner>());
        return new QualityStatsHttpClient(client);
    }
}
