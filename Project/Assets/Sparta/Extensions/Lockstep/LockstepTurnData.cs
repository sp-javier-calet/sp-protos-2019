using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public sealed class ClientLockstepTurnData
    {
        public int Turn { get; private set; }

        public List<ClientLockstepCommandData> Commands { get; set; }

        public ClientLockstepTurnData(int turn=0)
        {
            Turn = turn;
            Commands = new List<ClientLockstepCommandData>();
        }

        public ClientLockstepTurnData(int turn, List<ClientLockstepCommandData> commands)
        {
            Turn = turn;
            Commands = commands;
        }

        public override string ToString()
        {
            return string.Format("[ClientLockstepTurnData: Turn={0}, CommandsCount={1}]", Turn, Commands.Count);
        }

        public void Deserialize(LockstepCommandFactory factory, IReader reader)
        {
            Turn = reader.ReadInt32();
            int commandCount = (int)reader.ReadByte();
            Commands = new List<ClientLockstepCommandData>();
            for(int j = 0; j < commandCount; ++j)
            {
                var command = new ClientLockstepCommandData();
                command.Deserialize(factory, reader);
                Commands.Add(command);
            }
        }

        public void Serialize(LockstepCommandFactory factory, IWriter writer)
        {
            writer.Write(Turn);
            writer.Write((byte)Commands.Count);
            for(int i = 0; i < Commands.Count; ++i)
            {
                var command = Commands[i];
                command.Serialize(factory, writer);
            }
        }

        public ServerLockstepTurnData ToServer(LockstepCommandFactory factory)
        {
            var commands = new List<ServerLockstepCommandData>(Commands.Count);
            for(var i = 0; i < Commands.Count; i++)
            {
                var cmd = Commands[i];
                if(cmd != null)
                {
                    commands.Add(cmd.ToServer(factory));
                }
            }
            return new ServerLockstepTurnData(Turn, commands);
        }
    }

    public sealed class ServerLockstepTurnData : INetworkShareable
    {
        public int Turn { get; set; }

        public List<ServerLockstepCommandData> Commands { get; set; }

        public ServerLockstepTurnData(int turn)
        {
            Turn = turn;
            Commands = new List<ServerLockstepCommandData>();
        }

        public ServerLockstepTurnData(int turn, List<ServerLockstepCommandData> commands)
        {
            Turn = turn;
            Commands = commands;
        }

        public override string ToString()
        {
            return string.Format("[ServerLockstepTurnData: Turn={0}, CommandsCount={1}]", Turn, Commands.Count);
        }

        public void Deserialize(IReader reader)
        {
            Turn = reader.ReadInt32();
            int commandCount = (int)reader.ReadByte();
            Commands = new List<ServerLockstepCommandData>();
            for(int j = 0; j < commandCount; ++j)
            {
                var command = new ServerLockstepCommandData();
                command.Deserialize(reader);
                Commands.Add(command);
            }

            throw new System.NotImplementedException();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write((byte)Commands.Count);
            for(int i = 0; i < Commands.Count; ++i)
            {
                var command = Commands[i];
                command.Serialize(writer);
            }
        }

        public ClientLockstepTurnData ToClient(LockstepCommandFactory factory)
        {
            var commands = new List<ClientLockstepCommandData>(Commands.Count);
            for(var i = 0; i < Commands.Count; i++)
            {
                var cmd = Commands[i];
                if(cmd != null)
                {
                    commands.Add(cmd.ToClient(factory));
                }
            }
            return new ClientLockstepTurnData(Turn, commands);
        }
    }
}