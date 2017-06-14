using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public sealed class ClientSetupMessage : INetworkShareable
    {
        public LockstepConfig Config { get; private set; }

        public LockstepGameParams GameParams { get; private set; }

        public ClientSetupMessage(LockstepConfig config = null, LockstepGameParams gameParams = null)
        {
            if(config == null)
            {
                config = new LockstepConfig();
            }
            Config = config;

            if(gameParams == null)
            {
                gameParams = new LockstepGameParams();
            }
            GameParams = gameParams;
        }

        public void Deserialize(IReader reader)
        {
            Config.Deserialize(reader);
            GameParams.Deserialize(reader);
        }

        public void Serialize(IWriter writer)
        {
            Config.Serialize(writer);
            GameParams.Serialize(writer);
        }
    }
}