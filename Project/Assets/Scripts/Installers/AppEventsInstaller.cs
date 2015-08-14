using Zenject;
using System;
using SocialPoint.AppEvents;

public class AppEventsInstaller : MonoInstaller
{

	public override void InstallBindings()
	{
        Container.Bind<IAppEvents>().ToSingle<SocialPointAppEvents>();
	}

}
