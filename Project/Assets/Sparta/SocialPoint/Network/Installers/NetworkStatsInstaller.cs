using System;
using SocialPoint.Dependency;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class NetworkStatsInstaller : ServiceInstaller
    {
        public bool Rebind = true;
        public int PingInterval = NetworkStatsServer.DefaultSendStatusMessageInterval;

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
            var nsc = new NetworkStatsClient(Container.Resolve<INetworkClient>("internal"), Container.Resolve<IUpdateScheduler>());
            nsc.PingInterval = PingInterval;
            return nsc;
        }
    }
}

