using System.Collections.Generic;

namespace SocialPoint.Lockstep
{
    public class LockstepTurnData
    {
        public int Turn { get; private set; }

        public List<ILockstepCommand> Commands { get; set; }

        public LockstepTurnData(int turn)
        {
            Turn = turn;
            Commands = new List<ILockstepCommand>();
        }

        public LockstepTurnData(int turn, List<ILockstepCommand> commands)
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