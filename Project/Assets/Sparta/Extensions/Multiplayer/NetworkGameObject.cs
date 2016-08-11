using SocialPoint.IO;
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
        }

        public NetworkGameObject(int id, Transform t)
        {
            Id = id;
            Transform = t;
        }

        public NetworkGameObject(NetworkGameObject go):
        this(go.Id, go.Transform)
        {            
        }

        public object Clone()
        {
            return new NetworkGameObject(this);
        }

        public override bool Equals(System.Object obj)
        {
            var go = (NetworkGameObject)obj;
            if((object)go == null)
            {
                return false;
            }
            return (Id == go.Id) && (Transform == go.Transform);
        }

        public bool Equals(NetworkGameObject go)
        {
            if((object)go == null)
            {
                return false;
            }
            return (Id == go.Id) && (Transform == go.Transform);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Transform.GetHashCode();
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
            return a.Id == b.Id && a.Transform == b.Transform;
        }

        public static bool operator !=(NetworkGameObject a, NetworkGameObject b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return string.Format("[NetworkGameObject:{0} {1}]", Id, Transform);
        }
    }

    public class NetworkGameObjectSerializer : ISerializer<NetworkGameObject>
    {
        TransformSerializer _trans = new TransformSerializer();

        public void Compare(NetworkGameObject newObj, NetworkGameObject oldObj, DirtyBits dirty)
        {
            dirty.Set(newObj.Transform != oldObj.Transform);
        }

        public void Serialize(NetworkGameObject newObj, IWriter writer)
        {
            writer.Write(newObj.Id);
            _trans.Serialize(newObj.Transform, writer);

        }

        public void Serialize(NetworkGameObject newObj, NetworkGameObject oldObj, IWriter writer, DirtyBits dirty)
        {
            if(DirtyBits.NullOrGet(dirty))
            {
                _trans.Serialize(newObj.Transform, oldObj.Transform, writer);
            }
        }
    }

    public class NetworkGameObjectParser : IParser<NetworkGameObject>
    {
        TransformParser _trans = new TransformParser();

        public NetworkGameObject Parse(IReader reader)
        {
            var obj = new NetworkGameObject(reader.ReadInt32());
            obj.Transform = _trans.Parse(reader);
            return obj;
        }

        public int GetDirtyBitsSize(NetworkGameObject obj)
        {
            return 1;
        }

        public NetworkGameObject Parse(NetworkGameObject obj, IReader reader, DirtyBits dirty)
        {
            if(DirtyBits.NullOrGet(dirty))
            {
                obj.Transform = _trans.Parse(obj.Transform, reader);
            }
            return obj;
        }
    }
}
