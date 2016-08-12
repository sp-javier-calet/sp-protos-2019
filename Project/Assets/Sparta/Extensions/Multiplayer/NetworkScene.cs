﻿using System.Collections.Generic;
using SocialPoint.IO;
using System.Collections;
using System;

namespace SocialPoint.Multiplayer
{
    public interface INetworkBehaviour : ICloneable
    {
        void OnStart(NetworkGameObject go);
        void Update(float dt);
        void OnDestroy();
    }

    public static class SceneMsgType
    {
        public const byte UpdateSceneEvent = 1;
        public const byte InstantiateObjectEvent = 2;
        public const byte DestroyObjectEvent = 3;
        public const byte Highest = 3;
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

        public NetworkScene(NetworkScene scene):this()
        {
            if(scene != null && scene._objects != null)
            {
                var itr = scene._objects.GetEnumerator();
                while(itr.MoveNext())
                {
                    _objects[itr.Current.Key] = new NetworkGameObject(itr.Current.Value);
                }
                itr.Dispose();
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
            if(FreeObjectId > id)
            {
                FreeObjectId = id;
            }
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
            return Equals((NetworkScene)obj);
        }

        public bool Equals(NetworkScene scene)
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
            return EqualObjects(a._objects, b._objects);
        }

        public static bool operator !=(NetworkScene a, NetworkScene b)
        {
            return !(a == b);
        }
    }

    public class NetworkGameSceneSerializer : ISerializer<NetworkScene>
    {
        NetworkGameObjectSerializer _go = new NetworkGameObjectSerializer();

        public void Compare(NetworkScene newScene, NetworkScene oldScene, DirtyBits dirty)
        {
        }

        public void Serialize(NetworkScene newScene, IWriter writer)
        {
            UnityEngine.Debug.Log("initial serialize " + newScene.ObjectsCount);
            writer.Write(newScene.ObjectsCount);
            var itr = newScene.GetObjectEnumerator();
            while(itr.MoveNext())
            {
                _go.Serialize(itr.Current, writer);
            }
            itr.Dispose();
        }

        public void Serialize(NetworkScene newScene, NetworkScene oldScene, IWriter writer, DirtyBits dirty)
        {
            UnityEngine.Debug.Log("diff serialize " + newScene.ObjectsCount);
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

    public class NetworkGameSceneParser : IParser<NetworkScene>
    {
        NetworkGameObjectParser _go = new NetworkGameObjectParser();

        public NetworkScene Parse(IReader reader)
        {
            var obj = new NetworkScene();
            var c = reader.ReadInt32();
            UnityEngine.Debug.Log("initial parse " + c);
            for(var i = 0; i < c; i++)
            {
                var go = _go.Parse(reader);
                obj.AddObject(go);
            }
            return obj;
        }

        public int GetDirtyBitsSize(NetworkScene obj)
        {
            return 0;
        }

        public NetworkScene Parse(NetworkScene scene, IReader reader, DirtyBits dirty)
        {
            var c = reader.ReadInt32();
            UnityEngine.Debug.Log("diff parse " + c);
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
