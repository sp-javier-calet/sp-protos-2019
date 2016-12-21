using System;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.AppEvents;
using SocialPoint.AdminPanel;
using SocialPoint.ScriptEvents;

public class AppEventsInstaller : ServiceInstaller
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
        return new SocialPointAppEvents(Container.Resolve<Transform>());
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
