using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public sealed class ClientSetupMessage : INetworkShareable
    {
        public LockstepConfig Config { get; private set; }

        public ClientSetupMessage(LockstepConfig config = null)
        {
            if(config == null)
            {
                config = new LockstepConfig();
            }
            Config = config;
        }

        public void Deserialize(IReader reader)
        {

            Config.Deserialize(reader);
        }

        public void Serialize(IWriter writer)
        {
            Config.Serialize(writer);
        }
    }
}