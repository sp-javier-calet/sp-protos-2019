using System;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public sealed class ClientConfirmTurnsMessage : INetworkShareable
    {
        LockstepCommandFactory _factory;

        public ClientLockstepTurnData[] ConfirmedTurns { get; set; }

        public ClientConfirmTurnsMessage(LockstepCommandFactory factory)
        {
            _factory = factory;
        }

        public void Deserialize(IReader reader)
        {
            int turnCount = (int)reader.ReadByte();
            ConfirmedTurns = new ClientLockstepTurnData[turnCount];
            for(int i = 0; i < turnCount; ++i)
            {
                int turn = reader.ReadInt32();
                int commandCount = (int)reader.ReadByte();
                var commands = new List<ClientLockstepCommandData>();
                for(int j = 0; j < commandCount; ++j)
                {
                    var command = new ClientLockstepCommandData();
                    command.Deserialize(_factory, reader);
                    commands.Add(command);
                }
                ConfirmedTurns[i] = new ClientLockstepTurnData(turn, commands);
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

    public sealed class ServerConfirmTurnsMessage : INetworkShareable
    {
        public ServerLockstepTurnData[] ConfirmedTurns { get; set; }

        public ServerConfirmTurnsMessage()
        {
        }

        public void Deserialize(IReader reader)
        {
            int turnCount = (int)reader.ReadByte();
            ConfirmedTurns = new ServerLockstepTurnData[turnCount];
            for(int i = 0; i < turnCount; ++i)
            {
                int turn = reader.ReadInt32();
                int commandCount = (int)reader.ReadByte();
                var commands = new List<ServerLockstepCommandData>();
                for(int j = 0; j < commandCount; ++j)
                {
                    var command = new ServerLockstepCommandData();
                    command.Deserialize(reader);
                    commands.Add(command);
                }
                ConfirmedTurns[i] = new ServerLockstepTurnData(turn, commands);
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
                    command.Serialize(writer);
                }
            }
        }
    }
}