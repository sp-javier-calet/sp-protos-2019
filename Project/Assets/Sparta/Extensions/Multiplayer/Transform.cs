using SocialPoint.IO;
using System;

namespace SocialPoint.Multiplayer
{
    public struct Transform : IEquatable<Transform>
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Size;

        public Transform(Vector3 p, Quaternion r, Vector3 s)
        {
            Position = p;
            Rotation = r;
            Size = s;
        }

        public Transform(Vector3 p, Quaternion r):this(p, r, Vector3.Zero)
        {
        }

        public Transform(Vector3 p):this(p, Quaternion.Identity)
        {
        }

        public override bool Equals(System.Object obj)
        {
            var t = (Transform)obj;
            return (Position == t.Position) && (Rotation == t.Rotation) && (Size == t.Size);
        }

        public bool Equals(Transform t)
        {             
            return (Position == t.Position) && (Rotation == t.Rotation) && (Size == t.Size);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ Rotation.GetHashCode() ^ Size.GetHashCode();
        }

        public static bool operator ==(Transform a, Transform b)
        {
            return a.Position == b.Position && a.Rotation == b.Rotation && a.Size == b.Size;
        }

        public static bool operator !=(Transform a, Transform b)
        {
            return !(a == b);
        }


        public override string ToString()
        {
            return string.Format("[Transform: Position={0}, Rotation={1}, Size={2}]", Position, Rotation, Size);
        }
    }

    public class TransformSerializer : ISerializer<Transform>
    {
        Vector3Serializer _vector3 = new Vector3Serializer();
        QuaternionSerializer _quat = new QuaternionSerializer();

        public void Compare(Transform newObj, Transform oldObj, DirtyBits dirty)
        {
            dirty.Set(newObj.Position != oldObj.Position);
            dirty.Set(newObj.Rotation != oldObj.Rotation);
            dirty.Set(newObj.Size != oldObj.Size);
        }

        public void Serialize(Transform newObj, IWriter writer)
        {
            _vector3.Serialize(newObj.Position, writer);
            _quat.Serialize(newObj.Rotation, writer);
            _vector3.Serialize(newObj.Size, writer);
        }

        public void Serialize(Transform newObj, Transform oldObj, IWriter writer, DirtyBits dirty)
        {
            if(DirtyBits.NullOrGet(dirty))
            {
                _vector3.Serialize(newObj.Position, oldObj.Position, writer);
            }
            if(DirtyBits.NullOrGet(dirty))
            {
                _quat.Serialize(newObj.Rotation, oldObj.Rotation, writer);
            }
            if(DirtyBits.NullOrGet(dirty))
            {
                _vector3.Serialize(newObj.Size, oldObj.Size, writer);
            }
        }
    }

    public class TransformParser : IParser<Transform>
    {
        Vector3Parser _vector3 = new Vector3Parser();
        QuaternionParser _quat = new QuaternionParser();

        public Transform Parse(IReader reader)
        {
            Transform obj;
            obj.Position = _vector3.Parse(reader);
            obj.Rotation = _quat.Parse(reader);
            obj.Size = _vector3.Parse(reader);
            return obj;
        }

        public int GetDirtyBitsSize(Transform obj)
        {
            return 3;
        }

        public Transform Parse(Transform obj, IReader reader, DirtyBits dirty)
        {
            if(DirtyBits.NullOrGet(dirty))
            {
                obj.Position = _vector3.Parse(obj.Position, reader);
            }
            if(DirtyBits.NullOrGet(dirty))
            {
                obj.Rotation = _quat.Parse(obj.Rotation, reader);
            }
            if(DirtyBits.NullOrGet(dirty))
            {
                obj.Size = _vector3.Parse(obj.Size, reader);
            }
            return obj;
        }
    }
}