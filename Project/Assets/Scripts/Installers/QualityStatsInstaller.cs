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
    IHttpClient _httpClient;

    public override void InstallBindings()
    {
        if(Container.HasBinding<QualityStats>())
        {
            return;
        }
        if(!(_httpClient is QualityStatsHttpClient))
        {
            var httpClient = new QualityStatsHttpClient(_httpClient);
            Container.Rebind<IHttpClient>().ToInstance(httpClient);
            Container.Rebind<QualityStatsHttpClient>().ToInstance(httpClient);
            Container.Bind<IDisposable>().ToInstance(httpClient);
        }
        Container.Rebind<QualityStats>().ToSingle<QualityStats>();
        Container.Bind<IDisposable>().ToSingle<QualityStats>();
        Container.Resolve<QualityStats>();
    }
}
