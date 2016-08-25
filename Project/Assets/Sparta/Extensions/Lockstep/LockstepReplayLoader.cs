using SocialPoint.Lockstep.Network;
using UnityEngine.Networking;
using System.IO;
using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public static class LockstepReplayLoader
    {
        public static void LoadReplay(IReader reader, 
                                      ClientLockstepController clientLockstep,
                                      LockstepCommandDataFactory commandDataFactory)
        {
            SetLockstepConfigMessage configMessage = new SetLockstepConfigMessage();
            configMessage.Deserialize(reader);
            clientLockstep.Init(configMessage.Config);
            int count = reader.ReadInt32();
            clientLockstep.NeedsTurnConfirmation = false;
            for(int i = 0; i < count; ++i)
            {
                int turn = reader.ReadInt32();
                var command = commandDataFactory.CreateNetworkLockstepCommandData(turn, reader).LockstepCommand;
                clientLockstep.AddConfirmedCommand(command);
            }
        }
    }
}