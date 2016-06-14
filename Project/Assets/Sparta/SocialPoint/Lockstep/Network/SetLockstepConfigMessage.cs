﻿using System;
using System.Text;
using SocialPoint.Attributes;
using System.IO;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep.Network
{
    public class SetLockstepConfigMessage : INetworkMessage
    {
        public LockstepConfig Config { get; private set; }

        public SetLockstepConfigMessage(LockstepConfig config = null)
        {
            Config = config;
        }

        public void Deserialize(IReaderWrapper reader)
        {
            if(Config == null)
            {
                Config = new LockstepConfig();
            }

            Config.CommandStepFactor = reader.ReadInt32();
            Config.SimulationStep = reader.ReadInt32();
            Config.MinExecutionTurnAnticipation = reader.ReadInt32();
            Config.MaxExecutionTurnAnticipation = reader.ReadInt32();
            Config.ExecutionTurnAnticipation = reader.ReadInt32();
            Config.MaxRetries = reader.ReadInt32();
        }

        public void Serialize(IWriterWrapper writer)
        {
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