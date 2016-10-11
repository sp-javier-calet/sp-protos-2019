using System;
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.Utils;
using SocialPoint.AdminPanel;
using SocialPoint.WebSockets;

public class WebSocketConnectionInstaller : Installer
{
    const string WebSocketConnectionTag = "websockets";
    const string DefaultEndpoint = "ws://echo.websocket.org";

    public enum WebsocketsTech
    {
        Sharp,
        Unity
    }

    [Serializable]
    public class SettingsData
    {
        public WebsocketsTech Tech = WebsocketsTech.Sharp;
        public string Endpoint = DefaultEndpoint;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        if(Settings.Tech == WebsocketsTech.Sharp)
        {
            Container.Rebind<WebSocketSharpClient>().ToMethod<WebSocketSharpClient>(CreateWebSocketSharpClient);
            Container.Rebind<INetworkClient>(WebSocketConnectionTag).ToLookup<WebSocketSharpClient>();
            Container.Bind<IDisposable>().ToLookup<WebSocketSharpClient>();
        }
        else
        {
            Container.Rebind<WebSocketUnityClient>().ToMethod<WebSocketUnityClient>(CreateWebSocketUnityClient);
            Container.Rebind<INetworkClient>(WebSocketConnectionTag).ToLookup<WebSocketUnityClient>();
            Container.Bind<IDisposable>().ToLookup<WebSocketUnityClient>();
        }

        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelWebSockets>(CreateAdminPanel);
    }

    WebSocketSharpClient CreateWebSocketSharpClient()
    {
        return new WebSocketSharpClient(Settings.Endpoint, Container.Resolve<ICoroutineRunner>());
    }

    WebSocketUnityClient CreateWebSocketUnityClient()
    {
        return new WebSocketUnityClient(Settings.Endpoint, Container.Resolve<ICoroutineRunner>());
    }

    AdminPanelWebSockets CreateAdminPanel()
    {
        return new AdminPanelWebSockets(Container.Resolve<INetworkClient>(WebSocketConnectionTag));
    }
}
