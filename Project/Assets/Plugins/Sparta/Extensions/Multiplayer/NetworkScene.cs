using System.Collections.Generic;
using System;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public class SyncGroup
    {
        public SyncGroupSettings Settings;

        readonly Dictionary<int, NetworkGameObject> _objects = new Dictionary<int, NetworkGameObject>();

        public float TimeSinceLastSync { get; private set; }

        public bool CanSync
        { 
            get
            {
                return TimeSinceLastSync >= Settings.SyncInterval;
            }
        }

        public void AddObject(NetworkGameObject obj)
        {
            if(!_objects.ContainsKey(obj.Id))
            {
                _objects.Add(obj.Id, obj);
            }
        }

        public bool RemoveObject(int objId)
        {
            return _objects.Remove(objId);
        }

        public void Update(float dt)
        {
            TimeSinceLastSync += dt;
        }

        public void ResetTimer()
        {
            TimeSinceLastSync = 0f;
        }
    }

    public class NetworkScene : IEquatable<NetworkScene>, ICloneable, INetworkBehaviourProvider, IDisposable
    {
        NetworkSceneContext _context = null;

        public NetworkSceneContext Context
        {
            get
            {
                SocialPoint.Base.DebugUtils.Assert(_context != null);
                return _context;
            }
            set
            {
                _context = value;
            }
        }

        public Dictionary<int, NetworkGameObject> SyncObjects = new Dictionary<int, NetworkGameObject>();

        Dictionary<int, NetworkGameObject> _objects = new Dictionary<int, NetworkGameObject>();

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
                return SyncObjects.Count;
            }
        }

        public INetworkBehaviourContainer Behaviours{ get; protected set; }

        public NetworkBehaviourContainer<INetworkSceneBehaviour> TypedBehaviours{ get; protected set; }

        protected NetworkBehaviourContainerObserver<INetworkSceneBehaviour> _behaviourObserver;

        const int InitialObjectId = 1;

        public NetworkScene(NetworkSceneContext context)
        {
            Context = context;
            _objects.Clear();
            SyncObjects.Clear();

            FreeObjectId = InitialObjectId;

            TypedBehaviours = Context.Pool.Get<NetworkBehaviourContainer<INetworkSceneBehaviour>>();
            Behaviours = TypedBehaviours;
            _behaviourObserver = Context.Pool.Get<NetworkBehaviourContainerObserver<INetworkSceneBehaviour>>().Init(TypedBehaviours);
        }

        public virtual void Copy(NetworkScene other, NetworkGameObject.PairOperation newObjectCopy = null, NetworkGameObject.PairOperation customObjectCopy = null)
        {
            Context = other.Context;
            {
                var tmp = Context.Pool.Get<List<NetworkGameObject>>();
                var itr = other.GetSyncObjectEnumerator(tmp);
                while(itr.MoveNext())
                {
                    var go = itr.Current;
                    var myGo = FindObject(go.Id);
                    if(myGo == null)
                    {
                        //New object
                        var myNewGo = (NetworkGameObject)go.Clone();
                        AddObject(myNewGo);
                        if(newObjectCopy != null)
                        {
                            newObjectCopy(go, myNewGo);
                        }
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
                Context.Pool.Return(tmp);
            }
            {
                // remove objects that are not present
                var tmp = Context.Pool.Get<List<NetworkGameObject>>();
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
                Context.Pool.Return(tmp);
            }
        }

        public virtual void DeepCopy(NetworkScene other)
        {
            Context = other.Context;
            {
                var tmp = Context.Pool.Get<List<NetworkGameObject>>();
                var itr = other.GetSyncObjectEnumerator(tmp);
                while(itr.MoveNext())
                {
                    var go = itr.Current;

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
                Context.Pool.Return(tmp);
            }
            {
                // remove objects that are not present
                var tmp = Context.Pool.Get<List<NetworkGameObject>>();
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
                Context.Pool.Return(tmp);
            }

            TypedBehaviours.Copy(other.TypedBehaviours);
        }

        protected void AddClonedObjectsFromScene(NetworkScene other)
        {
            var tmp = Context.Pool.Get<List<NetworkGameObject>>();
            var itr = other.GetObjectEnumerator(tmp);
            while(itr.MoveNext())
            {
                AddObject((NetworkGameObject)itr.Current.Clone());
            }
            itr.Dispose();
            Context.Pool.Return(tmp);
        }

        public virtual Object Clone()
        {
            var scene = new NetworkScene(Context);
            scene.Context = Context;
            scene.AddClonedObjectsFromScene(this);
            scene.Behaviours = Behaviours;
            scene.TypedBehaviours = TypedBehaviours;
            return scene;
        }

        protected void AddDeeplyClonedObjectsFromScene(NetworkScene other)
        {
            var tmp = Context.Pool.Get<List<NetworkGameObject>>();
            var itr = other.GetObjectEnumerator(tmp);
            while(itr.MoveNext())
            {
                AddObject((NetworkGameObject)itr.Current.DeepClone());
            }
            itr.Dispose();
            Context.Pool.Return(tmp);
        }

        public virtual Object DeepClone()
        {
            var scene = new NetworkScene(Context);
            scene.Context = Context;
            scene.AddDeeplyClonedObjectsFromScene(this);
            scene.TypedBehaviours = (NetworkBehaviourContainer<INetworkSceneBehaviour>)TypedBehaviours.Clone();
            scene.Behaviours = scene.TypedBehaviours;
            return scene;
        }

        public virtual void Dispose()
        {
            var objectsEnum = _objects.GetEnumerator();
            while(objectsEnum.MoveNext())
            {
                if(objectsEnum.Current.Value == null)
                {
                    continue;
                }
                objectsEnum.Current.Value.Dispose();
            }
            objectsEnum.Dispose();

            if(TypedBehaviours != null)
            {
                TypedBehaviours.Dispose();
                TypedBehaviours = null;
            }

            if(Behaviours != null)
            {
                Behaviours.Dispose();
                Behaviours = null;
            }

            OnObjectAdded = null;
            OnObjectRemoved = null;

            _behaviourObserver.Clear();

            Clear();

            Context.Pool.Return(this);
        }

        public void Clear()
        {            
            _objects.Clear();
            SyncObjects.Clear();
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
                SyncObjects.Add(obj.Id, obj);
            }
            if(FreeObjectId <= obj.UniqueId)
            {
                FreeObjectId = obj.UniqueId + 1;
            }
            if(OnObjectAdded != null)
            {
                OnObjectAdded(obj);
            }

            var tmp = Context.Pool.Get<List<INetworkSceneBehaviour>>();
            var itr = TypedBehaviours.GetEnumerator(tmp);
            while(itr.MoveNext())
            {
                var current = itr.Current;
                if(current == null)
                {
                    continue;
                }

                current.OnInstantiateObject(obj);
            }
            Context.Pool.Return(tmp);

            itr.Dispose();
        }

        public virtual bool RemoveObject(int id)
        {
            return RemoveObject(FindObject(id));
        }

        bool RemoveObject(NetworkGameObject go)
        {
            if(go == null)
            {
                return false;
            }

            if(!_objects.Remove(go.Id))
            {
                return false;
            }

            SyncObjects.Remove(go.Id);
            go.Invalidate();

            if(OnObjectRemoved != null)
            {
                OnObjectRemoved(go);
            }

            var tmp = Context.Pool.Get<List<INetworkSceneBehaviour>>();
            var itr = TypedBehaviours.GetEnumerator(tmp);
            while(itr.MoveNext())
            {
                var current = itr.Current;
                if(current == null)
                {
                    continue;
                }
                current.OnDestroyObject(go.Id);
            }
            itr.Dispose();
            Context.Pool.Return(tmp);

            return true;
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

        public Dictionary<int, NetworkGameObject>.ValueCollection.Enumerator GetObjectEnumerator()
        {
            return _objects.Values.GetEnumerator();
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
            result.AddRange(SyncObjects.Values);
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

        public void AddBehaviour(INetworkSceneBehaviour behaviour)
        {
            TypedBehaviours.Add(behaviour);
        }

        public T GetBehaviour<T>(T behaviour) where T : class
        {
            return TypedBehaviours.Get<T>();
        }

        public void AddBehaviours(IEnumerable<INetworkSceneBehaviour> behaviours)
        {
            TypedBehaviours.Add(behaviours);
        }

        public void Update(float dt)
        {
            var tmp = Context.Pool.Get<List<INetworkSceneBehaviour>>();
            var itr = TypedBehaviours.GetEnumerator(tmp);
            while(itr.MoveNext())
            {
                if(itr.Current == null)
                {
                    continue;
                }
                itr.Current.Update(dt);
            }
            Context.Pool.Return(tmp);
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

        protected virtual void OnBehaviourAdded(INetworkSceneBehaviour behaviour)
        {
            behaviour.OnStart();
        }

        protected virtual void OnBehaviourRemoved(INetworkSceneBehaviour behaviour)
        {
            behaviour.OnDestroy();
        }
    }

    public interface INetworkSceneBehaviour : IDisposable, IDeltaUpdateable
    {
        NetworkScene Scene { set; }

        void OnStart();

        void OnDestroy();

        void OnInstantiateObject(NetworkGameObject go);

        void OnDestroyObject(int id);
    }

    public class NetworkSceneSerializer : IDiffWriteSerializer<NetworkScene>
    {
        NetworkSceneContext _context = null;

        public NetworkSceneContext Context
        {
            get
            {
                SocialPoint.Base.DebugUtils.Assert(_context != null);
                return _context;
            }
            set
            {
                _context = value;
            }
        }

        readonly NetworkBehaviourContainerSerializer<INetworkSceneBehaviour> _behaviourSerializer;
        readonly NetworkGameObjectSerializer _objectSerializer;

        public NetworkSceneSerializer(NetworkSceneContext context, NetworkGameObjectSerializer objectSerializer = null)
        {
            Context = context;
            _objectSerializer = objectSerializer ?? new NetworkGameObjectSerializer(context);
            _behaviourSerializer = new NetworkBehaviourContainerSerializer<INetworkSceneBehaviour>();
        }

        public void RegisterSceneBehaviour<T>(byte type, IDiffWriteSerializer<T> serializer) where T : INetworkSceneBehaviour
        {
            _behaviourSerializer.Register(type, serializer);
        }

        public void RegisterObjectBehaviour<T>(byte type, IDiffWriteSerializer<T> parser) where T : INetworkBehaviour
        {
            _objectSerializer.RegisterBehaviour(type, parser);
        }

        public void Compare(NetworkScene newObj, NetworkScene oldObj, Bitset dirty)
        {
        }

        public void Serialize(NetworkScene newScene, IWriter writer)
        {
            var tmp = newScene.Context.Pool.Get<List<NetworkGameObject>>();
            writer.Write(newScene.SyncObjectsCount);
            var itr = newScene.GetSyncObjectEnumerator(tmp);
            while(itr.MoveNext())
            {
                var go = itr.Current;
                _objectSerializer.Serialize(go, writer);
            }
            itr.Dispose();
            newScene.Context.Pool.Return(tmp);

            _behaviourSerializer.Serialize(newScene.TypedBehaviours, writer);
        }

        public void Serialize(NetworkScene newScene, NetworkScene oldScene, IWriter writer, Bitset dirty)
        {
            var objectsToCreate = newScene.Context.Pool.Get<List<NetworkGameObject>>();
            var objectsToUpdateNew = newScene.Context.Pool.Get<List<NetworkGameObject>>();
            var objectsToUpdateOld = newScene.Context.Pool.Get<List<NetworkGameObject>>();
            var objectsToRemove = newScene.Context.Pool.Get<List<NetworkGameObject>>();

            objectsToCreate.Clear();
            objectsToUpdateNew.Clear();
            objectsToUpdateOld.Clear();
            objectsToRemove.Clear();

            foreach(var go in newScene.SyncObjects.Values)
            {
                var oldGo = oldScene.FindObject(go.Id);
                if(oldGo == null)
                {
                    objectsToCreate.Add(go);
                }
                else
                {
                    objectsToUpdateNew.Add(go);
                    objectsToUpdateOld.Add(oldGo);
                }
            }
            foreach(var oldGo in oldScene.SyncObjects.Values)
            {
                if(!objectsToUpdateNew.Exists(go => go.Id == oldGo.Id))
                {
                    objectsToRemove.Add(oldGo);
                }
            }

            writer.Write(objectsToCreate.Count);
            foreach(var go in objectsToCreate)
            {
                _objectSerializer.Serialize(go, writer);
            }

            writer.Write(objectsToUpdateNew.Count);
            var i = 0;
            foreach(var go in objectsToUpdateNew)
            {
                writer.Write(go.Id);
                _objectSerializer.Serialize(go, objectsToUpdateOld[i++], writer);
            }

            writer.Write(objectsToRemove.Count);
            foreach(var go in objectsToRemove)
            {
                writer.Write(go.Id);
            }

            newScene.Context.Pool.Return<List<NetworkGameObject>>(objectsToCreate);
            newScene.Context.Pool.Return<List<NetworkGameObject>>(objectsToUpdateNew);
            newScene.Context.Pool.Return<List<NetworkGameObject>>(objectsToUpdateOld);
            newScene.Context.Pool.Return<List<NetworkGameObject>>(objectsToRemove);

            _behaviourSerializer.Serialize(newScene.TypedBehaviours, oldScene.TypedBehaviours, writer);
        }
    }
        
    public class NetworkSceneParser : IDiffReadParser<NetworkScene>
    {
        NetworkSceneContext _context = null;

        NetworkGameObject _fakeGameObject;

        public NetworkSceneContext Context
        {
            get
            {
                SocialPoint.Base.DebugUtils.Assert(_context != null);
                return _context;
            }
            set
            {
                _context = value;
            }
        }
            
        readonly NetworkBehaviourContainerParser<INetworkSceneBehaviour> _behaviourParser;
        readonly NetworkGameObjectParser _objectParser;

        public NetworkSceneParser(NetworkSceneContext context, NetworkGameObjectParser objectParser = null, Action<Exception> handleException = null)
        {
            Context = context;
            _objectParser = objectParser ?? new NetworkGameObjectParser(Context, CreateObject, handleException);
            _behaviourParser = new NetworkBehaviourContainerParser<INetworkSceneBehaviour>(handleException);
            _fakeGameObject = CreateObject(1, 0);
        }
    
        public void RegisterSceneBehaviour<T>(byte type, IDiffReadParser<T> parser) where T : INetworkSceneBehaviour
        {
            _behaviourParser.Register(type, parser);
        }

        public void RegisterObjectBehaviour<T>(byte type, IDiffReadParser<T> parser) where T : INetworkBehaviour
        {
            _objectParser.RegisterBehaviour(type, parser);
        }

        NetworkScene CreateScene()
        {
            return new NetworkScene(Context);
        }

        NetworkGameObject CreateObject(int objId, byte objType)
        {
            var obj = Context.Pool.Get<NetworkGameObject>();
            obj.Init(Context, objId, false, null, objType);
            return obj;
        }

        public int GetDirtyBitsSize(NetworkScene obj)
        {
            return 0;
        }

        public NetworkScene Parse(IReader reader)
        {
            NetworkScene scene = new NetworkScene(Context);

            var c = reader.ReadInt32();
            for(var i = 0; i < c; i++)
            {
                var go = _objectParser.Parse(reader);
                scene.AddObject(go);
            }

            scene.Context = Context;
            scene.TypedBehaviours.Copy(_behaviourParser.Parse(reader));
            return scene;
        }

        public NetworkScene Parse(NetworkScene scene, IReader reader, Bitset dirty)
        {
            var objectsToCreateCount = reader.ReadInt32();
            for(var i = 0; i < objectsToCreateCount; i++)
            {
                var go = _objectParser.Parse(reader);
                var oldGo = scene.FindObject(go.Id);
                if(oldGo != null)
                {
                    Base.Log.w("Trying to add game object " + go.Id + " which was already in the scene. Replacing the old game object by the new one.");
                    scene.RemoveObject(oldGo.Id);
                }
                scene.AddObject(go);
            }

            var objectsToUpdateCount = reader.ReadInt32();
            for(var i = 0; i < objectsToUpdateCount; i++)
            {
                var id = reader.ReadInt32();
                var go = scene.FindObject(id);
                if(go == null)
                {
                    Base.Log.w("Trying to update game object " + id + " not present in the scene. Ignoring update data.");
                    // Parse object to skipp its update data.
                    _objectParser.Parse(_fakeGameObject, reader);
                    continue;
                }
                _objectParser.Parse(go, reader);
            }

            var objectsToRemoveCount = reader.ReadInt32();
            for(var i = 0; i < objectsToRemoveCount; i++)
            {
                var id = reader.ReadInt32();
                scene.RemoveObject(id);
            }

            _behaviourParser.Parse(scene.TypedBehaviours, reader);

            scene.Context = Context;

            return scene;
        }
    }
}
