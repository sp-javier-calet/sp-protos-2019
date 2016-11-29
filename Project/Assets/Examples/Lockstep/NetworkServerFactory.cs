using System;
using System.Collections.Generic;
using SocialPoint.Lockstep;

namespace Examples.Lockstep
{
    public class NetworkServerFactory : INetworkServerGameFactory
    {
        public object Create(LockstepNetworkServer server, Dictionary<string, string> config)
        {
            return new ServerBehaviour(server);
        }
    }
}