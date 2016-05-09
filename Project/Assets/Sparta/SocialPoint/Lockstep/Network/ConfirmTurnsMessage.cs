using System;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SocialPoint.Lockstep.Network
{
    public class ConfirmTurnsMessage : MessageBase
    {
        NetworkLockstepCommandDataFactory _commandDataFactory;

        public LockstepTurnData[] ConfirmedTurns { get; set; }

        public ConfirmTurnsMessage(NetworkLockstepCommandDataFactory commandDataFactory)
        {
            _commandDataFactory = commandDataFactory;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);

            int turnCount = (int)reader.ReadByte();
            ConfirmedTurns = new LockstepTurnData[turnCount];
            for(int i = 0; i < turnCount; ++i)
            {
                int turn = reader.ReadInt32();
                int commandCount = (int)reader.ReadByte();
                var commands = new List<ILockstepCommand>();
                for(int j = 0; j < commandCount; ++j)
                {
                    NetworkLockstepCommandData data = _commandDataFactory.CreateNetworkLockstepCommandData(turn, reader);
                    commands.Add(data.LockstepCommand);
                }
                ConfirmedTurns[i] = new LockstepTurnData(turn, commands);
            }
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.Write((byte)ConfirmedTurns.Length);
            for(int i = 0; i < ConfirmedTurns.Length; ++i)
            {
                var confirmedTurn = ConfirmedTurns[i];
                writer.Write(confirmedTurn.Turn);
                writer.Write((byte)confirmedTurn.Commands.Count);
                for(int j = 0; j < confirmedTurn.Commands.Count; ++j)
                {
                    var command = confirmedTurn.Commands[j];
                    NetworkLockstepCommandData data = _commandDataFactory.CreateNetworkLockstepCommandData(command);
                    data.Serialize(writer);
                }
            }
        }
    }
}