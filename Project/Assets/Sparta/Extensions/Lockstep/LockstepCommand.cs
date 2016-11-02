using System;
using System.IO;
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
        public const int DefaultCommandStepDuration = 100;
        public const int DefaultSimulationStepDuration = 10;

        // SimulationStep is the guaranteed simulation tick. Cannot be skipped.
        public int SimulationStepDuration = DefaultSimulationStepDuration;

        // Command processing tick.
        public int CommandStepDuration = DefaultCommandStepDuration;

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