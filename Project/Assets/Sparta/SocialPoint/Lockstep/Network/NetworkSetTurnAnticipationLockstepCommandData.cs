using System;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SocialPoint.Lockstep.Network
{
    public class NetworkSetTurnAnticipationLockstepCommandData : NetworkLockstepCommandData
    {
        ClientLockstepController _lockstepController;

        public NetworkSetTurnAnticipationLockstepCommandData(int turn, ClientLockstepController lockstepController)
            : base(turn, NetworkLockstepCommandDataTypes.SetTurnAnticipation)
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

        public override void DeserializeCustomData(NetworkReader reader)
        {
        
            byte anticipation = reader.ReadByte();

            if(_lockstepCommand == null)
            {
                _lockstepCommand = new SetTurnAnticipationLockstepCommand(_lockstepController, anticipation, Turn);
            }
        }

        public override void SerializeCustomData(NetworkWriter writer)
        {
            writer.Write((byte)_lockstepCommand.Anticipation);
        }
    }
}