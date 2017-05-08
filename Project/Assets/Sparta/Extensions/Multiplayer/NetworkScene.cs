using System.Collections.Generic;
using System;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public class NetworkScene : IEquatable<NetworkScene>, ICloneable, INetworkBehaviourProvider
    {
        Dictionary<int, NetworkGameObject> _objects = new Dictionary<int, NetworkGameObject>();
        Dictionary<int, NetworkGameObject> _syncObjects = new Dictionary<int, NetworkGameObject>();

        public int FreeObjectId{ get; private set; }

        public Action<NetworkGameObject> OnObjectAdded;
        public Action<NetworkGameObject> OnObjectRemoved;

        public int ObjectsCount
        {
            get
            {
                return _objects.Count;
            }
        }

        public int SyncObjectsCount
        {
            get
            {
                return _syncObjects.Count;
            }
        }

        public INetworkBehaviourContainer Behaviours{ get; protected set; }

        const int InitialObjectId = 1;

        public NetworkScene()
        {
            _objects.Clear();
            _syncObjects.Clear();

            FreeObjectId = InitialObjectId;
        }

        public virtual void Copy(NetworkScene other, NetworkGameObject.PairOperation customObjectCopy = null)
        {
            {
                var itr = other._syncObjects.GetEnumerator();
                while(itr.MoveNext())
                {
                    var go = itr.Current.Value;
                    var myGo = FindObject(go.Id);
                    if(myGo == null)
                    {
                        //New object
                        AddObject((NetworkGameObject)go.Clone());
                    }
                    else
                    {
                        //For existing objects copy data (keep references)
                        if(customObjectCopy == null)
                        {
                            myGo.Copy(go);
                        }
                        else
                        {
                            customObjectCopy(go, myGo);
                        }
                    }
                }
                itr.Dispose();
            }
            {
                // remove objects that are not present
                var tmp = ObjectPool.Get<List<NetworkGameObject>>();
                var itr = GetSyncObjectEnumerator(tmp);
                while(itr.MoveNext())
                {
                    var id = itr.Current.Id;
                    if(other.FindObject(id) == null)
                    {
                        RemoveObject(id);
                    }
                }
                itr.Dispose();
                ObjectPool.Return(tmp);
            }
        }

        public virtual void DeepCopy(NetworkScene other)
        {
            {
                var itr = other._syncObjects.GetEnumerator();
                while(itr.MoveNext())
                {
                    var go = itr.Current.Value;
                    var myGo = FindObject(go.Id);
                    if(myGo == null)
                    {
                        //New object
                        AddObject((NetworkGameObject)go.DeepClone());
                    }
                    else
                    {
                        myGo.DeepCopy(go);
                    }
                }
                itr.Dispose();
            }
            {
                // remove objects that are not present
                var tmp = ObjectPool.Get<List<NetworkGameObject>>();
                var itr = GetSyncObjectEnumerator(tmp);
                while(itr.MoveNext())
                {
                    var id = itr.Current.Id;
                    if(other.FindObject(id) == null)
                    {
                        RemoveObject(id);
                    }
                }
                itr.Dispose();
                ObjectPool.Return(tmp);
            }
        }

        protected void AddClonedObjectsFromScene(NetworkScene other)
        {
            var tmp = ObjectPool.Get<List<NetworkGameObject>>();
            var itr = other.GetObjectEnumerator(tmp);
            while(itr.MoveNext())
            {
                AddObject((NetworkGameObject)itr.Current.Clone());
            }
            itr.Dispose();
            ObjectPool.Return(tmp);
        }

        public virtual Object Clone()
        {
            var scene = ObjectPool.Get<NetworkScene>();
            scene.AddClonedObjectsFromScene(this);
            return scene;
        }

        protected void AddDeeplyClonedObjectsFromScene(NetworkScene other)
        {
            var tmp = ObjectPool.Get<List<NetworkGameObject>>();
            var itr = other.GetObjectEnumerator(tmp);
            while(itr.MoveNext())
            {
                AddObject((NetworkGameObject)itr.Current.DeepClone());
            }
            itr.Dispose();
            ObjectPool.Return(tmp);
        }

        public virtual Object DeepClone()
        {
            var scene = ObjectPool.Get<NetworkScene>();
            scene.AddDeeplyClonedObjectsFromScene(this);
            return scene;
        }

        public virtual void Dispose()
        {
            OnObjectAdded = null;
            OnObjectRemoved = null;

            var objectsEnum = _objects.GetEnumerator();
            while(objectsEnum.MoveNext())
            {
                objectsEnum.Current.Value.Dispose();
            }
            objectsEnum.Dispose();

            Clear();

            ObjectPool.Return(this);
        }

        public void Clear()
        {            
            _objects.Clear();
            _syncObjects.Clear();
            FreeObjectId = InitialObjectId;
        }

        public int ProvideObjectId()
        {
            return ++FreeObjectId;
        }

        public virtual void AddObject(NetworkGameObject obj)
        {
            _objects.Add(obj.Id, obj);
            if(!obj.Local)
            {
                _syncObjects.Add(obj.Id, obj);
            }
            if(FreeObjectId <= obj.UniqueId)
            {
                FreeObjectId = obj.UniqueId + 1;
            }
            if(OnObjectAdded != null)
            {
                OnObjectAdded(obj);
            }
        }

        public virtual bool RemoveObject(int id)
        {
            var go = FindObject(id);
            if(go != null)
            {
                _objects.Remove(id);
                _syncObjects.Remove(id);
                go.Invalidate();
                if(OnObjectRemoved != null)
                {
                    OnObjectRemoved(go);
                }
                return true;
            }
            //TODO: Find a way to reuse ids?
            return false;
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

        public List<NetworkGameObject>.Enumerator GetObjectEnumerator(List<NetworkGameObject> result)
        {
            result.Clear();
            result.AddRange(_objects.Values);
            return result.GetEnumerator();
        }

        public List<NetworkGameObject>.Enumerator GetSyncObjectEnumerator(List<NetworkGameObject> result)
        {
            result.Clear();
            result.AddRange(_syncObjects.Values);
            return result.GetEnumerator();
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

    public class NetworkScene<B> : NetworkScene where B : class, INetworkSceneBehaviour
    {
        public NetworkBehaviourContainer<B> TypedBehaviours{ get; protected set; }

        protected NetworkBehaviourContainerObserver<B> _behaviourObserver;

        public NetworkScene() : base()
        {
            TypedBehaviours = ObjectPool.Get<NetworkBehaviourContainer<B>>();
            Behaviours = TypedBehaviours;
            _behaviourObserver = ObjectPool.Get<NetworkBehaviourContainerObserver<B>>().Init(TypedBehaviours);
        }

        public override object Clone()
        {
            var scene = new NetworkScene<B>();
            scene.AddClonedObjectsFromScene(this);
            scene.Behaviours = Behaviours;
            scene.TypedBehaviours = TypedBehaviours;
            return scene;
        }

        public override object DeepClone()
        {
            var scene = new NetworkScene<B>();
            scene.AddDeeplyClonedObjectsFromScene(this);
            scene.TypedBehaviours = (NetworkBehaviourContainer<B>)TypedBehaviours.Clone();
            scene.Behaviours = scene.TypedBehaviours;
            return scene;
        }

        public override void DeepCopy(NetworkScene other)
        {
            base.DeepCopy(other);
            var bscene = other as NetworkScene<B>;
            if(bscene != null)
            {
                TypedBehaviours.Copy(bscene.TypedBehaviours);
            }
        }

        public void AddBehaviour(B behaviour)
        {
            TypedBehaviours.Add(behaviour);
        }

        public T GetBehaviour<T>(T behaviour) where T : class
        {
            return TypedBehaviours.Get<T>();
        }

        public void AddBehaviours(IEnumerable<B> behaviours)
        {
            TypedBehaviours.Add(behaviours);
        }

        public void Update(float dt)
        {
            var tmp = ObjectPool.Get<List<B>>();
            var itr = TypedBehaviours.GetEnumerator(tmp);
            while(itr.MoveNext())
            {
                itr.Current.Update(dt);
            }
            ObjectPool.Return(tmp);
            itr.Dispose();
        }

        public void UpdatePendingLogic()
        {
            for(var i = 0; i < _behaviourObserver.Added.Count; i++)
            {
                _behaviourObserver.Added[i].Scene = this;
            }
            for(var i = 0; i < _behaviourObserver.Added.Count; i++)
            {
                OnBehaviourAdded(_behaviourObserver.Added[i]);
            }
            for(var i = 0; i < _behaviourObserver.Removed.Count; i++)
            {
                OnBehaviourRemoved(_behaviourObserver.Removed[i]);
            }
            _behaviourObserver.Clear();
        }

        protected virtual void OnBehaviourAdded(B behaviour)
        {
            behaviour.OnStart();
        }

        protected virtual void OnBehaviourRemoved(B behaviour)
        {
            behaviour.OnDestroy();
        }

        public override void AddObject(NetworkGameObject go)
        {
            base.AddObject(go);
            var tmp = ObjectPool.Get<List<B>>();
            var itr = TypedBehaviours.GetEnumerator(tmp);
            while(itr.MoveNext())
            {
                itr.Current.OnInstantiateObject(go);
            }
            ObjectPool.Return(tmp);
            itr.Dispose();
        }

        public override bool RemoveObject(int objectId)
        {
            if(!base.RemoveObject(objectId))
            {
                return false;
            }
            var tmp = ObjectPool.Get<List<B>>();
            var itr = TypedBehaviours.GetEnumerator(tmp);
            while(itr.MoveNext())
            {
                itr.Current.OnDestroyObject(objectId);
            }
            ObjectPool.Return(tmp);
            itr.Dispose();
            return true;
        }
    }

    public interface INetworkSceneBehaviour
    {
        NetworkScene Scene { set; }

        void OnStart();

        void OnDestroy();

        void Update(float dt);

        void OnInstantiateObject(NetworkGameObject go);

        void OnDestroyObject(int id);
    }

    public class NetworkSceneSerializer : IDiffWriteSerializer<NetworkScene>
    {
        IDiffWriteSerializer<NetworkGameObject> _objectSerializer;

        public NetworkSceneSerializer(IDiffWriteSerializer<NetworkGameObject> objectSerializer = null)
        {
            if(objectSerializer == null)
            {
                objectSerializer = new NetworkGameObjectSerializer();
            }
            _objectSerializer = objectSerializer;
        }

        public void Compare(NetworkScene newScene, NetworkScene oldScene, Bitset dirty)
        {
        }

        public void Serialize(NetworkScene newScene, IWriter writer)
        {
            var tmp = ObjectPool.Get<List<NetworkGameObject>>();
            writer.Write(newScene.SyncObjectsCount);
            var itr = newScene.GetSyncObjectEnumerator(tmp);
            while(itr.MoveNext())
            {
                var go = itr.Current;
                _objectSerializer.Serialize(go, writer);
            }
            itr.Dispose();
            ObjectPool.Return(tmp);
        }

        public void Serialize(NetworkScene newScene, NetworkScene oldScene, IWriter writer, Bitset dirty)
        {
            var tmp = ObjectPool.Get<List<NetworkGameObject>>();
            
            writer.Write(newScene.SyncObjectsCount);
            var itr = newScene.GetSyncObjectEnumerator(tmp);
            while(itr.MoveNext())
            {
                var go = itr.Current;
                writer.Write(go.Id);
                var oldGo = oldScene.FindObject(go.Id);
                if(oldGo == null)
                {
                    _objectSerializer.Serialize(go, writer);
                }
                else
                {
                    _objectSerializer.Serialize(go, oldGo, writer);
                }
            }
            itr.Dispose();

            tmp.Clear();
            itr = oldScene.GetSyncObjectEnumerator(tmp);
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

            ObjectPool.Return(tmp);
        }
    }

    public class NetworkSceneSerializer<Behaviour> : IDiffWriteSerializer<NetworkScene<Behaviour>> where Behaviour : class, INetworkSceneBehaviour
    {
        NetworkSceneSerializer _sceneSerializer;
        NetworkBehaviourContainerSerializer<Behaviour> _behaviourSerializer;
        NetworkGameObjectSerializer<INetworkBehaviour> _objectSerializer;

        public NetworkSceneSerializer(NetworkGameObjectSerializer<INetworkBehaviour> objectSerializer = null)
        {
            _objectSerializer = objectSerializer ?? new NetworkGameObjectSerializer<INetworkBehaviour>();
            _sceneSerializer = new NetworkSceneSerializer(_objectSerializer);
            _behaviourSerializer = new NetworkBehaviourContainerSerializer<Behaviour>();
        }

        public void RegisterSceneBehaviour<T>(byte type, IDiffWriteSerializer<T> serializer) where T : Behaviour
        {
            _behaviourSerializer.Register(type, serializer);
        }

        public void RegisterObjectBehaviour<T>(byte type, IDiffWriteSerializer<T> parser) where T : INetworkBehaviour
        {
            _objectSerializer.RegisterBehaviour(type, parser);
        }

        public void Compare(NetworkScene<Behaviour> newObj, NetworkScene<Behaviour> oldObj, Bitset dirty)
        {
        }

        public void Serialize(NetworkScene<Behaviour> newObj, IWriter writer)
        {
            _sceneSerializer.Serialize(newObj, writer);
            _behaviourSerializer.Serialize(newObj.TypedBehaviours, writer);
        }

        public void Serialize(NetworkScene<Behaviour> newObj, NetworkScene<Behaviour> oldObj, IWriter writer, Bitset dirty)
        {
            _sceneSerializer.Serialize(newObj, oldObj, writer);
            _behaviourSerializer.Serialize(newObj.TypedBehaviours, oldObj.TypedBehaviours, writer);
        }
    }

    public class NetworkSceneParser : IDiffReadParser<NetworkScene>
    {
        Func<NetworkScene> _factory;
        IDiffReadParser<NetworkGameObject> _objectParser;

        public NetworkSceneParser(IDiffReadParser<NetworkGameObject> objectParser = null, Func<NetworkScene> factory = null)
        {
            if(objectParser == null)
            {
                objectParser = new NetworkGameObjectParser();
            }
            _factory = factory;
            _objectParser = objectParser;
        }

        public int GetDirtyBitsSize(NetworkScene obj)
        {
            return 0;
        }

        public NetworkScene Parse(IReader reader)
        {
            var obj = _factory == null ? new NetworkScene() : _factory();
            var c = reader.ReadInt32();
            for(var i = 0; i < c; i++)
            {
                var go = _objectParser.Parse(reader);
                obj.AddObject(go);
            }
            return obj;
        }

        public NetworkScene Parse(NetworkScene scene, IReader reader, Bitset dirty)
        {
            var c = reader.ReadInt32();
            for(var i = 0; i < c; i++)
            {
                var id = reader.ReadInt32();
                var go = scene.FindObject(id);
                if(go == null)
                {
                    go = _objectParser.Parse(reader);
                    scene.AddObject(go);
                }
                else
                {
                    _objectParser.Parse(go, reader);
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

    public class NetworkSceneParser<Behaviour> : IDiffReadParser<NetworkScene<Behaviour>> where Behaviour : class, INetworkSceneBehaviour
    {
        readonly NetworkSceneParser _sceneParser;
        readonly NetworkBehaviourContainerParser<Behaviour> _behaviourParser;
        readonly NetworkGameObjectParser<INetworkBehaviour> _objectParser;

        public NetworkSceneParser(NetworkGameObjectParser<INetworkBehaviour> objectParser = null)
        {
            _objectParser = objectParser ?? new NetworkGameObjectParser<INetworkBehaviour>(CreateObject);
            _sceneParser = new NetworkSceneParser(_objectParser, CreateScene);
            _behaviourParser = new NetworkBehaviourContainerParser<Behaviour>();
        }

        public void RegisterSceneBehaviour<T>(byte type, IDiffReadParser<T> parser) where T : Behaviour
        {
            _behaviourParser.Register(type, parser);
        }

        public void RegisterObjectBehaviour<T>(byte type, IDiffReadParser<T> parser) where T : INetworkBehaviour
        {
            _objectParser.RegisterBehaviour(type, parser);
        }

        NetworkScene CreateScene()
        {
            return new NetworkScene<Behaviour>();
        }

        NetworkGameObject<INetworkBehaviour> CreateObject(int objId, byte objType)
        {
            var obj = ObjectPool.Get<NetworkGameObject<INetworkBehaviour>>();
            obj.Init(objId, false, null, objType);
            return obj;
        }

        public NetworkScene<Behaviour> Parse(IReader reader)
        {
            var scene = _sceneParser.Parse(reader) as NetworkScene<Behaviour>;
            scene.TypedBehaviours.Copy(_behaviourParser.Parse(reader));
            return scene;
        }

        public int GetDirtyBitsSize(NetworkScene<Behaviour> obj)
        {
            return 0;
        }

        public NetworkScene<Behaviour> Parse(NetworkScene<Behaviour> obj, IReader reader, Bitset dirty)
        {
            _sceneParser.Parse(obj, reader);
            _behaviourParser.Parse(obj.TypedBehaviours, reader);
            return obj;
        }
    }
}
