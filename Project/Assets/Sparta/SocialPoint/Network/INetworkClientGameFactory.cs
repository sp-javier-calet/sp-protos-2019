using SocialPoint.Utils;
using System.Collections.Generic;

namespace SocialPoint.Network
{
    public interface INetworkClientGameFactory
    {
        object Create(INetworkClient client, IUpdateScheduler scheduler, Dictionary<string, string> config);
    }
}