using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    [System.Serializable]
    public sealed class LockstepConfig : INetworkShareable
    {
        // The commands will be only processed every CommandStepFactor simulation steps reached
        public int CommandStepFactor = 10;

        // SimulationStep is the guaranteed simulation tick. Cannot be skipped.
        public int SimulationStep = 10;

        // Command processing tick.
        public int CommandStep
        {
            get
            {
                return SimulationStep * CommandStepFactor;
            }
        }

        // Minimum turn anticipation allowed (ExecutionTurnAnticipation will get reduced when good networking conditions
        // met)
        public int MinExecutionTurnAnticipation = 1;

        // Maximum turn anticipation allowed (ExecutionTurnAnticipation will grow when bad networking conditions met)
        public int MaxExecutionTurnAnticipation = 20;

        // Initial turn anticipation (commands will be scheduled to be executed by default to current
        // command + ExecutionTurnAnticipation)
        public int ExecutionTurnAnticipation = 2;

        // Maximum retries allowed per command
        public int MaxRetries = 2;

        public void Deserialize(IReader reader)
        {
            CommandStepFactor = reader.ReadInt32();
            SimulationStep = reader.ReadInt32();
            MinExecutionTurnAnticipation = reader.ReadInt32();
            MaxExecutionTurnAnticipation = reader.ReadInt32();
            ExecutionTurnAnticipation = reader.ReadInt32();
            MaxRetries = reader.ReadInt32();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(CommandStepFactor);
            writer.Write(SimulationStep);
            writer.Write(MinExecutionTurnAnticipation);
            writer.Write(MaxExecutionTurnAnticipation);
            writer.Write(ExecutionTurnAnticipation);
            writer.Write(MaxRetries);
        }
    }
}