using SocialPoint.Lockstep.Network;
using SocialPoint.Utils;
using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public static class LockstepReplayLoader
    {
        public static void LoadReplay(IReader reader, 
                                      ClientLockstepController clientLockstep,
                                      LockstepCommandFactory commandFactory)
        {
            var configMessage = new ClientSetupMessage();
            configMessage.Deserialize(reader);
            clientLockstep.Init(configMessage.Config);
            int count = reader.ReadInt32();
            for(int i = 0; i < count; ++i)
            {
                var command = new LockstepCommandData();
                command.Deserialize(commandFactory, reader);
                clientLockstep.AddConfirmedCommand(command);
            }
        }
    }
}