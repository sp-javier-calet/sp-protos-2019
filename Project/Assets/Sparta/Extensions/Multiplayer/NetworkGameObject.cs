using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Utils;
using System;
using System.Collections.Generic;

public class FinderSettings
{
    public static readonly int DefaultListCapacity = 20;
}

namespace SocialPoint.Multiplayer
{
    public interface INetworkBehaviour : ICloneable, IDisposable, IDeltaUpdateable
    {
        NetworkGameObject GameObject { set; }

        void OnAwake();

        void OnStart();

        void OnDestroy();
    }

    public class NetworkGameObject : IEquatable<NetworkGameObject>, ICloneable, INetworkBehaviourProvider
    {
        public delegate void PairOperation(NetworkGameObject go1, NetworkGameObject go2);

        public Action<NetworkGameObject> SyncGroupChanged;

        int _syncGroup = 0;

        public int SyncGroup
        {
            get
            {
                return _syncGroup;
            }
            set
            {
                var changed = value != _syncGroup;
                _syncGroup = value;

                if(changed && SyncGroupChanged != null)
                {
                    SyncGroupChanged(this);
                }
            }
        }

        public bool Invalid { get; protected set; }

        public bool Untargeteable { get; set; }

        public byte Type{ get; protected set; }

        public int Id{ get; protected set; }

        public Transform Transform{ get; protected set; }

        public INetworkBehaviourContainer Behaviours{ get; protected set; }

        public bool Local{ get; protected set; }

        public bool IsServerGameObject { get; protected set; }

        public int UniqueId
        {
            get
            {
                return Local ? -1 * Id : Id;
            }
        }

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

        public static bool IsNullOrInvalid(NetworkGameObject go)
        {
            return (go == null || go.Invalid);
        }

        public NetworkBehaviourContainer<INetworkBehaviour> TypedBehaviours{ get; protected set; }

        protected NetworkBehaviourContainerObserver<INetworkBehaviour> _behaviourObserver;

        public NetworkGameObject()
        {
            TypedBehaviours = new NetworkBehaviourContainer<INetworkBehaviour>();
            Behaviours = TypedBehaviours;
            _behaviourObserver = new NetworkBehaviourContainerObserver<INetworkBehaviour>().Init(TypedBehaviours);
        }

        public NetworkGameObject(NetworkSceneContext context)
        {
            Context = context;
            TypedBehaviours = Context.Pool.Get<NetworkBehaviourContainer<INetworkBehaviour>>();
            Behaviours = TypedBehaviours;
            _behaviourObserver = Context.Pool.Get<NetworkBehaviourContainerObserver<INetworkBehaviour>>().Init(TypedBehaviours);
        }

        public NetworkGameObject Init(NetworkSceneContext context, int id, bool isServerGameObject = false, Transform transform = null, byte type = 0, bool local = false, int syncGroup = 0)
        {
            Untargeteable = true;
            
            Context = context;

            SyncGroupChanged = null;

            SyncGroup = syncGroup;

            IsServerGameObject = isServerGameObject;
            if(id <= 0)
            {
                throw new ArgumentException("NetworkGameObject id needs to be positive.");
            }
            Type = type;
            Local = local;
            Id = local ? -id : id;
            if(transform == null)
            {
                transform = Context.Pool.Get<Transform>();
            }
            else
            {
                transform = (Transform)transform.Clone(Context.Pool);
            }
            Transform = transform;
            Behaviours = TypedBehaviours;

            return this;
        }

        public NetworkGameObject Init(NetworkSceneContext context,
                                      int id,
                                      bool isServerGameObject, 
                                      Transform transform,
                                      byte type,
                                      bool local, 
                                      NetworkBehaviourContainer<INetworkBehaviour> typedBehaviours, 
                                      NetworkBehaviourContainerObserver<INetworkBehaviour> behaviourObserver = null,
                                      int syncGroup = 0)
        {
            Init(context, id, isServerGameObject, transform, type, local, syncGroup);

            Behaviours = TypedBehaviours = typedBehaviours;
            _behaviourObserver = behaviourObserver ?? Context.Pool.Get<NetworkBehaviourContainerObserver<INetworkBehaviour>>().Init(typedBehaviours);
            return this;
        }

