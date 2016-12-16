using SocialPoint.IO;
using SocialPoint.Utils;
using System;

namespace SocialPoint.Geometry
{
    public class Transform : IEquatable<Transform>, ICloneable
    {
        public Vector Position;

        public Quat Rotation;

        public Vector Scale;

        public Transform(Vector p, Quat r, Vector s)
        {
            Position = p;
            Rotation = r;
            Scale = s;
        }

        public Transform(Vector p, Quat r) : this(p, r, Vector.One)
        {
        }

        public Transform(Vector p) : this(p, Quat.Identity)
        {
        }

        public Transform() : this(Vector.Zero)
        {
        }

        public Transform(Transform t)
        {
            if(t != null)
            {
                Position = t.Position;
                Rotation = t.Rotation;
                Scale = t.Scale;
            }
        }

        public object Clone()
        {
            return new Transform(this);
        }

        public override bool Equals(System.Object obj)
        {
            var go = (Transform)obj;
            if((object)go == null)
            {
                return false;
            }
            return Compare(this, go);
        }

        public bool Equals(Transform go)
        {
            if((object)go == null)
            {
                return false;
            }
            return Compare(this, go);
        }

        public static bool operator ==(Transform a, Transform b)
        {
            var na = (object)a == null;
            var nb = (object)b == null;
            if(na && nb)
            {
                return true;
            }
            else if(na || nb)
            {
                return false;
            }
            return Compare(a, b);
        }

        public static bool operator !=(Transform a, Transform b)
        {
            return !(a == b);
        }

        static bool Compare(Transform a, Transform b)
        {
            return a.Position == b.Position && a.Rotation == b.Rotation && a.Scale == b.Scale;
        }

        public override int GetHashCode()
        {
            var hash = Position.GetHashCode();
            hash = CryptographyUtils.HashCombine(hash, Rotation.GetHashCode());
            hash = CryptographyUtils.HashCombine(hash, Scale.GetHashCode());
            return hash;
        }

        public static Transform Identity
        {
            get
            {
                return new Transform(Vector.Zero);
            }
        }

        public override string ToString()
        {
            return string.Format("[Transform: Position={0}, Rotation={1}, Scale={2}]", Position, Rotation, Scale);
        }
    }

    public class TransformSerializer : IDiffWriteSerializer<Transform>
    {
        public static readonly TransformSerializer Instance = new TransformSerializer();

        public void Compare(Transform newObj, Transform oldObj, Bitset dirty)
        {
            dirty.Set(newObj.Position != oldObj.Position);
            dirty.Set(newObj.Rotation != oldObj.Rotation);
            dirty.Set(newObj.Scale != oldObj.Scale);
        }

        public void Serialize(Transform newObj, IWriter writer)
        {
            VectorSerializer.Instance.Serialize(newObj.Position, writer);
            QuatSerializer.Instance.Serialize(newObj.Rotation, writer);
            VectorSerializer.Instance.Serialize(newObj.Scale, writer);
        }

        public void Serialize(Transform newObj, Transform oldObj, IWriter writer, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                VectorSerializer.Instance.Serialize(newObj.Position, oldObj.Position, writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                QuatSerializer.Instance.Serialize(newObj.Rotation, oldObj.Rotation, writer);
            }
            if(Bitset.NullOrGet(dirty))
            {
                VectorSerializer.Instance.Serialize(newObj.Scale, oldObj.Scale, writer);
            }
        }
    }

    public class TransformParser : IDiffReadParser<Transform>
    {
        public static readonly TransformParser Instance = new TransformParser();

        public Transform Parse(IReader reader)
        {
            var obj = new Transform();
            obj.Position = VectorParser.Instance.Parse(reader);
            obj.Rotation = QuatParser.Instance.Parse(reader);
            obj.Scale = VectorParser.Instance.Parse(reader);
            return obj;
        }

        public int GetDirtyBitsSize(Transform obj)
        {
            return 3;
        }

        public Transform Parse(Transform obj, IReader reader, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                obj.Position = VectorParser.Instance.Parse(obj.Position, reader);
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.Rotation = QuatParser.Instance.Parse(obj.Rotation, reader);
            }
            if(Bitset.NullOrGet(dirty))
            {
                obj.Scale = VectorParser.Instance.Parse(obj.Scale, reader);
            }
            return obj;
        }
    }
}