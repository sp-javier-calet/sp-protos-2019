using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Login;
using SocialPoint.Dependency;
using System;

namespace SocialPoint.Marketing
{
    public class MarketingInstaller : ServiceInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public bool DebugMode;
        }

        public SettingsData Settings;

        public override void InstallBindings()
        {
            Container.Rebind<IMarketingAttributionManager>().ToMethod<IMarketingAttributionManager>(CreateMarketingAttributionManager, SetupMarketingAttributionManager);
            Container.Bind<IDisposable>().ToLookup<IMarketingAttributionManager>();
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelMarketing>(CreateAdminPanelMarketing);
        }

        public IMarketingAttributionManager CreateMarketingAttributionManager()
        {
            return new SocialPointMarketingAttributionManager(Container.Resolve<IAppEvents>(), Container.Resolve<IAttrStorage>("persistent"));
        }

        public void SetupMarketingAttributionManager(IMarketingAttributionManager manager)
        {
            manager.DebugMode = Settings.DebugMode;

            var trackers = Container.ResolveList<IMarketingTracker>();
            for(int i = 0; i < trackers.Count; i++)
            {
                manager.AddTracker(trackers[i]);
            }

            manager.LoginData = Container.Resolve<ILoginData>();
        }

        public AdminPanelMarketing CreateAdminPanelMarketing()
        {
            var adminPanel = new AdminPanelMarketing(Container.Resolve<IMarketingAttributionManager>(), Container.Resolve<IAttrStorage>("persistent"));
            return adminPanel;
        }
    }
}

