using System;
using System.IO;
using SocialPoint.Attributes;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    public interface ILockstepCommand : INetworkShareable, ICloneable
    {
    }

    [System.Serializable]
    public sealed class LockstepConfig : INetworkShareable
    {
        const string CommandStepDurationAttrKey = "command_step_duration";
        const string SimulationStepDurationAttrKey = "simulation_step_duration";
        const string MaxSkippedEmptyTurnsAttrKey = "max_skipped_empty_turns";

        public const int DefaultCommandStepDuration = 100;
        public const int DefaultSimulationStepDuration = 10;
        public const int DefaultMaxSkippedEmptyTurns = 0;

        // SimulationStep is the guaranteed simulation tick. Cannot be skipped.
        public int SimulationStepDuration = DefaultSimulationStepDuration;

        // Command processing tick.
        public int CommandStepDuration = DefaultCommandStepDuration;

        // Max time the server can be skipping turns
        public int MaxSkippedEmptyTurns = DefaultMaxSkippedEmptyTurns;

        public void Deserialize(IReader reader)
        {
            CommandStepDuration = reader.ReadInt32();
            SimulationStepDuration = reader.ReadInt32();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(CommandStepDuration);
            writer.Write(SimulationStepDuration);
        }

        public override string ToString()
        {
            return string.Format("[LockstepConfig\n" +
            "SimulationStepDuration:{0}\n" +
            "CommandStepDuration:{1}]", SimulationStepDuration, CommandStepDuration);
        }

        public Attr ToAttr()
        {
            var attrDic = new AttrDic();
            attrDic.Set(CommandStepDurationAttrKey, new AttrInt(CommandStepDuration));
            attrDic.Set(SimulationStepDurationAttrKey, new AttrInt(SimulationStepDuration));
            attrDic.Set(MaxSkippedEmptyTurnsAttrKey, new AttrInt(MaxSkippedEmptyTurns));
            return attrDic;
        }
    }

    public sealed class LockstepGameParams : INetworkShareable
    {
        public uint RandomSeed;

        public LockstepGameParams()
        {
            RandomSeed = XRandom.GenerateSeed();
        }

        public void Deserialize(IReader reader)
        {
            RandomSeed = reader.ReadUInt32();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(RandomSeed);
        }
    }
}