using System;
using System.Collections;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public interface INetworkActionDelegate
    {
        void ApplyAction(object action, NetworkScene scene);
    }
}
