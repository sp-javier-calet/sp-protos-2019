using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Login;
using SocialPoint.Dependency;
using System;
using SocialPoint.Utils;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
using SocialPoint.Alert;
using SocialPoint.Hardware;
#endif

namespace SocialPoint.Marketing
{
    public class MarketingInstaller : ServiceInstaller, IInitializable
    {
        [Serializable]
        public class SettingsData
        {
            public bool DebugMode;
        }

        public SettingsData Settings;

        public override void InstallBindings()
        {
            Container.Bind<IInitializable>().ToInstance(this);

            Container.Rebind<IMarketingAttributionManager>().ToMethod<IMarketingAttributionManager>(CreateMarketingAttributionManager, SetupMarketingAttributionManager);
            Container.Bind<IDisposable>().ToLookup<IMarketingAttributionManager>();

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelMarketing>(CreateAdminPanelMarketing);
            #endif
        }

        public void Initialize()
        {
            Container.Resolve<IMarketingAttributionManager>();
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

        #if ADMIN_PANEL
        public AdminPanelMarketing CreateAdminPanelMarketing()
        {
            var adminPanel = new AdminPanelMarketing(
                                 Container.Resolve<IMarketingAttributionManager>(), 
                                 Container.Resolve<IAttrStorage>("persistent"),
                                 Container.Resolve<IDeviceInfo>(),
                                 Container.Resolve<INativeUtils>(),
                                 Container.Resolve<IAppEvents>(),
                                 Container.Resolve<IAlertView>()
                             );
            return adminPanel;
        }
        #endif
    }
}

