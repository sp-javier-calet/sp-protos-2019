using System;
using System.Collections.Generic;
using System.IO;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep.Network
{
    public class LockstepCommandMessage : INetworkMessage
    {
        public ILockstepCommand LockstepCommand { get; private set; }

        public int Client;

        LockstepCommandDataFactory _commandDataFactory;

        public LockstepCommandMessage(LockstepCommandDataFactory commandDataFactory, ILockstepCommand command = null)
        {
            _commandDataFactory = commandDataFactory;
            LockstepCommand = command;
        }

        public void Deserialize(IReaderWrapper reader)
        {
            int turn = reader.ReadInt32();
            var commandData = _commandDataFactory.CreateNetworkLockstepCommandData(turn, reader);
            LockstepCommand = commandData.LockstepCommand;
        }

        public void Serialize(IWriterWrapper writer)
        {
            if(LockstepCommand != null)
            {
                writer.Write(LockstepCommand.Turn);
                var commandData = _commandDataFactory.CreateNetworkLockstepCommandData(LockstepCommand);
                commandData.Serialize(writer);
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