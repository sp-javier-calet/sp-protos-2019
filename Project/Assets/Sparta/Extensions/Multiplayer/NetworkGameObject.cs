using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Geometry;
using System;

namespace SocialPoint.Multiplayer
{
    public class NetworkGameObject : IEquatable<NetworkGameObject>, ICloneable
    {
        public int Id{ get; private set; }

        public Transform Transform;

        public NetworkGameObject(int id)
        {
            Id = id;
            Transform = new Transform();
        }

        public NetworkGameObject(int id, Transform t)
        {
            Id = id;
            Transform = t;
        }

        public NetworkGameObject(NetworkGameObject go)
        {
            if(go != null)
            {
                Id = go.Id;
                Transform = new Transform(go.Transform);
            }
            else
            {
                Transform = new Transform();
            }
        }

        public object Clone()
        {
            return new NetworkGameObject(this);
        }

        public override bool Equals(System.Object obj)
        {
            return Equals(obj as NetworkGameObject);
        }

        public bool Equals(NetworkGameObject go)
        {
            if((object)go == null)
            {
                return false;
            }
            return Compare(this, go);
        }

        public override int GetHashCode()
        {
            var hash = Id.GetHashCode();
            hash = CryptographyUtils.HashCombine(hash, Transform.GetHashCode());
            return hash;
        }

        public static bool operator ==(NetworkGameObject a, NetworkGameObject b)
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

        public static bool operator !=(NetworkGameObject a, NetworkGameObject b)
        {
            return !(a == b);
        }

        static bool Compare(NetworkGameObject a, NetworkGameObject b)
        {
            return a.Id == b.Id && a.Transform == b.Transform;
        }

        public override string ToString()
        {
            return string.Format("[NetworkGameObject:{0} {1}]", Id, Transform);
        }
    }

    public class NetworkGameObjectSerializer : IDiffWriteSerializer<NetworkGameObject>
    {
        public static readonly NetworkGameObjectSerializer Instance = new NetworkGameObjectSerializer();

        public void Compare(NetworkGameObject newObj, NetworkGameObject oldObj, Bitset dirty)
        {
            dirty.Set(newObj.Transform != oldObj.Transform);
        }

        public void Serialize(NetworkGameObject newObj, IWriter writer)
        {
            writer.Write(newObj.Id);
            TransformSerializer.Instance.Serialize(newObj.Transform, writer);

        }

        public void Serialize(NetworkGameObject newObj, NetworkGameObject oldObj, IWriter writer, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                TransformSerializer.Instance.Serialize(newObj.Transform, oldObj.Transform, writer);
            }
        }
    }

    public class NetworkGameObjectParser : IDiffReadParser<NetworkGameObject>
    {
        public static readonly NetworkGameObjectParser Instance = new NetworkGameObjectParser();

        public NetworkGameObject Parse(IReader reader)
        {
            var obj = new NetworkGameObject(reader.ReadInt32());
            obj.Transform = TransformParser.Instance.Parse(reader);
            return obj;
        }

        public int GetDirtyBitsSize(NetworkGameObject obj)
        {
            return 1;
        }

        public NetworkGameObject Parse(NetworkGameObject obj, IReader reader, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                obj.Transform = TransformParser.Instance.Parse(obj.Transform, reader);
            }
            return obj;
        }
    }
}
