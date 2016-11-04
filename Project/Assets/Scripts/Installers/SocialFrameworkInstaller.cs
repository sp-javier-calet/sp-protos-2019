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
    const string DefaultSocketEndpoint = "ws://sprocket-00.int.lod.laicosp.net:8001/ws";
    const string DefaultAlliancesServer = "";

    [Serializable]
    public class SettingsData
    {
        public string AlliancesServer = DefaultAlliancesServer;
        public string SocketEndpoint = DefaultSocketEndpoint;
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
        Container.Rebind<WebSocketSharpClient>().ToMethod<WebSocketSharpClient>(CreateWebSocket, SetupWebSocket);
        Container.Rebind<IWebSocketClient>(SocialFrameworkTag).ToLookup<WebSocketSharpClient>();
        Container.Bind<IDisposable>().ToLookup<WebSocketSharpClient>();

        Container.Bind<ConnectionManager>().ToMethod<ConnectionManager>(CreateConnectionManager, SetupConnectionManager);    
        Container.Bind<IDisposable>().ToLookup<ConnectionManager>();

        Container.Bind<ChatManager>().ToMethod<ChatManager>(CreateChatManager, SetupChatManager);
        Container.Bind<IDisposable>().ToLookup<ChatManager>();

        Container.Bind<AlliancesManager>().ToMethod<AlliancesManager>(CreateAlliancesManager, SetupAlliancesManager);
        Container.Bind<IDisposable>().ToLookup<AlliancesManager>();

        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelSocialFramework>(CreateAdminPanelSocialFramework);
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelWebSockets>(CreateAdminPanelWebSockets);

        // Game chat rooms
        Container.Bind<IChatRoom>().ToMethod<ChatRoom<PublicChatMessage>>(CreatePublicChatRoom, SetupPublicChatRoom);
        Container.Bind<IChatRoom>().ToMethod<ChatRoom<AllianceChatMessage>>(CreateAllianceChatRoom, SetupAllianceChatRoom);
    }

    ChatRoom<PublicChatMessage> CreatePublicChatRoom()
    {
        return new ChatRoom<PublicChatMessage>("public");
    }

    void SetupPublicChatRoom(ChatRoom<PublicChatMessage> room)
    {
        room.ChatManager = Container.Resolve<ChatManager>();
        room.Localization = Container.Resolve<Localization>();

        // Configure optional events to manage custom data
        room.ParseUnknownNotifications = PublicChatMessage.ParseUnknownNotifications;
        room.ParseExtraInfo = PublicChatMessage.ParseExtraInfo;
        room.SerializeExtraInfo = PublicChatMessage.SerializeExtraInfo;
    }

    ChatRoom<AllianceChatMessage> CreateAllianceChatRoom()
    {
        return new ChatRoom<AllianceChatMessage>("alliance");
    }

    void SetupAllianceChatRoom(ChatRoom<AllianceChatMessage> room)
    {
        room.ChatManager = Container.Resolve<ChatManager>();
        room.Localization = Container.Resolve<Localization>();

        // Configure optional events to manage custom data
        room.ParseUnknownNotifications = AllianceChatMessage.ParseUnknownNotifications;
        room.ParseExtraInfo = AllianceChatMessage.ParseExtraInfo;
        room.SerializeExtraInfo = AllianceChatMessage.SerializeExtraInfo;
    }

    WebSocketSharpClient CreateWebSocket()
    {
        return new WebSocketSharpClient(Settings.SocketEndpoint, Settings.Protocols, Container.Resolve<ICoroutineRunner>());
    }

    void SetupWebSocket(WebSocketSharpClient client)
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
        manager.AlliancesServerUrl = Settings.AlliancesServer;
        manager.HttpClient = Container.Resolve<IHttpClient>();
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
}
