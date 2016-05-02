using System;
using UnityEngine.Networking;
using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep.Network
{
    public struct TurnTypeTuple
    {
        public int Turn;
        public byte Type;
    }

    public class NetworkLockstepCommandDataFactory : FamilyGenericFactory<TurnTypeTuple, NetworkLockstepCommandData>
    {
        public NetworkLockstepCommandDataFactory(List<IGenericFactory<TurnTypeTuple, NetworkLockstepCommandData>> factories = null)
            : base(factories)
        {
        }

        public NetworkLockstepCommandData CreateNetworkLockstepCommandData(ILockstepCommand command)
        {
            var commandData = Create(new TurnTypeTuple {
                Turn = command.Turn,
                Type = command.LockstepCommandDataType
            });
            commandData.LockstepCommand = command;
            return commandData;
        }

        public NetworkLockstepCommandData CreateNetworkLockstepCommandData(int turn, NetworkReader reader)
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