        public virtual void Dispose()
        {
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

            if(Transform != null)
            {
                Transform.Dispose();
                Context.Pool.Return(Transform);
                Transform = null;
            }

            if(_behaviourObserver != null)
            {
                _behaviourObserver.Dispose();
                _behaviourObserver = null;
            }

            Invalid = false;
            Type = 0;
            Id = 0;
            Local = false;
            _syncGroup = 0;
            IsServerGameObject = false;

            //Nando - NetworkScene<B> refactor: Commented because it is not working properly. Pooling wasn't working before this refactor so it won't affect performance
            //Context.Pool.Return(this);
        }

        public void Invalidate()
        {
            Invalid = true;
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
            hash = CryptographyUtils.HashCombine(hash, Behaviours.GetHashCode());
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
            return string.Format("[NetworkGameObject:{0}({1}) {2}]", Id, Type, Transform);
        }

        public virtual object Clone()
        {
            var other = Context.Pool.Get<NetworkGameObject>();
            other.Init(Context, UniqueId, IsServerGameObject, Transform, Type, Local, TypedBehaviours, _behaviourObserver, SyncGroup);
            return other;
        }

        public virtual object DeepClone()
        {
            var typedBehaviours = (NetworkBehaviourContainer<INetworkBehaviour>)TypedBehaviours.Clone();
            var other = Context.Pool.Get<NetworkGameObject>();
            other.Init(Context, UniqueId, IsServerGameObject, Transform, Type, Local, typedBehaviours, null, SyncGroup);
            return other;
        }


        public virtual void Copy(NetworkGameObject other)
        {
            Context = other.Context;
            Id = other.Id;
            Type = other.Type;
            Local = other.Local;
            _syncGroup = other.SyncGroup;
            Transform.Copy(other.Transform);
            Behaviours = other.Behaviours;
        }

        public virtual void DeepCopy(NetworkGameObject other)
        {
            Copy(other);

            if(other != null)
            {
                TypedBehaviours.Copy(other.TypedBehaviours);
            }
        }

        public INetworkBehaviour AddBehaviour(INetworkBehaviour behaviour, Type type)
        {
            TypedBehaviours.Add(behaviour, type);
            return behaviour;
        }

        public void AddBehaviours(List<INetworkBehaviour> bs, List<Type> types)
        {
            if(bs == null)
            {
                return;
            }
            var itr = bs.GetEnumerator();
            var itrType = types.GetEnumerator();
            while(itr.MoveNext())
            {
                itrType.MoveNext();

                if(itr.Current == null)
                {
                    continue;
                }

                AddBehaviour(itr.Current, itrType.Current);
            }
            itr.Dispose();
            itrType.Dispose();
        }

        public void AddClonedBehaviours(List<INetworkBehaviour> bs, List<Type> types)
        {
            var itr = bs.GetEnumerator();
            var itrType = types.GetEnumerator();
            while(itr.MoveNext())
            {
                itrType.MoveNext();

                if(itr.Current == null)
                {
                    continue;
                }

                var proto = itr.Current as ICloneable;
                if(proto == null)
                {
                    throw new ArgumentException(string.Format("Class {0} is not ICloneable.", itr.Current.GetType()));
                }
                AddBehaviour((INetworkBehaviour)proto.Clone(), itrType.Current);
            }
            itr.Dispose();
            itrType.Dispose();
        }

        public virtual void OnDestroy()
        {
            var tmp = Context.Pool.Get<List<INetworkBehaviour>>();
            var itr = TypedBehaviours.GetEnumerator(tmp);
            while(itr.MoveNext())
            {
                if(itr.Current == null)
                {
                    continue;
                }
                itr.Current.OnDestroy();
            }
            Context.Pool.Return(tmp);
            itr.Dispose();
        }

        public virtual void Update(float dt)
        {
            var tmp = Context.Pool.Get<List<INetworkBehaviour>>();
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
                var behaviour = _behaviourObserver.Added[i];
                behaviour.GameObject = this;
                behaviour.OnAwake();
                Untargeteable = false;
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

        protected virtual void OnBehaviourAdded(INetworkBehaviour behaviour)
        {
            behaviour.OnStart();
        }

        protected virtual void OnBehaviourRemoved(INetworkBehaviour behaviour)
        {
            behaviour.OnDestroy();
        }
    }

    public class NetworkGameObjectSerializer : IDiffWriteSerializer<NetworkGameObject>
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

