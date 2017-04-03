using System;
using SocialPoint.Dependency;
using SocialPoint.Network;

namespace SocialPoint.Network
{
    public class LocalNetworkInstaller : SubInstaller
    {
        public override void InstallBindings()
        {
            Container.Rebind<LocalNetworkServer>().ToMethod<LocalNetworkServer>(CreateLocalServer, SetupServer);
            Container.Rebind<INetworkServer>("internal").ToLookup<LocalNetworkServer>();
            Container.Rebind<LocalNetworkClient>().ToMethod<LocalNetworkClient>(CreateLocalClient, SetupClient);
            Container.Rebind<INetworkClient>("internal").ToLookup<LocalNetworkClient>();
        }

        LocalNetworkClient CreateLocalClient()
        {
            return new LocalNetworkClient(
                Container.Resolve<LocalNetworkServer>());
        }

        LocalNetworkServer CreateLocalServer()
        {
            return new LocalNetworkServer();
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