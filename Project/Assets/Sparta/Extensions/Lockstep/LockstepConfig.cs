using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    [System.Serializable]
    public sealed class LockstepConfig : INetworkShareable
    {
        public const int DefaultCommandStepFactor = 10;
        public const int DefaultSimulationStep = 10;

        // The commands will be only processed every CommandStepFactor simulation steps reached
        public int CommandStepFactor = DefaultCommandStepFactor;

        // SimulationStep is the guaranteed simulation tick. Cannot be skipped.
        public int SimulationStep = DefaultSimulationStep;

        // Command processing tick.
        public int CommandStep
        {
            get
            {
                return SimulationStep * CommandStepFactor;
            }
        }

        public void Deserialize(IReader reader)
        {
            CommandStepFactor = reader.ReadInt32();
            SimulationStep = reader.ReadInt32();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(CommandStepFactor);
            writer.Write(SimulationStep);
        }
    }
}