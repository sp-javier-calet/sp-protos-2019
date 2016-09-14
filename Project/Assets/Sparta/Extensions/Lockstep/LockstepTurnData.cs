using System.Collections.Generic;

namespace SocialPoint.Lockstep
{
    public sealed class LockstepTurnData
    {
        public int Turn { get; private set; }

        public List<LockstepCommandData> Commands { get; set; }

        public LockstepTurnData(int turn)
        {
            Turn = turn;
            Commands = new List<LockstepCommandData>();
        }

        public LockstepTurnData(int turn, List<LockstepCommandData> commands)
        {
            Turn = turn;
            Commands = commands;
        }

        public override string ToString()
        {
            return string.Format("[LockstepTurnData: Turn={0}, CommandsCount={1}]", Turn, Commands.Count);
        }
    }
}