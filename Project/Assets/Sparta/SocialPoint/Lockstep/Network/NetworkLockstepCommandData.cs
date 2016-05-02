using System;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace SocialPoint.Lockstep.Network
{
    public static class NetworkLockstepCommandDataTypes
    {
        public const byte SetTurnAnticipation = 0;
    }

    public abstract class NetworkLockstepCommandData
    {
        public int Turn { get; protected set; }

        public byte Type { get; private set; }

        public NetworkLockstepCommandData(int turn, byte type)
        {
            Turn = turn;
            Type = type;
        }

        public virtual void DeserializeCustomData(NetworkReader reader)
        {
        }

        public virtual void SerializeCustomData(NetworkWriter writer)
        {
        }

        public abstract ILockstepCommand LockstepCommand { get; set; }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(Type);
            SerializeCustomData(writer);
        }
    }
}