        readonly NetworkBehaviourContainerSerializer<INetworkBehaviour> _behaviourSerializer;

        public NetworkGameObjectSerializer(NetworkSceneContext context)
        {
            Context = context;
            _behaviourSerializer = new NetworkBehaviourContainerSerializer<INetworkBehaviour>();
        }

        public void Compare(NetworkGameObject newObj, NetworkGameObject oldObj, Bitset dirty)
        {
            dirty.Set(newObj.Transform != oldObj.Transform);
        }

        public void RegisterBehaviour<T>(byte type, IDiffWriteSerializer<T> serializer) where T : INetworkBehaviour
        {
            _behaviourSerializer.Register(type, serializer);
        }

        public void Serialize(NetworkGameObject newObj, IWriter writer)
        {
            writer.Write(newObj.Id);
            writer.Write(newObj.Type);
            TransformShortSerializer.Instance.Serialize(newObj.Transform, writer);

            if(newObj != null)
            {
                writer.Write(true);
                _behaviourSerializer.Serialize(newObj.TypedBehaviours, writer);
            }
            else
            {
                writer.Write(false);
            }
        }

        public void Serialize(NetworkGameObject newObj, NetworkGameObject oldObj, IWriter writer, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                TransformShortSerializer.Instance.Serialize(newObj.Transform, oldObj.Transform, writer);
            }

            if(newObj != null)
            {
                writer.Write(true);
                var oldBehaviours = oldObj == null ? new NetworkBehaviourContainer<INetworkBehaviour>() : oldObj.TypedBehaviours;
                _behaviourSerializer.Serialize(newObj.TypedBehaviours, oldBehaviours, writer);
            }
            else
            {
                writer.Write(false);
            }
        }
    }

    public delegate NetworkGameObject NetworkGameObjectFactoryDelegate(int objId, byte objType);

    public class NetworkGameObjectParser : IDiffReadParser<NetworkGameObject>
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

        readonly NetworkGameObjectFactoryDelegate _factory;
        readonly NetworkBehaviourContainerParser<INetworkBehaviour> _behaviourParser;

        public NetworkGameObjectParser(NetworkSceneContext context, NetworkGameObjectFactoryDelegate factory = null)
        {
            Context = context;
            _factory = factory;
            _behaviourParser = new NetworkBehaviourContainerParser<INetworkBehaviour>();
        }

        public NetworkGameObject Parse(IReader reader)
        {
            var objId = reader.ReadInt32();
            var objType = reader.ReadByte();
            NetworkGameObject obj = null;
            if(_factory != null)
            {
                obj = _factory(objId, objType);
                obj.Context = Context;
            }
            else
            {
                obj = Context.Pool.Get<NetworkGameObject>();
                obj.Init(Context, objId, false, null, objType, false);
            }
            if(obj != null)
            {
                obj.Transform.Copy(TransformShortParser.Instance.Parse(reader));
            }

            var hasBehaviours = reader.ReadBoolean();
            if(hasBehaviours && obj != null)
            {
                NetworkBehaviourContainer<INetworkBehaviour> behaviourContainer = _behaviourParser.Parse(reader);
                var itr = behaviourContainer.Behaviours.GetEnumerator();
                while(itr.MoveNext())
                {
                    if(itr.Current == null)
                    {
                        continue;
                    }

                    itr.Current.GameObject = obj;
                }
                itr.Dispose();
                obj.TypedBehaviours.Copy(behaviourContainer);
            }
            return obj;
        }
            
        NetworkGameObject CreateObject(int objId, byte objType)
        {
            return _factory(objId, objType);
        }

        public void RegisterBehaviour<T>(byte type, IDiffReadParser<T> parser) where T : INetworkBehaviour
        {
            _behaviourParser.Register(type, parser);
        }

        public int GetDirtyBitsSize(NetworkGameObject obj)
        {
            return 1;
        }

        public NetworkGameObject Parse(NetworkGameObject obj, IReader reader, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                obj.Transform.Copy(TransformShortParser.Instance.Parse(obj.Transform, reader));
            }

            var hasBehaviours = reader.ReadBoolean();
            if(hasBehaviours)
            {
                if(obj != null)
                {
                    _behaviourParser.Parse(obj.TypedBehaviours, reader);
                }
            }
            return obj;
        }
    }
}
