using System;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.Login;
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
        Container.Bind<ConnectionManager>().ToMethod<ConnectionManager>(CreateConnectionManager, SetupConnectionManager);    
        Container.Bind<ChatManager>().ToMethod<ChatManager>(CreateChatManager);
    }

    ConnectionManager CreateConnectionManager()
    {
        return new ConnectionManager();
    }

    void SetupConnectionManager(ConnectionManager manager)
    {
        manager.AppEvents = Container.Resolve<IAppEvents>();
        manager.Scheduler = Container.Resolve<IUpdateScheduler>();
        manager.LoginData = Container.Resolve<ILoginData>();
        manager.DeviceInfo = Container.Resolve<IDeviceInfo>();
    }

    ChatManager CreateChatManager()
    {
        return new ChatManager(
            Container.Resolve<ConnectionManager>());
    }
}
