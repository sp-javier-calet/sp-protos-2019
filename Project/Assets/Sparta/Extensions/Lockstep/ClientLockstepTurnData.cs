using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public sealed class ClientLockstepTurnData
    {
        List<ClientLockstepCommandData> _commands;

        public ClientLockstepTurnData(List<ClientLockstepCommandData> commands=null)
        {
            if(commands == null)
            {
                commands = new List<ClientLockstepCommandData>();
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

        public ClientLockstepCommandData GetCommand(int i)
        {
            return _commands[i];
        }

        public void AddCommand(ClientLockstepCommandData cmd)
        {
            _commands.Add(cmd);
        }

        public void Clear()
        {
            _commands.Clear();
        }

        public List<ClientLockstepCommandData>.Enumerator GetCommandEnumerator()
        {
            return _commands.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Format("[ClientLockstepTurnData:{0}]", _commands.Count);
        }

        public void Deserialize(LockstepCommandFactory factory, IReader reader)
        {
            int commandCount = (int)reader.ReadByte();
            _commands = new List<ClientLockstepCommandData>();
            for(int j = 0; j < commandCount; ++j)
            {
                var command = new ClientLockstepCommandData();
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

        public ServerLockstepTurnData ToServer(LockstepCommandFactory factory)
        {
            var commands = new List<ServerLockstepCommandData>(_commands.Count);
            for(var i = 0; i < _commands.Count; i++)
            {
                var cmd = _commands[i];
                if(cmd != null)
                {
                    commands.Add(cmd.ToServer(factory));
                }
            }
            return new ServerLockstepTurnData(commands);
        }
    }    
}