using System.Collections.Generic;
using SocialPoint.IO;
using System.Collections;
using System;

namespace SocialPoint.Multiplayer
{
    public class NetworkGameScene
    {
        Dictionary<int,NetworkGameObject> _objects = new Dictionary<int,NetworkGameObject>();

        public int ObjectsCount
        {
            get
            {
                return _objects.Count;
            }
        }

        public void AddObject(NetworkGameObject obj)
        {
            if(FindObject(obj.Id) != null)
            {
                throw new InvalidOperationException("Object with same id already exists");
            }
            _objects[obj.Id] = obj;
        }

        public void RemoveObject(int id)
        {
            _objects.Remove(id);
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
            var itr = _objects.GetEnumerator();
            while(itr.MoveNext())
            {
                yield return itr.Current.Value;
            }
            itr.Dispose();
        }

        static bool EqualObjects(Dictionary<int,NetworkGameObject> a, Dictionary<int,NetworkGameObject> b)
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
            return Equals((NetworkGameScene)obj);
        }

        public bool Equals(NetworkGameScene scene)
        {
            if((object)scene == null)
            {
                return false;
            }
            return EqualObjects(_objects, scene._objects);
        }

        public override int GetHashCode()
        {
            int code = 0;
            var itr = _objects.GetEnumerator();
            while(itr.MoveNext())
            {
                code ^= itr.Current.Value.GetHashCode();
            }
            itr.Dispose();
            return code;
        }

        public static bool operator ==(NetworkGameScene a, NetworkGameScene b)
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
            return EqualObjects(a._objects, b._objects);
        }

        public static bool operator !=(NetworkGameScene a, NetworkGameScene b)
        {
            return !(a == b);
        }
    }

    public class NetworkGameSceneSerializer : ISerializer<NetworkGameScene>
    {
        NetworkGameObjectSerializer _go = new NetworkGameObjectSerializer();

        public void Compare(NetworkGameScene newScene, NetworkGameScene oldScene, DirtyBits dirty)
        {
        }

        public void Serialize(NetworkGameScene newScene, IWriter writer)
        {
            writer.Write(newScene.ObjectsCount);
            var itr = newScene.GetObjectEnumerator();
            while(itr.MoveNext())
            {
                _go.Serialize(itr.Current, writer);
            }
            itr.Dispose();
        }

        public void Serialize(NetworkGameScene newScene, NetworkGameScene oldScene, IWriter writer, DirtyBits dirty)
        {
            writer.Write(newScene.ObjectsCount);
            var itr = newScene.GetObjectEnumerator();
            while(itr.MoveNext())
            {
                var go = itr.Current;
                writer.Write(go.Id);
                var oldGo = oldScene.FindObject(go.Id);
                if(oldGo == null)
                {
                    _go.Serialize(go, writer);
                }
                else
                {
                    _go.Serialize(go, oldGo, writer);
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

    public class NetworkGameSceneParser : IParser<NetworkGameScene>
    {
        NetworkGameObjectParser _go = new NetworkGameObjectParser();

        public NetworkGameScene Parse(IReader reader)
        {
            var obj = new NetworkGameScene();
            var c = reader.ReadInt32();
            for(var i = 0; i < c; i++)
            {
                var go = _go.Parse(reader);
                obj.AddObject(go);
            }
            return obj;
        }

        public int GetDirtyBitsSize(NetworkGameScene obj)
        {
            return 0;
        }

        public NetworkGameScene Parse(NetworkGameScene scene, IReader reader, DirtyBits dirty)
        {
            var c = reader.ReadInt32();
            for(var i = 0; i < c; i++)
            {
                var id = reader.ReadInt32();
                var go = scene.FindObject(id);
                if(go == null)
                {
                    go = _go.Parse(reader);
                    scene.AddObject(go);
                }
                else
                {
                    _go.Parse(go, reader);
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
