using System;
using SocialPoint.Network;
using SocialPoint.QualityStats;
using Zenject;

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

    QualityStatsHttpClient CreateHttpClient(InjectContext ctx)
    {
        var client = ctx.Container.Instantiate<HttpClient>();
        return new QualityStatsHttpClient(client);
    }
}
