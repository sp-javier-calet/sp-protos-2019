using System;
using SocialPoint.Utils;
using System.Collections.Generic;
using System.IO;

namespace SocialPoint.Lockstep
{
    public class SetTurnAnticipationLockstepCommandData : LockstepCommandData
    {
        ClientLockstepController _lockstepController;

        public SetTurnAnticipationLockstepCommandData(int turn, ClientLockstepController lockstepController)
            : base(turn, LockstepCommandDataTypes.SetTurnAnticipation)
        {
            _lockstepController = lockstepController;
        }

        SetTurnAnticipationLockstepCommand _lockstepCommand;

        public override ILockstepCommand LockstepCommand
        {
            get
            {
                return _lockstepCommand;
            }
            set
            {
                _lockstepCommand = (SetTurnAnticipationLockstepCommand)value;
            }
        }

        public override void DeserializeCustomData(IReaderWrapper reader)
        {
        
            byte anticipation = reader.ReadByte();

            if(_lockstepCommand == null)
            {
                _lockstepCommand = new SetTurnAnticipationLockstepCommand(_lockstepController, anticipation, Turn);
            }
        }

        public override void SerializeCustomData(IWriterWrapper writer)
        {
            writer.Write((byte)_lockstepCommand.Anticipation);
        }
    }
}