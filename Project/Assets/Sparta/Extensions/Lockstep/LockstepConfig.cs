using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    [System.Serializable]
    public sealed class LockstepConfig : INetworkShareable
    {
        public const int DefaultCommandStepFactor = 10;
        public const int DefaultSimulationStep = 10;
        public const int DefaultMinExecutionTurnAnticipation = 1;
        public const int DefaultMaxExecutionTurnAnticipation = 20;
        public const int DefaultInitialExecutionTurnAnticipation = 2;

        // The commands will be only processed every CommandStepFactor simulation steps reached
        public int CommandStepFactor = DefaultCommandStepFactor;

        // SimulationStep is the guaranteed simulation tick. Cannot be skipped.
        public int SimulationStep = DefaultSimulationStep;

        // Minimum turn anticipation allowed (ExecutionTurnAnticipation will get reduced when good networking conditions
        // met)
        public int MinExecutionTurnAnticipation = DefaultMinExecutionTurnAnticipation;

        // Maximum turn anticipation allowed (ExecutionTurnAnticipation will grow when bad networking conditions met)
        public int MaxExecutionTurnAnticipation = DefaultMaxExecutionTurnAnticipation;

        // Initial turn anticipation (commands will be scheduled to be executed by default to current
        // command + ExecutionTurnAnticipation)
        public int InitialExecutionTurnAnticipation = DefaultInitialExecutionTurnAnticipation;

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
            MinExecutionTurnAnticipation = reader.ReadInt32();
            MaxExecutionTurnAnticipation = reader.ReadInt32();
            InitialExecutionTurnAnticipation = reader.ReadInt32();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(CommandStepFactor);
            writer.Write(SimulationStep);
            writer.Write(MinExecutionTurnAnticipation);
            writer.Write(MaxExecutionTurnAnticipation);
            writer.Write(InitialExecutionTurnAnticipation);
        }
    }
}