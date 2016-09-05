using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Network;
using System;
using BulletSharp.Math;

namespace SocialPoint.Multiplayer
{
    public class Vector3Serializer : IWriteSerializer<Vector3>
    {
        public static readonly Vector3Serializer Instance = new Vector3Serializer();

        public void Compare(Vector3 newObj, Vector3 oldObj, Bitset dirty)
        {
            dirty.Set(newObj.X != oldObj.X);
            dirty.Set(newObj.Y != oldObj.Y);
            dirty.Set(newObj.Z != oldObj.Z);
        }

        public void Serialize(Vector3 newObj, IWriter writer)
        {
            writer.Write(newObj.X);
            writer.Write(newObj.Y);
            writer.Write(newObj.Z);
        }

        public void Serialize(Vector3 newObj, Vector3 oldObj, IWriter writer, Bitset dirty)
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
        }
    }

    public class Vector3Parser : IReadParser<Vector3>
    {
        public static readonly Vector3Parser Instance = new Vector3Parser();

        public Vector3 Parse(IReader reader)
        {
            Vector3 obj;
            obj.X = reader.ReadSingle();
            obj.Y = reader.ReadSingle();
            obj.Z = reader.ReadSingle();
            return obj;
        }

        public int GetDirtyBitsSize(Vector3 obj)
        {
            return 3;
        }

        public Vector3 Parse(Vector3 obj, IReader reader, Bitset dirty)
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
            return obj;
        }
    }
}