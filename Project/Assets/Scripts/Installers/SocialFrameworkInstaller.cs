using System;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.Utils;
using SocialPoint.Social;

public class SocialFrameworkInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
    }

    public override void InstallBindings()
    {   
        Container.Bind<ConnectionManager>().ToMethod<ConnectionManager>(CreateConnectionManager);    
        Container.Bind<ConnectionManager>().ToMethod<ConnectionManager>(CreateConnectionManager);
    }

    ConnectionManager CreateConnectionManager()
    {
        return new ConnectionManager(
            Container.Resolve<ChatManager>(),
            Container.Resolve<IAppEvents>(),
            Container.Resolve<IUpdateScheduler>());
    }
}
