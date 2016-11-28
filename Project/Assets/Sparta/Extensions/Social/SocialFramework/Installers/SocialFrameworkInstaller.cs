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

    const string DefaultWAMPProtocol = "wamp.2.json";
    const string DefaultEndpoint = "ws://sprocket-00.int.lod.laicosp.net:8002/ws";

    [Serializable]
    public class SettingsData
    {
        public string[] Endpoints = new string[] { DefaultEndpoint };
        public string[] Protocols = new string[] { DefaultWAMPProtocol };
    }

    public SettingsData Settings = new SettingsData();

    string _httpProxy;
    IDeviceInfo _deviceInfo;

    public override void InstallBindings()
    {
        _httpProxy = EditorProxy.GetProxy();
        _deviceInfo = Container.Resolve<IDeviceInfo>();

        // Service Installer
        Container.Rebind<WebSocketClient>().ToMethod<WebSocketClient>(CreateWebSocket, SetupWebSocket);
        Container.Rebind<IWebSocketClient>(SocialFrameworkTag).ToLookup<WebSocketClient>();
        Container.Bind<IDisposable>().ToLookup<WebSocketClient>();

        Container.Bind<ConnectionManager>().ToMethod<ConnectionManager>(CreateConnectionManager, SetupConnectionManager);    
        Container.Bind<IDisposable>().ToLookup<ConnectionManager>();

        Container.Bind<ChatManager>().ToMethod<ChatManager>(CreateChatManager, SetupChatManager);
        Container.Bind<IDisposable>().ToLookup<ChatManager>();

        Container.Bind<AlliancesManager>().ToMethod<AlliancesManager>(CreateAlliancesManager, SetupAlliancesManager);
        Container.Bind<IDisposable>().ToLookup<AlliancesManager>();

        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelSocialFramework>(CreateAdminPanelSocialFramework);
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelWebSockets>(CreateAdminPanelWebSockets);

        Container.Listen<IChatRoom>().WhenResolved(SetupChatRoom);
    }

    WebSocketClient CreateWebSocket()
    {
        return new WebSocketClient(Settings.Endpoints, Settings.Protocols, Container.Resolve<IUpdateScheduler>());
    }

    void SetupWebSocket(WebSocketClient client)
    {
        if(!string.IsNullOrEmpty(_httpProxy))
        {
            client.Proxy = _httpProxy;
        }
        else if(_deviceInfo.NetworkInfo.Proxy != null)
        {
            client.Proxy = _deviceInfo.NetworkInfo.Proxy.ToString();
        }
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

    void SetupChatManager(ChatManager manager)
    {
        manager.Register(Container.ResolveList<IChatRoom>());
    }

    AlliancesManager CreateAlliancesManager()
    {
        return new AlliancesManager(
            Container.Resolve<ConnectionManager>());
    }

    void SetupAlliancesManager(AlliancesManager manager)
    {
        manager.LoginData = Container.Resolve<ILoginData>();

        if(Container.HasBinding<AllianceDataFactory>())
        {
            manager.Factory = Container.Resolve<AllianceDataFactory>();
        }
    }

    AdminPanelSocialFramework CreateAdminPanelSocialFramework()
    {
        return new AdminPanelSocialFramework(
            Container.Resolve<ConnectionManager>(),
            Container.Resolve<ChatManager>(),
            Container.Resolve<AlliancesManager>());
    }

    AdminPanelWebSockets CreateAdminPanelWebSockets()
    {
        return new AdminPanelWebSockets(
            Container.Resolve<IWebSocketClient>(SocialFrameworkTag),
            SocialFrameworkTag);
    }

    void SetupChatRoom(IChatRoom room)
    {
        room.ChatManager = Container.Resolve<ChatManager>();
        room.Localization = Container.Resolve<Localization>();
    }
}
