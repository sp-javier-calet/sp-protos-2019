using SocialPoint.Lockstep.Network;
using UnityEngine.Networking;

namespace SocialPoint.Lockstep
{
    public static class LockstepReplayLoader
    {
        public static void LoadReplay(NetworkReader networkReader, 
                                      ClientLockstepController clientLockstep,
                                      NetworkLockstepCommandDataFactory commandDataFactory)
        {
            SetLockstepConfigMessage configMessage = new SetLockstepConfigMessage();
            configMessage.Deserialize(networkReader);
            clientLockstep.Init(configMessage.Config);
            int count = networkReader.ReadInt32();
            clientLockstep.NeedsTurnConfirmation = false;
            for(int i = 0; i < count; ++i)
            {
                int turn = networkReader.ReadInt32();
                var command = commandDataFactory.CreateNetworkLockstepCommandData(turn, networkReader).LockstepCommand;
                clientLockstep.AddConfirmedCommand(command);
            }
        }
    }
}