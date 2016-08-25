using System;
using System.Text;
using SocialPoint.Attributes;
using System.IO;
using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public class SetLockstepConfigMessage : INetworkMessage
    {
        public byte PlayerId { get; private set; }

        public LockstepConfig Config { get; private set; }

        public SetLockstepConfigMessage(byte playerId = 0, LockstepConfig config = null)
        {
            PlayerId = playerId;
            Config = config;
        }

        public void Deserialize(IReader reader)
        {
            if(Config == null)
            {
                Config = new LockstepConfig();
            }

            PlayerId = reader.ReadByte();
            Config.CommandStepFactor = reader.ReadInt32();
            Config.SimulationStep = reader.ReadInt32();
            Config.MinExecutionTurnAnticipation = reader.ReadInt32();
            Config.MaxExecutionTurnAnticipation = reader.ReadInt32();
            Config.ExecutionTurnAnticipation = reader.ReadInt32();
            Config.MaxRetries = reader.ReadInt32();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(Config.CommandStepFactor);
            writer.Write(Config.SimulationStep);
            writer.Write(Config.MinExecutionTurnAnticipation);
            writer.Write(Config.MaxExecutionTurnAnticipation);
            writer.Write(Config.ExecutionTurnAnticipation);
            writer.Write(Config.MaxRetries);
        }

        public bool RequiresSync
        {
            get
            {
                return false;
            }
        }
    }
}