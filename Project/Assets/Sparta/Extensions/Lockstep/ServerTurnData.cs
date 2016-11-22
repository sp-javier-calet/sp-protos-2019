using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public sealed class ServerTurnData : INetworkShareable
    {
        List<ServerCommandData> _commands;

        public ServerTurnData(List<ServerCommandData> commands = null)
        {
            if(commands == null)
            {
                commands = new List<ServerCommandData>();
            }
            _commands = commands;
        }

        public int CommandCount
        {
            get
            {
                return _commands.Count;
            }
        }

        public static bool IsNullOrEmpty(ServerTurnData turn)
        {
            return turn == null || turn.CommandCount == 0;
        }

        public void AddCommand(ServerCommandData cmd)
        {
            if(cmd != null)
            {
                _commands.Add(cmd);
            }
        }

        public static readonly ServerTurnData Empty = new ServerTurnData();

        public void Clear()
        {
            _commands.Clear();
        }

        public override string ToString()
        {
            return string.Format("[ServerTurnData:{0}]", _commands.Count);
        }

        public void Deserialize(IReader reader)
        {
            int commandCount = (int)reader.ReadByte();
            _commands = new List<ServerCommandData>();
            for(int j = 0; j < commandCount; ++j)
            {
                var command = new ServerCommandData();
                command.Deserialize(reader);
                _commands.Add(command);
            }

            throw new System.NotImplementedException();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write((byte)_commands.Count);
            for(int i = 0; i < _commands.Count; ++i)
            {
                var command = _commands[i];
                command.Serialize(writer);
            }
        }

        public ClientTurnData ToClient(LockstepCommandFactory factory)
        {
            var commands = new List<ClientCommandData>(_commands.Count);
            for(var i = 0; i < _commands.Count; i++)
            {
                var cmd = _commands[i];
                if(cmd != null)
                {
                    commands.Add(cmd.ToClient(factory));
                }
            }
            return new ClientTurnData(commands);
        }
    }
}