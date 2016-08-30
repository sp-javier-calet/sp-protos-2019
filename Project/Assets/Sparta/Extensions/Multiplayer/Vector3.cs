using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Network;
using System;

namespace SocialPoint.Multiplayer
{
    public struct Vector3 : IEquatable<Vector3>
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float v = 0.0f)
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
                default:
                    throw new InvalidOperationException("Index out of range.");
                }
            }
        }

        public override bool Equals(System.Object obj)
        {
            if(!(obj is Vector3))
            {
                return false;
            }
            return this == (Vector3)obj;
        }

        public bool Equals(Vector3 v)
        {             
            return this == v;
        }

        public override int GetHashCode()
        {
            var hash = x.GetHashCode();
            hash = CryptographyUtils.HashCombine(hash, y.GetHashCode());
            hash = CryptographyUtils.HashCombine(hash, z.GetHashCode());
            return hash;
        }

        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return !(a == b);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator *(Vector3 a, float b)
        {
            return new Vector3(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3 operator /(Vector3 a, float b)
        {
            return new Vector3(a.x / b, a.y / b, a.z / b);
        }

        public static Vector3 Zero
        {
            get
            {
                return new Vector3(0.0f);
            }
        }

        public static Vector3 One
        {
            get
            {
                return new Vector3(1.0f);
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
                return x * x + y * y + z * z;
            }
        }

        public Vector3 Normalized
        {
            get
            {
                return this / Magnitude;
            }
        }

        public static float Distance(Vector3 a, Vector3 b)
        {
            return (a - b).Magnitude;
        }

        public static float Angle(Vector3 a, Vector3 b)
        {
            var v = (a.x * b.x + a.y * b.y + a.z * b.z) / (a.Magnitude * b.Magnitude);
            return (float)Math.Acos(v);
        }

        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.y * b.z - b.y * a.z,
                a.z * b.x - b.z * a.x,
                a.x * b.y - b.x * a.y);
        }

        public static float Dot(Vector3 a, Vector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1},{2}]", x, y, z);
        }
    }

    public class Vector3Serializer : IWriteSerializer<Vector3>
    {
        public static readonly Vector3Serializer Instance = new Vector3Serializer();

        public void Compare(Vector3 newObj, Vector3 oldObj, Bitset dirty)
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

        public void Serialize(Vector3 newObj, Vector3 oldObj, IWriter writer, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.x);
            }
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.y);
            }
            if(Bitset.NullOrGet(dirty))
            {
                writer.Write(newObj.z);
            }
        }
    }

    public class Vector3Parser : IReadParser<Vector3>
    {
        public static readonly Vector3Parser Instance = new Vector3Parser();

        public Vector3 Parse(IReader reader)
        {
            Vector3 obj;
            obj.x = reader.ReadSingle();
            obj.y = reader.ReadSingle();
            obj.z = reader.ReadSingle();
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
                obj.x = reader.ReadSingle();
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.y = reader.ReadSingle();
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.z = reader.ReadSingle();
            }
            return obj;
        }
    }
}