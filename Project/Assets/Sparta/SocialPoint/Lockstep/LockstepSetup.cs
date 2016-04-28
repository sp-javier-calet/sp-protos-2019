namespace SocialPoint.Lockstep
{
    public class LockstepConfig
    {
        public int CommandStep = 100;
        public int SimulationStep = 10;
        public int MinExecutionTurnAnticipation = 1;
        public int MaxExecutionTurnAnticipation = 20;
        public int ExecutionTurnAnticipation = 2;
        public int MaxRetries = 2;
    }
}