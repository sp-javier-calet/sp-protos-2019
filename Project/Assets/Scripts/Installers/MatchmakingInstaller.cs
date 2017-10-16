using System;
using SocialPoint.Attributes;
using SocialPoint.Dependency;
using SocialPoint.Lockstep;
using SocialPoint.Login;
using SocialPoint.Matchmaking;
using SocialPoint.Network;

public class MatchmakingInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        const string DefaultBaseUrl = "http://int-lod.socialpointgames.es";
        const string DefaultWebsocketUrl = "ws://int-lod.socialpointgames.com:8001/find_opponent";
        public string BaseUrl = DefaultBaseUrl;
        public string[] WebsocketUrls = { DefaultWebsocketUrl };
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
            ), 
            new AttrMatchStorage(
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
