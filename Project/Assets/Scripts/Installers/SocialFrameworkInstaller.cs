using System;
using SocialPoint.AppEvents;
using SocialPoint.AdminPanel;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.Login;
using SocialPoint.Locale;
using SocialPoint.Network;
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
        Container.Bind<IDisposable>().ToLookup<ConnectionManager>();

        Container.Bind<ChatManager>().ToMethod<ChatManager>(CreateChatManager);
        Container.Bind<IDisposable>().ToLookup<ChatManager>();

        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelSocialFramework>(CreateAdminPanel);
    }

    ConnectionManager CreateConnectionManager()
    {
        return new ConnectionManager(Container.Resolve<INetworkClient>());
    }

    void SetupConnectionManager(ConnectionManager manager)
    {
        manager.AppEvents = Container.Resolve<IAppEvents>();
        manager.Scheduler = Container.Resolve<IUpdateScheduler>();
        manager.LoginData = Container.Resolve<ILoginData>();
        manager.DeviceInfo = Container.Resolve<IDeviceInfo>();
        manager.Localization = Container.Resolve<Localization>();
    }

    ChatManager CreateChatManager()
    {
        return new ChatManager(
            Container.Resolve<ConnectionManager>());
    }

    AdminPanelSocialFramework CreateAdminPanel()
    {
        return new AdminPanelSocialFramework(
            Container.Resolve<ConnectionManager>(),
            Container.Resolve<ChatManager>());
    }
}
