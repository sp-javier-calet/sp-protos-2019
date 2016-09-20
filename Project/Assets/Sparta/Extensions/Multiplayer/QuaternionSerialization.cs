using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Network;
using System;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    public class QuaternionSerializer : IWriteSerializer<JQuaternion>
    {
        public static readonly QuaternionSerializer Instance = new QuaternionSerializer();

        public void Compare(JQuaternion newObj, JQuaternion oldObj, Bitset dirty)
        {
            dirty.Set(newObj.X != oldObj.X);
            dirty.Set(newObj.Y != oldObj.Y);
            dirty.Set(newObj.Z != oldObj.Z);
            dirty.Set(newObj.W != oldObj.W);
        }

        public void Serialize(JQuaternion newObj, IWriter writer)
        {
            writer.Write(newObj.X);
            writer.Write(newObj.Y);
            writer.Write(newObj.Z);
            writer.Write(newObj.W);
        }

        public void Serialize(JQuaternion newObj, JQuaternion oldObj, IWriter writer, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.X);
            }
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.Y);
            }
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.Z);
            }
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.W);
            }
        }
    }

    public class QuaternionParser : IReadParser<JQuaternion>
    {
        public static readonly QuaternionParser Instance = new QuaternionParser();

        public JQuaternion Parse(IReader reader)
        {
            JQuaternion obj;
            obj.X = reader.ReadSingle();
            obj.Y = reader.ReadSingle();
            obj.Z = reader.ReadSingle();
            obj.W = reader.ReadSingle();
            return obj;
        }

        public int GetDirtyBitsSize(JQuaternion obj)
        {
            return 4;
        }

        public JQuaternion Parse(JQuaternion obj, IReader reader, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                obj.X = reader.ReadSingle();
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.Y = reader.ReadSingle();
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.Z = reader.ReadSingle();
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.W = reader.ReadSingle();
            }
            return obj;
        }
    }
}