using System;
using System.Collections.Generic;
using SocialPoint.Utils;
using System.IO;

namespace SocialPoint.Lockstep
{
    public struct TurnTypeTuple
    {
        public int Turn;
        public byte Type;
    }

    public class LockstepCommandDataFactory : FamilyGenericFactory<TurnTypeTuple, LockstepCommandData>
    {
        public LockstepCommandDataFactory(List<IGenericFactory<TurnTypeTuple, LockstepCommandData>> factories = null)
            : base(factories)
        {
        }

        public LockstepCommandData CreateNetworkLockstepCommandData(ILockstepCommand command)
        {
            var commandData = Create(new TurnTypeTuple {
                Turn = command.Turn,
                Type = command.LockstepCommandDataType
            });
            commandData.LockstepCommand = command;
            return commandData;
        }

        public LockstepCommandData CreateNetworkLockstepCommandData(int turn, IReaderWrapper reader)
        {
            byte type = reader.ReadByte();
            var commandData = Create(new TurnTypeTuple {
                Turn = turn,
                Type = type
            });
            if(commandData != null)
            {
                commandData.DeserializeCustomData(reader);
            }
            return commandData;
        }
    }
}