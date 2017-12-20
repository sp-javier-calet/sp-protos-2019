using System;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.Network;
using SocialPoint.ServerEvents;
using SocialPoint.Utils;

namespace SocialPoint.QualityStats
{
    public class QualityStatsInstaller : SubInstaller, IInitializable
    {
        [Serializable]
        public class SettingsData
        {
            public bool UseEmpty;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            if(!Settings.UseEmpty)
            {
                Container.Rebind<QualityStatsHttpClient>().ToMethod<QualityStatsHttpClient>(CreateHttpClient);
                Container.Bind<IDisposable>().ToLookup<QualityStatsHttpClient>();
                Container.Rebind<IHttpClient>().ToLookup<QualityStatsHttpClient>(); //Dispose "internal"??? FIXME
                Container.Rebind<SocialPointQualityStats>().ToMethod<SocialPointQualityStats>(CreateQualityStats, SetupQualityStats);
                Container.Bind<IDisposable>().ToLookup<SocialPointQualityStats>();

                Container.Bind<IInitializable>().ToInstance(this);
            }
        }

        SocialPointQualityStats CreateQualityStats()
        {
            return new SocialPointQualityStats(
                Container.Resolve<IDeviceInfo>(),
                Container.Resolve<IAppEvents>(),
                Container.Resolve<IUpdateScheduler>());
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
