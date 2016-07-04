using System;
using System.Collections.Generic;
using System.IO;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    public static class LockstepCommandDataTypes
    {
        public const byte SetTurnAnticipation = 0;
    }

    public abstract class LockstepCommandData
    {
        public int Turn { get; protected set; }

        public byte Type { get; private set; }

        public LockstepCommandData(int turn, byte type)
        {
            Turn = turn;
            Type = type;
        }

        public virtual void DeserializeCustomData(IReaderWrapper reader)
        {
        }

        public virtual void SerializeCustomData(IWriterWrapper writer)
        {
        }

        public abstract ILockstepCommand LockstepCommand { get; set; }

        public void Serialize(IWriterWrapper writer)
        {
            writer.Write(Type);
            SerializeCustomData(writer);
        }
    }
}