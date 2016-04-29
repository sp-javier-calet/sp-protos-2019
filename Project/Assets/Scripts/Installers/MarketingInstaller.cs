using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Login;
using SocialPoint.Marketing;
using SocialPoint.Dependency;
using System;

public class MarketingInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public bool DebugMode;
    }

    public SettingsData Settings;

    public override void InstallBindings()
    {
        Container.Bind<IMarketingAttributionManager>().ToSingleMethod<IMarketingAttributionManager>(CreateMarketingAttributionManager);
        Container.Bind<IDisposable>().ToLookup<IMarketingAttributionManager>();
        Container.Bind<IAdminPanelConfigurer>().ToSingleMethod<AdminPanelMarketing>(CreateAdminPanelMarketing);
    }

    public IMarketingAttributionManager CreateMarketingAttributionManager()
    {
        var manager = new MarketingAttributionManager(Container.Resolve<IAppEvents>(), Container.Resolve<IAttrStorage>("persistent"));
        manager.DebugMode = Settings.DebugMode;
        return manager;
    }

    public AdminPanelMarketing CreateAdminPanelMarketing()
    {
        var adminPanel = new AdminPanelMarketing(Container.Resolve<IMarketingAttributionManager>(), Container.Resolve<IAttrStorage>("persistent"));
        return adminPanel;
    }
}

