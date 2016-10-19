using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public sealed class ClientSetupMessage : INetworkShareable
    {
        public LockstepConfig Config { get; private set; }

        public uint RandomSeed { get; private set; }

        public ClientSetupMessage(uint randomSeed = 0, LockstepConfig config = null)
        {
            if(config == null)
            {
                config = new LockstepConfig();
            }
            Config = config;
            RandomSeed = randomSeed;
        }

        public void Deserialize(IReader reader)
        {
            Config.Deserialize(reader);
            RandomSeed = reader.ReadUInt32();
        }

        public void Serialize(IWriter writer)
        {
            Config.Serialize(writer);
            writer.Write(RandomSeed);
        }
    }
}