using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Lockstep
{    
    public sealed class ServerLockstepTurnData : INetworkShareable
    {
        List<ServerLockstepCommandData> _commands;

        public ServerLockstepTurnData(List<ServerLockstepCommandData> commands=null)
        {
            if(commands == null)
            {
                commands = new List<ServerLockstepCommandData>();
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

        public void AddCommand(ServerLockstepCommandData cmd)
        {
            _commands.Add(cmd);
        }

        public void Clear()
        {
            _commands.Clear();
        }

        public override string ToString()
        {
            return string.Format("[ServerLockstepTurnData:{0}]", _commands.Count);
        }

        public void Deserialize(IReader reader)
        {
            int commandCount = (int)reader.ReadByte();
            _commands = new List<ServerLockstepCommandData>();
            for(int j = 0; j < commandCount; ++j)
            {
                var command = new ServerLockstepCommandData();
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

        public ClientLockstepTurnData ToClient(LockstepCommandFactory factory)
        {
            var commands = new List<ClientLockstepCommandData>(_commands.Count);
            for(var i = 0; i < _commands.Count; i++)
            {
                var cmd = _commands[i];
                if(cmd != null)
                {
                    commands.Add(cmd.ToClient(factory));
                }
            }
            return new ClientLockstepTurnData(commands);
        }
    }
}