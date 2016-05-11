using System;
using SocialPoint.Dependency;
using SocialPoint.AppEvents;
using SocialPoint.AdminPanel;
using SocialPoint.ScriptEvents;

public class AppEventsInstaller : MonoInstaller
{
	public override void InstallBindings()
	{
        Container.Rebind<IAppEvents>().ToMethod<SocialPointAppEvents>(CreateAppEvents);
        Container.Bind<IDisposable>().ToLookup<IAppEvents>();

        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelAppEvents>(CreateAdminPanelAppEvents);

        Container.Bind<AppEventsBridge>().ToMethod<AppEventsBridge>(CreateAppEventsBridge);
        Container.Bind<IEventsBridge>().ToLookup<AppEventsBridge>();
        Container.Bind<IScriptEventsBridge>().ToLookup<AppEventsBridge>();
	}

    SocialPointAppEvents CreateAppEvents()
    {
        return new SocialPointAppEvents(Container.gameObject.transform);
    }

    AdminPanelAppEvents CreateAdminPanelAppEvents()
    {
        return new AdminPanelAppEvents(Container.Resolve<IAppEvents>());
    }

    AppEventsBridge CreateAppEventsBridge()
    {
        return new AppEventsBridge(Container.Resolve<IAppEvents>());
    }
}
