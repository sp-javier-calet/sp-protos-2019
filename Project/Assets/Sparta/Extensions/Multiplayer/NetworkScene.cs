using System.Collections.Generic;
using System.Collections;
using System;
using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Network;

namespace SocialPoint.Multiplayer
{
    public interface INetworkBehaviour : ICloneable
    {
        void OnStart(NetworkGameObject go);

        void Update(float dt);

        void OnDestroy();
    }

    public class NetworkScene : IEquatable<NetworkScene>, ICloneable
    {
        Dictionary<int,NetworkGameObject> _objects;

        public int FreeObjectId{ get; private set; }

        public int ObjectsCount
        {
            get
            {
                return _objects.Count;
            }
        }

        public NetworkScene()
        {
            _objects = new Dictionary<int,NetworkGameObject>();
            FreeObjectId = 1;
        }

        public NetworkScene(NetworkScene scene) : this()
        {
            if(scene != null)
            {
                FreeObjectId = scene.FreeObjectId;
                if(scene._objects != null)
                {
                    var itr = scene._objects.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        _objects[itr.Current.Key] = new NetworkGameObject(itr.Current.Value);
                    }
                    itr.Dispose();
                }
            }
        }

        public Object Clone()
        {
            return new NetworkScene(this);
        }

        public void AddObject(NetworkGameObject obj)
        {
            if(FindObject(obj.Id) != null)
            {
                throw new InvalidOperationException("Object with same id already exists");
            }
            _objects[obj.Id] = obj;
            if(FreeObjectId == obj.Id)
            {
                FreeObjectId++;
            }
        }

        public bool RemoveObject(int id)
        {
            var r = _objects.Remove(id);
            //TODO: Find a way to reuse ids?
            return r;
        }

        public NetworkGameObject FindObject(int id)
        {
            NetworkGameObject obj;
            if(_objects.TryGetValue(id, out obj))
            {
                return obj;
            }
            return null;
        }

        public IEnumerator<NetworkGameObject> GetObjectEnumerator()
        {
            var objects = new List<NetworkGameObject>(_objects.Values);
            for(var i = 0; i < objects.Count; i++)
            {
                yield return objects[i];
            }
        }

        static bool Compare(NetworkScene a, NetworkScene b)
        {
            return CompareObjects(a._objects, b._objects);
        }

        static bool CompareObjects(Dictionary<int,NetworkGameObject> a, Dictionary<int,NetworkGameObject> b)
        {
            var na = (object)a == null;
            var nb = (object)b == null;
            if(na && nb)
            {
                return true;
            }
            if(na || nb || a.Count != b.Count)
            {
                return false;
            }
            var itr = a.GetEnumerator();
            bool diff = false;
            while(itr.MoveNext())
            {
                NetworkGameObject bgo;
                if(!b.TryGetValue(itr.Current.Key, out bgo))
                {
                    diff = true;
                    break;
                }
                var ago = itr.Current.Value;
                if(!ago.Equals(bgo))
                {
                    diff = true;
                    break;
                }
            }
            itr.Dispose();
            return !diff;
        }

        public override bool Equals(System.Object obj)
        {
            return Equals(obj as NetworkScene);
        }

        public bool Equals(NetworkScene scene)
        {
            if((object)scene == null)
            {
                return false;
            }
            return Compare(this, scene);
        }

        public override int GetHashCode()
        {
            int hash = 0;
            var itr = _objects.GetEnumerator();
            while(itr.MoveNext())
            {
                hash = CryptographyUtils.HashCombine(hash, itr.Current.Value.GetHashCode());
            }
            itr.Dispose();
            return hash;
        }

        public static bool operator ==(NetworkScene a, NetworkScene b)
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

        public static bool operator !=(NetworkScene a, NetworkScene b)
        {
            return !(a == b);
        }
    }

    public class NetworkSceneSerializer : IDiffWriteSerializer<NetworkScene>
    {
        public static readonly NetworkSceneSerializer Instance = new NetworkSceneSerializer();

        public void Compare(NetworkScene newScene, NetworkScene oldScene, Bitset dirty)
        {
        }

        public void Serialize(NetworkScene newScene, IWriter writer)
        {
            writer.Write(newScene.ObjectsCount);
            var itr = newScene.GetObjectEnumerator();
            var gos = NetworkGameObjectSerializer.Instance;
            while(itr.MoveNext())
            {
                gos.Serialize(itr.Current, writer);
            }
            itr.Dispose();
        }

        public void Serialize(NetworkScene newScene, NetworkScene oldScene, IWriter writer, Bitset dirty)
        {
            writer.Write(newScene.ObjectsCount);
            var itr = newScene.GetObjectEnumerator();
            var gos = NetworkGameObjectSerializer.Instance;
            while(itr.MoveNext())
            {
                var go = itr.Current;
                writer.Write(go.Id);
                var oldGo = oldScene.FindObject(go.Id);
                if(oldGo == null)
                {
                    gos.Serialize(go, writer);
                }
                else
                {
                    gos.Serialize(go, oldGo, writer);
                }
            }
            itr.Dispose();
            itr = oldScene.GetObjectEnumerator();
            var removed = new List<int>();
            while(itr.MoveNext())
            {
                var go = itr.Current;
                if(newScene.FindObject(go.Id) == null)
                {
                    removed.Add(go.Id);
                }
            }
            itr.Dispose();
            writer.Write(removed.Count);
            for(var i = 0; i < removed.Count; i++)
            {
                writer.Write(removed[i]);
            }
        }
    }

    public class NetworkSceneParser : IDiffReadParser<NetworkScene>
    {
        public static readonly NetworkSceneParser Instance = new NetworkSceneParser();

        public NetworkScene Parse(IReader reader)
        {
            var obj = new NetworkScene();
            var c = reader.ReadInt32();
            var gop = NetworkGameObjectParser.Instance;
            for(var i = 0; i < c; i++)
            {
                var go = gop.Parse(reader);
                obj.AddObject(go);
            }
            return obj;
        }

        public int GetDirtyBitsSize(NetworkScene obj)
        {
            return 0;
        }

        public NetworkScene Parse(NetworkScene scene, IReader reader, Bitset dirty)
        {
            var c = reader.ReadInt32();
            var gop = NetworkGameObjectParser.Instance;
            for(var i = 0; i < c; i++)
            {
                var id = reader.ReadInt32();
                var go = scene.FindObject(id);
                if(go == null)
                {
                    go = gop.Parse(reader);
                    scene.AddObject(go);
                }
                else
                {
                    gop.Parse(go, reader);
                }
            }
            c = reader.ReadInt32();
            for(var i = 0; i < c; i++)
            {
                var id = reader.ReadInt32();
                scene.RemoveObject(id);
            }
            return scene;
        }
    }
}
