using System;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.Network;
using SocialPoint.ServerEvents;

namespace SocialPoint.QualityStats
{
    public class QualityStatsInstaller : SubInstaller, IInitializable
    {
        public override void InstallBindings()
        {
            Container.Rebind<QualityStatsHttpClient>().ToMethod<QualityStatsHttpClient>(CreateHttpClient);
            Container.Bind<IDisposable>().ToLookup<QualityStatsHttpClient>();
            Container.Rebind<IHttpClient>().ToLookup<QualityStatsHttpClient>();
            Container.Rebind<SocialPointQualityStats>().ToMethod<SocialPointQualityStats>(CreateQualityStats, SetupQualityStats);
            Container.Bind<IDisposable>().ToLookup<SocialPointQualityStats>();

            Container.Bind<IInitializable>().ToInstance(this);
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

        public void Initialize()
        {
            Container.Resolve<SocialPointQualityStats>();
        }
    }
}
