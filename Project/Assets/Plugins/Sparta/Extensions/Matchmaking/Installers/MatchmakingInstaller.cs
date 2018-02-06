
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.WebSockets;
using SocialPoint.Login;
using SocialPoint.Utils;
using SocialPoint.Attributes;
using System;

namespace SocialPoint.Matchmaking
{
    public class MatchmakingInstaller : Installer
    {
        [Serializable]
        public class SettingsData
        {
            const string DefaultBaseUrl = "http://int-lod.socialpointgames.es";
            const string DefaultWebsocketUrl = "ws://int-lod.socialpointgames.com:8001/find_opponent";
            public string BaseUrl = DefaultBaseUrl;
            public string[] WebsocketUrls = new string[] { DefaultWebsocketUrl };
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Rebind<IMatchmakingServer>().ToMethod<HttpMatchmakingServer>
                (CreateServer, SetupServer);
            Container.Rebind<IMatchmakingClient>().ToMethod<StoredMatchmakingClient>
                (CreateClient, SetupClient);

            Container.Bind<IMatchmakingClientDelegate>().ToMethod<PhotonMatchmakingClientDelegate>(CreatePhotonDelegate);
        }

        PhotonMatchmakingClientDelegate CreatePhotonDelegate()
        {
            var factory = Container.Resolve<PhotonNetworkClientFactory>() as INetworkClientFactory;

            return new PhotonMatchmakingClientDelegate(
                factory.Create() as PhotonNetworkClient,
                Container.Resolve<IMatchmakingClient>());
        }

        HttpMatchmakingServer CreateServer()
        {
            return new HttpMatchmakingServer(
                Container.Resolve<IHttpClient>(),
                () => Settings.BaseUrl);
        }

        void SetupServer(HttpMatchmakingServer server)
        {
            var delegates = Container.ResolveList<IMatchmakingServerDelegate>();
            for(var i = 0; i < delegates.Count; i++)
            {
                server.AddDelegate(delegates[i]);
            }
        }

        StoredMatchmakingClient CreateClient()
        {
            return new StoredMatchmakingClient(
                new WampMatchmakingClient(
                    Container.Resolve<SocialPoint.Connection.ConnectionManager>(),
					Container.Resolve<ILoginData>()
                ), new AttrMatchStorage(
                Container.Resolve<IAttrStorage>("volatile")
            )
            );
        }

        void SetupClient(StoredMatchmakingClient client)
        {
            var delegates = Container.ResolveList<IMatchmakingClientDelegate>();
            for(var i = 0; i < delegates.Count; i++)
            {
                client.AddDelegate(delegates[i]);
            }
        }
    }
}
