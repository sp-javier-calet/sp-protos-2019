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
using SocialPoint.WebSockets;

public class SocialFrameworkInstaller : Installer
{
    const string SocialFrameworkTag = "social_framework";
    const string DefaultEndpoint = "ws://int-mc-sprocket.socialpointgames.com:8050/ws";

    [Serializable]
    public class SettingsData
    {
        public string Endpoint = DefaultEndpoint;
    }

    public SettingsData Settings;

    public override void InstallBindings()
    {   
        Container.Rebind<WebSocketSharpClient>().ToMethod<WebSocketSharpClient>(CreateWebSocket);
        Container.Rebind<IWebSocketClient>(SocialFrameworkTag).ToLookup<WebSocketSharpClient>();
        Container.Bind<IDisposable>().ToLookup<WebSocketSharpClient>();

        Container.Bind<ConnectionManager>().ToMethod<ConnectionManager>(CreateConnectionManager, SetupConnectionManager);    
        Container.Bind<IDisposable>().ToLookup<ConnectionManager>();

        Container.Bind<ChatManager>().ToMethod<ChatManager>(CreateChatManager);
        Container.Bind<IDisposable>().ToLookup<ChatManager>();

        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelSocialFramework>(CreateAdminPanelSocialFramework);
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelWebSockets>(CreateAdminPanelWebSockets);
    }

    WebSocketSharpClient CreateWebSocket()
    {
        return new WebSocketSharpClient(Settings.Endpoint, Container.Resolve<ICoroutineRunner>());
    }

    ConnectionManager CreateConnectionManager()
    {
        return new ConnectionManager(Container.Resolve<IWebSocketClient>(SocialFrameworkTag));
    }

    void SetupConnectionManager(ConnectionManager manager)
    {
        manager.AppEvents = Container.Resolve<IAppEvents>();
        manager.Scheduler = Container.Resolve<IUpdateScheduler>();
        manager.LoginData = Container.Resolve<ILoginData>();
        manager.PlayerData = Container.Resolve<IPlayerData>();
        manager.DeviceInfo = Container.Resolve<IDeviceInfo>();
        manager.Localization = Container.Resolve<Localization>();
    }

    ChatManager CreateChatManager()
    {
        return new ChatManager(
            Container.Resolve<ConnectionManager>());
    }

    AdminPanelSocialFramework CreateAdminPanelSocialFramework()
    {
        return new AdminPanelSocialFramework(
            Container.Resolve<ConnectionManager>(),
            Container.Resolve<ChatManager>());
    }

    AdminPanelWebSockets CreateAdminPanelWebSockets()
    {
        return new AdminPanelWebSockets(
            Container.Resolve<IWebSocketClient>(SocialFrameworkTag),
            SocialFrameworkTag);
    }
}
