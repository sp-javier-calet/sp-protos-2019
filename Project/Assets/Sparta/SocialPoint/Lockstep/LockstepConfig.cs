namespace SocialPoint.Lockstep
{
    public class LockstepConfig
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
    }
}