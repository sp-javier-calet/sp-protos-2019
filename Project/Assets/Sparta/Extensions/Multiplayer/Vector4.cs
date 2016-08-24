using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Network;
using System;

namespace SocialPoint.Multiplayer
{
    public struct Vector4 : IEquatable<Vector4>
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Vector4(float v=0.0f)
        {
            x = v;
            y = v;
            z = v;
            w = v;
        }

        public Vector4(float px, float py, float pz, float pw)
        {
            x = px;
            y = py;
            z = pz;
            w = pw;
        }

        public float this[int key]
        {
            get
            {
                switch(key)
                {
                case 0:
                    return x;
                case 1:
                    return y;
                case 2:
                    return z;
                case 3:
                    return w;
                }
                throw new InvalidOperationException("Index out of range.");
            }
            set
            {
                switch(key)
                {
                case 0:
                    x = value;
                    break;
                case 1:
                    y = value;
                    break;
                case 2:
                    z = value;
                    break;
                case 3:
                    w = value;
                    break;
                default:
                    throw new InvalidOperationException("Index out of range.");
                }
            }
        }

        public override bool Equals(System.Object obj)
        {
            return this == (Vector4)obj;
        }

        public bool Equals(Vector4 v)
        {             
            return this == v;
        }

        public override int GetHashCode()
        {
            var hash = x.GetHashCode();
            hash = CryptographyUtils.HashCombine(hash, y.GetHashCode());
            hash = CryptographyUtils.HashCombine(hash, z.GetHashCode());
            hash = CryptographyUtils.HashCombine(hash, w.GetHashCode());
            return hash;
        }

        public static bool operator ==(Vector4 a, Vector4 b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
        }

        public static bool operator !=(Vector4 a, Vector4 b)
        {
            return !(a == b);
        }            

        public static Vector4 operator -(Vector4 a, Vector4 b)
        {
            return new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }

        public static Vector4 operator +(Vector4 a, Vector4 b)
        {
            return new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }

        public static Vector4 operator *(Vector4 a, float b)
        {
            return new Vector4(a.x * b, a.y * b, a.z * b, a.w * b);
        }

        public static Vector4 operator /(Vector4 a, float b)
        {
            return new Vector4(a.x / b, a.y / b, a.z / b, a.w / b);
        }

        public static Vector4 Zero
        {
            get
            {
                return new Vector4(0.0f);
            }
        }

        public static Vector4 One
        {
            get
            {
                return new Vector4(1.0f);
            }
        }

        static float Sqrt(float v)
        {
            return (float)Math.Sqrt(v);
        }

        public float Magnitude
        {
            get
            {
                return Sqrt(SqrMagnitude);
            }
        }

        public float SqrMagnitude
        {
            get
            {
                return x * x + y * y + z * z + w * w;
            }
        }

        public Vector4 Normalized
        {
            get
            {
                return this / Magnitude;
            }
        }

        public static float Distance(Vector4 a, Vector4 b)
        {
            return (a - b).Magnitude;
        }

        public static float Angle(Vector4 a, Vector4 b)
        {
            var v = (a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w) / (a.Magnitude * b.Magnitude);
            return (float)Math.Acos(v);
        }

        public static Vector3 Cross(Vector4 a, Vector4 b)
        {
            return new Vector3(
                a.y * b.z - b.y * a.z,
                a.z * b.x - b.z * a.x,
                a.x * b.y - b.x * a.y);
        }

        public static float Dot(Vector4 a, Vector4 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1},{2},{3}]", x, y, z ,w);
        }
    }

    public class Vector4Serializer : ISerializer<Vector4>
    {
        public static readonly Vector4Serializer Instance = new Vector4Serializer();

        public void Compare(Vector4 newObj, Vector4 oldObj, DirtyBits dirty)
        {
            dirty.Set(newObj.x != oldObj.x);
            dirty.Set(newObj.y != oldObj.y);
            dirty.Set(newObj.z != oldObj.z);
            dirty.Set(newObj.w != oldObj.w);
        }

        public void Serialize(Vector4 newObj, IWriter writer)
        {
            writer.Write(newObj.x);
            writer.Write(newObj.y);
            writer.Write(newObj.z);
            writer.Write(newObj.w);
        }

        public void Serialize(Vector4 newObj, Vector4 oldObj, IWriter writer, DirtyBits dirty)
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

    public class Vector4Parser : IParser<Vector4>
    {
        public static readonly Vector4Parser Instance = new Vector4Parser();

        public Vector4 Parse(IReader reader)
        {
            Vector4 obj;
            obj.x = reader.ReadSingle();
            obj.y = reader.ReadSingle();
            obj.z = reader.ReadSingle();
            obj.w = reader.ReadSingle();
            return obj;
        }

        public int GetDirtyBitsSize(Vector4 obj)
        {
            return 4;
        }

        public Vector4 Parse(Vector4 obj, IReader reader, DirtyBits dirty)
        {
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