using System;
using System.Text;
using UnityEngine.Networking;
using SocialPoint.Attributes;

namespace SocialPoint.Lockstep.Network
{
    public class SetLockstepConfigMessage : MessageBase
    {
        public LockstepConfig Config { get; private set; }

        public SetLockstepConfigMessage(LockstepConfig config = null)
        {
            Config = config;
        }

        public override void Deserialize(NetworkReader reader)
        {
            if(Config == null)
            {
                Config = new LockstepConfig();
            }
            base.Deserialize(reader);
            Config.CommandStepFactor = reader.ReadInt32();
            Config.SimulationStep = reader.ReadInt32();
            Config.MinExecutionTurnAnticipation = reader.ReadInt32();
            Config.MaxExecutionTurnAnticipation = reader.ReadInt32();
            Config.ExecutionTurnAnticipation = reader.ReadInt32();
            Config.MaxRetries = reader.ReadInt32();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Config.CommandStepFactor);
            writer.Write(Config.SimulationStep);
            writer.Write(Config.MinExecutionTurnAnticipation);
            writer.Write(Config.MaxExecutionTurnAnticipation);
            writer.Write(Config.ExecutionTurnAnticipation);
            writer.Write(Config.MaxRetries);
        }
    }
}