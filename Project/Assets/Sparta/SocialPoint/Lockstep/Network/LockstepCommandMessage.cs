using System;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SocialPoint.Lockstep.Network
{
    public class LockstepCommandMessage : MessageBase
    {
        public ILockstepCommand LockstepCommand { get; private set; }

        public int Client;

        NetworkLockstepCommandDataFactory _commandDataFactory;

        public LockstepCommandMessage(NetworkLockstepCommandDataFactory commandDataFactory, ILockstepCommand command = null)
        {
            _commandDataFactory = commandDataFactory;
            LockstepCommand = command;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            int turn = reader.ReadInt32();
            var commandData = _commandDataFactory.CreateNetworkLockstepCommandData(turn, reader);
            LockstepCommand = commandData.LockstepCommand;
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            if(LockstepCommand != null)
            {
                writer.Write(LockstepCommand.Turn);
                var commandData = _commandDataFactory.CreateNetworkLockstepCommandData(LockstepCommand);
                commandData.Serialize(writer);
            }
        }
    }
}