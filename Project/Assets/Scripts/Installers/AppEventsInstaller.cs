using Zenject;
using System;
using SocialPoint.AppEvents;
using SocialPoint.AdminPanel;

public class AppEventsInstaller : MonoInstaller
{
	public override void InstallBindings()
	{
        Container.Rebind<IAppEvents>().ToSingle<SocialPointAppEvents>();
        Container.Bind<IDisposable>().ToLookup<IAppEvents>();

        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelAppEvents>();
	}
}
