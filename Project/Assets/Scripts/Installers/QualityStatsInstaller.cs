using System;
using SocialPoint.Network;
using SocialPoint.QualityStats;
using Zenject;

public class QualityStatsInstaller : Installer
{
    [Inject]
    IHttpClient _httpClient;

    public override void InstallBindings()
    {
        if(!(_httpClient is QualityStatsHttpClient))
        {
            var httpClient = new QualityStatsHttpClient(_httpClient);
            Container.Rebind<IHttpClient>().ToInstance(httpClient);
            Container.Rebind<QualityStatsHttpClient>().ToInstance(httpClient);
            Container.Bind<IDisposable>().ToInstance(httpClient);
        }
        Container.Rebind<QualityStats>().ToSingle<QualityStats>();
        Container.Bind<IDisposable>().ToSingle<QualityStats>();
    }
}
