using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Login;
using SocialPoint.Marketing;
using SocialPoint.Dependency;
using System;

public class MarketingInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public bool DebugMode;
    }

    public SettingsData Settings;

    public override void InstallBindings()
    {
        Container.Bind<IMarketingAttributionManager>().ToMethod<IMarketingAttributionManager>(CreateMarketingAttributionManager);
        Container.Bind<IDisposable>().ToLookup<IMarketingAttributionManager>();
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelMarketing>(CreateAdminPanelMarketing);
    }

    public IMarketingAttributionManager CreateMarketingAttributionManager()
    {
        var manager = new SocialPointMarketingAttributionManager(Container.Resolve<IAppEvents>(), Container.Resolve<IAttrStorage>("persistent"));
        manager.DebugMode = Settings.DebugMode;
        var login = Container.Resolve<ILogin>();
        var trackers = Container.ResolveList<IMarketingTracker>();
        for(int i = 0; i < trackers.Count; i++)
        {
            manager.AddTracker(trackers[i]);
        }
        manager.GetUserID = () => login.UserId.ToString();
        return manager;
    }

    public AdminPanelMarketing CreateAdminPanelMarketing()
    {
        var adminPanel = new AdminPanelMarketing(Container.Resolve<IMarketingAttributionManager>(), Container.Resolve<IAttrStorage>("persistent"));
        return adminPanel;
    }
}

