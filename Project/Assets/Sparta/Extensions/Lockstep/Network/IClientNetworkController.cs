using System;

namespace SocialPoint.Lockstep.Network
{
    interface IClientNetworkController : IDisposable, INetworkMessageController
    {
        event Action<string> Log;
        event Action Connected;
        event Action Disconnected;
        event Action<int> OtherConnected;
        event Action<int> OtherDisconnected;
        event Action<int, string> Error;

        event Action<int, LockstepConfig> LockstepConfigReceived;

        void InitLockstep(ClientLockstepController clientLockstep,
                          LockstepCommandDataFactory commandDataFactory);

        void Start();

        void Stop();

        void SendClientReady();
    }
}