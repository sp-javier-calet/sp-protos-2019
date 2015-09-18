using Zenject;
using System;
using SocialPoint.AppEvents;
using SocialPoint.AdminPanel;

public class AppEventsInstaller : MonoInstaller
{
	public override void InstallBindings()
	{
        if(Container.HasBinding<IAppEvents>())
        {
            return;
        }
        Container.BindAllInterfacesToSingle<SocialPointAppEvents>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelAppEvents>();
	}
}
