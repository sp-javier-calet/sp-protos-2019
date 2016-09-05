using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Network;
using System;
using BulletSharp.Math;

namespace SocialPoint.Multiplayer
{
    public class Vector4Serializer : IWriteSerializer<Vector4>
    {
        public static readonly Vector4Serializer Instance = new Vector4Serializer();

        public void Compare(Vector4 newObj, Vector4 oldObj, Bitset dirty)
        {
            dirty.Set(newObj.X != oldObj.X);
            dirty.Set(newObj.Y != oldObj.Y);
            dirty.Set(newObj.Z != oldObj.Z);
            dirty.Set(newObj.W != oldObj.W);
        }

        public void Serialize(Vector4 newObj, IWriter writer)
        {
            writer.Write(newObj.X);
            writer.Write(newObj.Y);
            writer.Write(newObj.Z);
            writer.Write(newObj.W);
        }

        public void Serialize(Vector4 newObj, Vector4 oldObj, IWriter writer, Bitset dirty)
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

    public class Vector4Parser : IReadParser<Vector4>
    {
        public static readonly Vector4Parser Instance = new Vector4Parser();

        public Vector4 Parse(IReader reader)
        {
            Vector4 obj;
            obj.X = reader.ReadSingle();
            obj.Y = reader.ReadSingle();
            obj.Z = reader.ReadSingle();
            obj.W = reader.ReadSingle();
            return obj;
        }

        public int GetDirtyBitsSize(Vector4 obj)
        {
            return 4;
        }

        public Vector4 Parse(Vector4 obj, IReader reader, Bitset dirty)
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