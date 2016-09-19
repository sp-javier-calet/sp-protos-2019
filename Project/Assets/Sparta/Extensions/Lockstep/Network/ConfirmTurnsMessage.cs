using System;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public sealed class ConfirmTurnsMessage : INetworkShareable
    {
        LockstepCommandFactory _factory;

        public LockstepTurnData[] ConfirmedTurns { get; set; }

        public ConfirmTurnsMessage(LockstepCommandFactory factory)
        {
            _factory = factory;
        }

        public void Deserialize(IReader reader)
        {
            int turnCount = (int)reader.ReadByte();
            ConfirmedTurns = new LockstepTurnData[turnCount];
            for(int i = 0; i < turnCount; ++i)
            {
                int turn = reader.ReadInt32();
                int commandCount = (int)reader.ReadByte();
                var commands = new List<LockstepCommandData>();
                for(int j = 0; j < commandCount; ++j)
                {
                    var command = new LockstepCommandData();
                    command.Deserialize(_factory, reader);
                    commands.Add(command);
                }
                ConfirmedTurns[i] = new LockstepTurnData(turn, commands);
            }
        }

        public void Serialize(IWriter writer)
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
                    command.Serialize(_factory, writer);
                }
            }
        }
    }
}