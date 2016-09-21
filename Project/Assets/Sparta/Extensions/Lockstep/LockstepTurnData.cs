using System.Collections.Generic;

namespace SocialPoint.Lockstep
{
    public sealed class ClientLockstepTurnData
    {
        public int Turn { get; private set; }

        public List<ClientLockstepCommandData> Commands { get; set; }

        public ClientLockstepTurnData(int turn)
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

        public ServerLockstepTurnData ToServer(LockstepCommandFactory factory)
        {
            var commands = new List<ServerLockstepCommandData>(Commands.Count);
            for(var i=0; i<Commands.Count;i++)
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

    public sealed class ServerLockstepTurnData
    {
        public int Turn { get; private set; }

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

        public ClientLockstepTurnData ToClient(LockstepCommandFactory factory)
        {
            var commands = new List<ClientLockstepCommandData>(Commands.Count);
            for(var i=0; i<Commands.Count;i++)
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