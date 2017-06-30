using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public sealed class ClientTurnData
    {
        List<ClientCommandData> _commands;

        public ClientTurnData(List<ClientCommandData> commands = null)
        {
            if(commands == null)
            {
                commands = new List<ClientCommandData>();
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

        public static readonly ClientTurnData Empty = new ClientTurnData();

        public static bool IsNullOrEmpty(ClientTurnData turn)
        {
            return turn == null || turn.CommandCount == 0;
        }

        public void AddCommand(ClientCommandData cmd)
        {
            _commands.Add(cmd);
        }

        public void Clear()
        {
            _commands.Clear();
        }

        public List<ClientCommandData>.Enumerator GetCommandEnumerator()
        {
            return _commands.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Format("[ClientTurnData:{0}]", _commands.Count);
        }

        public void Deserialize(LockstepCommandFactory factory, IReader reader)
        {
            int commandCount = (int)reader.ReadByte();
            _commands = new List<ClientCommandData>();
            for(int j = 0; j < commandCount; ++j)
            {
                var command = new ClientCommandData();
                command.Deserialize(factory, reader);
                _commands.Add(command);
            }
        }

        public void Serialize(LockstepCommandFactory factory, IWriter writer)
        {
            writer.Write((byte)_commands.Count);
            for(int i = 0; i < _commands.Count; ++i)
            {
                var command = _commands[i];
                command.Serialize(factory, writer);
            }
        }

        public ServerTurnData ToServer(LockstepCommandFactory factory)
        {
            var commands = new List<ServerCommandData>(_commands.Count);
            for(var i = 0; i < _commands.Count; i++)
            {
                var cmd = _commands[i];
                if(cmd != null)
                {
                    commands.Add(cmd.ToServer(factory));
                }
            }
            return new ServerTurnData(commands);
        }
    }
}