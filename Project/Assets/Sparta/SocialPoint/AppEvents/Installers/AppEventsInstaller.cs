using System;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.AppEvents;
using SocialPoint.ScriptEvents;
using SocialPoint.Restart;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

public class AppEventsInstaller : ServiceInstaller
{
    public override void InstallBindings()
    {
        Container.Rebind<IAppEvents>().ToMethod<SocialPointAppEvents>(CreateAppEvents);
        Container.Bind<IDisposable>().ToLookup<IAppEvents>();

        #if ADMIN_PANEL
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelAppEvents>(CreateAdminPanelAppEvents);
        #endif

        Container.Bind<AppEventsBridge>().ToMethod<AppEventsBridge>(CreateAppEventsBridge);
        Container.Bind<IEventsBridge>().ToLookup<AppEventsBridge>();
        Container.Bind<IScriptEventsBridge>().ToLookup<AppEventsBridge>();
    }

    SocialPointAppEvents CreateAppEvents()
    {
        return new SocialPointAppEvents(Container.Resolve<Transform>());
    }

    #if ADMIN_PANEL
    AdminPanelAppEvents CreateAdminPanelAppEvents()
    {
        return new AdminPanelAppEvents(Container.Resolve<IAppEvents>(), Container.Resolve<IRestarter>());
    }
    #endif

    AppEventsBridge CreateAppEventsBridge()
    {
        return new AppEventsBridge(Container.Resolve<IAppEvents>());
    }
}
