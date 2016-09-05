using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Network;
using System;
using BulletSharp.Math;

namespace SocialPoint.Multiplayer
{
    public class QuaternionSerializer : IWriteSerializer<Quaternion>
    {
        public static readonly QuaternionSerializer Instance = new QuaternionSerializer();

        public void Compare(Quaternion newObj, Quaternion oldObj, Bitset dirty)
        {
            dirty.Set(newObj.X != oldObj.X);
            dirty.Set(newObj.Y != oldObj.Y);
            dirty.Set(newObj.Z != oldObj.Z);
            dirty.Set(newObj.W != oldObj.W);
        }

        public void Serialize(Quaternion newObj, IWriter writer)
        {
            writer.Write(newObj.X);
            writer.Write(newObj.Y);
            writer.Write(newObj.Z);
            writer.Write(newObj.W);
        }

        public void Serialize(Quaternion newObj, Quaternion oldObj, IWriter writer, Bitset dirty)
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

    public class QuaternionParser : IReadParser<Quaternion>
    {
        public static readonly QuaternionParser Instance = new QuaternionParser();

        public Quaternion Parse(IReader reader)
        {
            Quaternion obj;
            obj.X = reader.ReadSingle();
            obj.Y = reader.ReadSingle();
            obj.Z = reader.ReadSingle();
            obj.W = reader.ReadSingle();
            return obj;
        }

        public int GetDirtyBitsSize(Quaternion obj)
        {
            return 4;
        }

        public Quaternion Parse(Quaternion obj, IReader reader, Bitset dirty)
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