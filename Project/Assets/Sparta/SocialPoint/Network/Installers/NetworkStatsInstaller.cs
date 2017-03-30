using System;
using SocialPoint.Dependency;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class NetworkStatsInstaller : ServiceInstaller
    {
        public bool Rebind = true;

        public override void InstallBindings()
        {
            if(Rebind)
            {
                Container.Bind<INetworkServer>().ToMethod<NetworkStatsServer>(CreateNetworkStatsServer); 
                Container.Bind<INetworkClient>().ToMethod<NetworkStatsClient>(CreateNetworkStatsClient); 
            }
            else
            {
                Container.Bind<INetworkServer>().ToLookup<INetworkServer>("internal");
                Container.Bind<INetworkClient>().ToLookup<INetworkClient>("internal");
            }   
        }

        NetworkStatsServer CreateNetworkStatsServer()
        {
            return new NetworkStatsServer(
                Container.Resolve<INetworkServer>("internal"),
                Container.Resolve<IUpdateScheduler>());
        }

        NetworkStatsClient CreateNetworkStatsClient()
        {
            return new NetworkStatsClient(Container.Resolve<INetworkClient>("internal"));
        }
    }
}

