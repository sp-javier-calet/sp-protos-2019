using SocialPoint.Network;
using SocialPoint.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Photon.Stardust.S2S.Server
{
    public interface IGameClient : IDeltaUpdateable
    {
        void SetUp(INetworkClient client);
        void Start();
    }
}
