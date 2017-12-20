using System;
using SocialPoint.Dependency;

namespace SocialPoint.Network
{
    public class PhotonNetworkInstaller : SubInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public PhotonNetworkConfig Config;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.RebindUnityComponent<PhotonNetworkServer>().WithSetup<PhotonNetworkServer>(SetupPhotonServer);
            Container.Rebind<INetworkServer>("internal").ToLookup<PhotonNetworkServer>();
            Container.Rebind<INetworkServer>().ToLookup<PhotonNetworkServer>();
            Container.RebindUnityComponent<PhotonNetworkClient>().WithSetup<PhotonNetworkClient>(SetupPhotonClient);
            Container.Rebind<INetworkClient>("internal").ToLookup<PhotonNetworkClient>();
            Container.Rebind<INetworkClient>().ToLookup<PhotonNetworkClient>();
        }

        void SetupPhotonServer(PhotonNetworkServer server)
        {
            server.Config = new PhotonNetworkConfig(Settings.Config);
            SetupServer(server);
        }

        void SetupPhotonClient(PhotonNetworkClient client)
        {
            client.Config = new PhotonNetworkConfig(Settings.Config);
            SetupClient(client);
        }

        void SetupServer(INetworkServer server)
        {
            var dlgs = Container.ResolveList<INetworkServerDelegate>();
            for(var i = 0; i < dlgs.Count; i++)
            {
                server.AddDelegate(dlgs[i]);
            }
        }

        void SetupClient(INetworkClient client)
        {
            var dlgs = Container.ResolveList<INetworkClientDelegate>();
            for(var i = 0; i < dlgs.Count; i++)
            {
                client.AddDelegate(dlgs[i]);
            }
        }
    }
}
