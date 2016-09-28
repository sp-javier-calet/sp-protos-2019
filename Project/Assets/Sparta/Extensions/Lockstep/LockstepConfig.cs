using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    [System.Serializable]
    public sealed class LockstepConfig : INetworkShareable
    {
        public const int DefaultCommandStepDuration = 100;
        public const int DefaultSimulationStepDuration = 10;
        public const int DefaultMaxSimulationStepsPerFrame = 10;
        public const int DefaultSimulationDelay = 1000;

        // SimulationStep is the guaranteed simulation tick. Cannot be skipped.
        public int SimulationStepDuration = DefaultSimulationStepDuration;

        public int MaxSimulationStepsPerFrame = DefaultMaxSimulationStepsPerFrame;

        // Command processing tick.
        public int CommandStepDuration = DefaultCommandStepDuration;

        public int SimulationDelay = DefaultSimulationDelay;

        public void Deserialize(IReader reader)
        {
            CommandStepDuration = reader.ReadInt32();
            SimulationStepDuration = reader.ReadInt32();
            MaxSimulationStepsPerFrame = reader.ReadInt32();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(CommandStepDuration);
            writer.Write(SimulationStepDuration);
            writer.Write(MaxSimulationStepsPerFrame);
        }
    }
}