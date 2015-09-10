using Zenject;
using System;
using SocialPoint.AppEvents;

public class AppEventsInstaller : MonoInstaller
{
	public override void InstallBindings()
	{
        if(Container.HasBinding<IAppEvents>())
        {
            return;
        }
        Container.BindAllInterfacesToSingle<SocialPointAppEvents>();
	}
}
