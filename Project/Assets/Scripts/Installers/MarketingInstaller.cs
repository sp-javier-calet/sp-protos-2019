using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Login;
using SocialPoint.Marketing;
using System;
using Zenject;

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

    public IMarketingAttributionManager CreateMarketingAttributionManager(InjectContext context)
    {
        var manager = new MarketingAttributionManager(context.Container.Resolve<IAppEvents>(), context.Container.Resolve<IAttrStorage>("persistent"));
        manager.DebugMode = Settings.DebugMode;
        context.Container.Inject(manager);
        return manager;
    }

    public AdminPanelMarketing CreateAdminPanelMarketing(InjectContext context)
    {
        var adminPanel = new AdminPanelMarketing(context.Container.Resolve<IMarketingAttributionManager>(), context.Container.Resolve<IAttrStorage>("persistent"));
        context.Container.Inject(adminPanel);
        return adminPanel;
    }
}

