using System;

namespace SocialPoint.Lockstep
{
    public class SetTurnAnticipationLockstepCommand: ILockstepCommand
    {
        ClientLockstepController _lockstepController;

        public int Anticipation { get; private set; }

        public int Turn { get; private set; }

        public int Retries { get; set; }

        public byte LockstepCommandDataType
        {
            get
            {
                return Network.NetworkLockstepCommandDataTypes.SetTurnAnticipation;
            }
        }

        public SetTurnAnticipationLockstepCommand(ClientLockstepController lockstepController, int anticipation, int turn)
        {
            Turn = turn;
            Anticipation = anticipation;
            _lockstepController = lockstepController;
        }

        public event Action<ILockstepCommand,bool> Applied;
    
        public event Action<ILockstepCommand> Discarded;

        public bool Retry(int turn)
        {
            Turn = turn;
            Retries++;
            return true;
        }

        public bool Apply()
        {
            _lockstepController.ExecutionTurnAnticipation = Anticipation;
            if(Applied != null)
            {
                Applied(this, true);
            }
            return true;
        }

        public void Discard()
        {
            if(Discarded != null)
            {
                Discarded(this);
            }
        }

        public bool Equals(ILockstepCommand obj)
        {
            var other = obj as SetTurnAnticipationLockstepCommand;
            return other != null && other.Turn == Turn && other.Anticipation == Anticipation;
        }
    }
}