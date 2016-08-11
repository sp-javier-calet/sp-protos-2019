using SocialPoint.IO;
using System;

namespace SocialPoint.Multiplayer
{
    public struct Quaternion : IEquatable<Quaternion>
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quaternion(float v=0.0f)
        {
            x = v;
            y = v;
            z = v;
            w = v;
        }

        public Quaternion(float px, float py, float pz, float pw)
        {
            x = px;
            y = py;
            z = pz;
            w = pw;
        }

        public override bool Equals(System.Object obj)
        {
            var v = (Quaternion)obj;
            return x == v.x && y == v.y && z == v.z && w == v.w;
        }

        public bool Equals(Quaternion v)
        {             
            return x == v.x && y == v.y && z == v.z && w == v.w;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();
        }

        public static bool operator ==(Quaternion a, Quaternion b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
        }

        public static bool operator !=(Quaternion a, Quaternion b)
        {
            return !(a == b);
        }            

        public static Quaternion Identity
        {
            get
            {
                return new Quaternion(1.0f);
            }
        }

        public override string ToString()
        {
            return string.Format("[{0},{1},{2},{3}]", x, y, z ,w);
        }
    }

    public class QuaternionSerializer : ISerializer<Quaternion>
    {
        public void Compare(Quaternion newObj, Quaternion oldObj, DirtyBits dirty)
        {
            dirty.Set(newObj.x != oldObj.x);
            dirty.Set(newObj.y != oldObj.y);
            dirty.Set(newObj.z != oldObj.z);
            dirty.Set(newObj.w != oldObj.w);
        }

        public void Serialize(Quaternion newObj, IWriter writer)
        {
            writer.Write(newObj.x);
            writer.Write(newObj.y);
            writer.Write(newObj.z);
            writer.Write(newObj.w);
        }

        public void Serialize(Quaternion newObj, Quaternion oldObj, IWriter writer, DirtyBits dirty)
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
            if(DirtyBits.NullOrGet(dirty))
            {
                writer.Write(newObj.w);
            }
        }
    }

    public class QuaternionParser : IParser<Quaternion>
    {
        public Quaternion Parse(IReader reader)
        {
            Quaternion obj;
            obj.x = reader.ReadSingle();
            obj.y = reader.ReadSingle();
            obj.z = reader.ReadSingle();
            obj.w = reader.ReadSingle();
            return obj;
        }

        public Quaternion Parse(Quaternion obj, IReader reader)
        {
            var dirty = new DirtyBits();
            dirty.Read(reader, 4);
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
            if(DirtyBits.NullOrGet(dirty))
            {
                obj.w = reader.ReadSingle();
            }
            return obj;
        }
    }
}