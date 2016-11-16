
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.Matchmaking;
using SocialPoint.WebSockets;
using SocialPoint.Login;
using SocialPoint.Utils;
using SocialPoint.Attributes;
using SocialPoint.Lockstep;
using System;

public class MatchmakingInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public string BaseUrl = "http://int-lod.socialpointgames.es";
        public string WebsocketUrl = "ws://int-lod.socialpointgames.com:8001/find_opponent";
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        Container.Rebind<IMatchmakingServer>().ToMethod<HttpMatchmakingServer>
            (CreateServer, SetupServer);
        Container.Rebind<IMatchmakingClient>().ToMethod<StoredMatchmakingClient>
            (CreateClient, SetupClient);

        Container.Bind<IMatchmakingClientDelegate>().ToMethod<LockstepMatchmakingClientDelegate>(CreateLockstepDelegate);
        Container.Bind<IMatchmakingClientDelegate>().ToMethod<PhotonMatchmakingClientDelegate>(CreatePhotonDelegate);
    }

    LockstepMatchmakingClientDelegate CreateLockstepDelegate()
    {
        return new LockstepMatchmakingClientDelegate(
            Container.Resolve<LockstepNetworkClient>(),
            Container.Resolve<IMatchmakingClient>());
    }

    PhotonMatchmakingClientDelegate CreatePhotonDelegate()
    {
        return new PhotonMatchmakingClientDelegate(
            Container.Resolve<PhotonNetworkClient>(),
            Container.Resolve<IMatchmakingClient>());
    }

    HttpMatchmakingServer CreateServer()
    {
        return new HttpMatchmakingServer(
            Container.Resolve<IHttpClient>(),
            Settings.BaseUrl);
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
            new WebsocketMatchmakingClient(
                Container.Resolve<ILoginData>(),
                new WebSocketSharpClient(
                    Settings.WebsocketUrl,
                    Container.Resolve<IUpdateScheduler>()
                )
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
