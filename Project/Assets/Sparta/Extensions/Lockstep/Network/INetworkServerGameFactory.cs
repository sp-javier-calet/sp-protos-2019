using System.Collections.Generic;

namespace SocialPoint.Lockstep
{
    public interface INetworkServerGameFactory
    {
        object Create(LockstepNetworkServer server, Dictionary<string, string> config);
    }
}