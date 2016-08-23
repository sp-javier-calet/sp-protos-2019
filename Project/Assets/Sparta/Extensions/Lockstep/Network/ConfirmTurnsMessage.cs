using System;
using System.IO;
using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep.Network
{
    public sealed class ConfirmTurnsMessage : INetworkMessage
    {
        LockstepCommandDataFactory _commandDataFactory;

        public LockstepTurnData[] ConfirmedTurns { get; set; }

        public ConfirmTurnsMessage(LockstepCommandDataFactory commandDataFactory)
        {
            _commandDataFactory = commandDataFactory;
        }

        public void Deserialize(IReaderWrapper reader)
        {
            int turnCount = (int)reader.ReadByte();
            ConfirmedTurns = new LockstepTurnData[turnCount];
            for(int i = 0; i < turnCount; ++i)
            {
                int turn = reader.ReadInt32();
                int commandCount = (int)reader.ReadByte();
                var commands = new List<ILockstepCommand>();
                for(int j = 0; j < commandCount; ++j)
                {
                    LockstepCommandData data = _commandDataFactory.CreateNetworkLockstepCommandData(turn, reader);
                    commands.Add(data.LockstepCommand);
                }
                ConfirmedTurns[i] = new LockstepTurnData(turn, commands);
            }
        }

        public void Serialize(IWriterWrapper writer)
        {
            writer.Write((byte)ConfirmedTurns.Length);
            for(int i = 0; i < ConfirmedTurns.Length; ++i)
            {
                var confirmedTurn = ConfirmedTurns[i];
                writer.Write(confirmedTurn.Turn);
                writer.Write((byte)confirmedTurn.Commands.Count);
                for(int j = 0; j < confirmedTurn.Commands.Count; ++j)
                {
                    var command = confirmedTurn.Commands[j];
                    LockstepCommandData data = _commandDataFactory.CreateNetworkLockstepCommandData(command);
                    data.Serialize(writer);
                }
            }
        }

        public bool RequiresSync
        {
            get
            {
                return false;
            }
        }
    }
}