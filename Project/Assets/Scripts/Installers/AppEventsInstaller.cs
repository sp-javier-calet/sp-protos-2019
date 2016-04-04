using System;
using SocialPoint.Dependency;
using SocialPoint.AppEvents;
using SocialPoint.AdminPanel;
using SocialPoint.ScriptEvents;

public class AppEventsInstaller : MonoInstaller
{
	public override void InstallBindings()
	{
        Container.Rebind<IAppEvents>().ToSingle<SocialPointAppEvents>();
        Container.Bind<IDisposable>().ToLookup<IAppEvents>();

        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelAppEvents>();

        Container.Bind<IEventsBridge>().ToSingle<AppEventsBridge>();
        Container.Bind<IScriptEventsBridge>().ToSingle<AppEventsBridge>();
	}
}
