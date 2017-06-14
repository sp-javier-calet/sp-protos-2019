using System;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.Utils;
using SocialPoint.WebSockets;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Connection
{
    public class ConnectionInstaller : ServiceInstaller
    {
        const string ConnectionManagerTag = "connection_manager";

        const string DefaultWAMPProtocol = "wamp.2.json";
        const string DefaultEndpoint = "ws://sprocket-00.int.lod.laicosp.net:8002/ws";

        [Serializable]
        public class SettingsData
        {
            public string[] Endpoints = { DefaultEndpoint };
            public string[] Protocols = { DefaultWAMPProtocol };
            public bool UseNativeWebsocketIfSupported = true;

            public ConnectionManagerConfig Config = new ConnectionManagerConfig();
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            if(Settings.UseNativeWebsocketIfSupported && WebSocket.IsSupported)
            {
                Container.Rebind<WebSocketClient>().ToMethod<WebSocketClient>(CreateWebSocket, SetupWebSocket);
                Container.Rebind<IWebSocketClient>(ConnectionManagerTag).ToLookup<WebSocketClient>();
                Container.Bind<IDisposable>().ToLookup<WebSocketClient>();
            }
            else
            {
                Container.Rebind<WebSocketSharpClient>().ToMethod<WebSocketSharpClient>(CreateWebSocketSharp, SetupWebSocket);
                Container.Rebind<IWebSocketClient>(ConnectionManagerTag).ToLookup<WebSocketSharpClient>();
                Container.Bind<IDisposable>().ToLookup<WebSocketSharpClient>();
            }

            Container.Bind<ConnectionManager>().ToMethod<ConnectionManager>(CreateConnectionManager, SetupConnectionManager);    
            Container.Bind<IDisposable>().ToLookup<ConnectionManager>();

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelWebSockets>(CreateAdminPanelWebSockets);
            #endif
        }

        WebSocketClient CreateWebSocket()
        {
            return new WebSocketClient(Settings.Endpoints, Settings.Protocols, Container.Resolve<IUpdateScheduler>());
        }

        WebSocketSharpClient CreateWebSocketSharp()
        {
            return new WebSocketSharpClient(Settings.Endpoints, Settings.Protocols, Container.Resolve<IUpdateScheduler>());
        }

        void SetupWebSocket(IWebSocketClient client)
        {
            var deviceInfo = Container.Resolve<IDeviceInfo>();
            var httpProxy = EditorProxy.GetProxy();
            if(!string.IsNullOrEmpty(httpProxy))
            {
                client.Proxy = httpProxy;
            }
            else if(deviceInfo.NetworkInfo.Proxy != null)
            {
                client.Proxy = deviceInfo.NetworkInfo.Proxy.ToString();
            }
        }

        ConnectionManager CreateConnectionManager()
        {
            return new ConnectionManager(Container.Resolve<IWebSocketClient>(ConnectionManagerTag), Settings.Config);
        }

        void SetupConnectionManager(ConnectionManager manager)
        {
            manager.AppEvents = Container.Resolve<IAppEvents>();
            manager.Scheduler = Container.Resolve<IUpdateScheduler>();
            manager.LoginData = Container.Resolve<ILoginData>();
            manager.PlayerData = Container.Resolve<SocialPoint.Social.IPlayerData>();
            manager.DeviceInfo = Container.Resolve<IDeviceInfo>();
            manager.Localization = Container.Resolve<Localization>();
        }

        #if ADMIN_PANEL
        AdminPanelWebSockets CreateAdminPanelWebSockets()
        {
            return new AdminPanelWebSockets(
                Container.Resolve<IWebSocketClient>(ConnectionManagerTag),
                ConnectionManagerTag);
        }
        #endif
    }
}