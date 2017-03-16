using Photon.Stardust.S2S.Server;
using Photon.Stardust.S2S.Server.ClientConnections;
using System.Collections.Generic;
using SocialPoint.Network;

namespace SocialPoint.Lockstep
{
    public class LockstepApplication : StardustApplication
    {
        public override object CreateGameClient(object factory, StardustClientConnection conn, Dictionary<string, string> config)
        {
            var lockFactory = (INetworkClientGameFactory)factory;
            var lockClient = new LockstepClient();
            var cmdFactory = new LockstepCommandFactory();
            var netLockClient = new LockstepNetworkClient(conn.NetworkClient, lockClient, cmdFactory);
            conn.Scheduler.Add(lockClient);
            return lockFactory.Create(netLockClient, config);
        }
    }
}
