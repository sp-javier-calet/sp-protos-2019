using System.Collections.Generic;

namespace SocialPoint.Lockstep
{
    public interface INetworkClientGameFactory
    {
        object Create(LockstepNetworkClient client, Dictionary<string, string> config);
    }
}