using SocialPoint.IO;
using System;

namespace SocialPoint.Multiplayer
{
    public struct Vector3 : IEquatable<Vector3>
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float v=0.0f)
        {
            x = v;
            y = v;
            z = v;
        }

        public Vector3(float px, float py, float pz)
        {
            x = px;
            y = py;
            z = pz;
        }

        public override bool Equals(System.Object obj)
        {
            var v = (Vector3)obj;
            return (x == v.x) && (y == v.y) && (z == v.z);
        }

        public bool Equals(Vector3 v)
        {             
            return (x == v.x) && (y == v.y) && (z == v.z);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }

        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return !(a == b);
        }
    }

    public class Vector3Serializer : ISerializer<Vector3>
    {
        public void Compare(Vector3 newObj, Vector3 oldObj, DirtyBits dirty)
        {
            dirty.Set(newObj.x != oldObj.x);
            dirty.Set(newObj.y != oldObj.y);
            dirty.Set(newObj.z != oldObj.z);
        }

        public void Serialize(Vector3 newObj, IWriter writer)
        {
            writer.Write(newObj.x);
            writer.Write(newObj.y);
            writer.Write(newObj.z);
        }

        public void Serialize(Vector3 newObj, IWriter writer, DirtyBits dirty)
        {
            if(DirtyBits.NullOrGet(dirty))
            {
                writer.Write(newObj.x);
            }
            if(DirtyBits.NullOrGet(dirty))
            {
                writer.Write(newObj.y);
            }
            if(DirtyBits.NullOrGet(dirty))
            {
                writer.Write(newObj.z);
            }
        }
    }

    public class Vector3Parser : IParser<Vector3>
    {
        public Vector3 Parse(IReader reader)
        {
            Vector3 obj;
            obj.x = reader.ReadSingle();
            obj.y = reader.ReadSingle();
            obj.z = reader.ReadSingle();
            return obj;
        }

        public Vector3 Parse(Vector3 obj, IReader reader)
        {
            var dirty = new DirtyBits();
            dirty.Read(reader, 3);
            if(DirtyBits.NullOrGet(dirty))
            {
                obj.x = reader.ReadSingle();
            }
            if(DirtyBits.NullOrGet(dirty))
            {
                obj.y = reader.ReadSingle();
            }
            if(DirtyBits.NullOrGet(dirty))
            {
                obj.z = reader.ReadSingle();
            }
            return obj;
        }
    }
}