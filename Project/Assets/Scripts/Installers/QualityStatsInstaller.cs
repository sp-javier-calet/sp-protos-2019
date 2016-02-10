using System;
using SocialPoint.Network;
using SocialPoint.QualityStats;
using Zenject;

public class QualityStatsInstaller : Installer, IInitializable
{
    [Inject]
    IHttpClient _httpClient;

    public override void InstallBindings()
    {
        Container.Rebind<QualityStatsHttpClient>().ToSingle<QualityStatsHttpClient>();
        Container.Bind<IDisposable>().ToLookup<QualityStatsHttpClient>();
        Container.Rebind<QualityStats>().ToSingle<QualityStats>();
        Container.Bind<IDisposable>().ToSingle<QualityStats>();
    }

    public void Initialize()
    {
        var client = Container.Resolve<QualityStatsHttpClient>();
        Container.Rebind<IHttpClient>().ToSingleInstance(client);
    }        
}
