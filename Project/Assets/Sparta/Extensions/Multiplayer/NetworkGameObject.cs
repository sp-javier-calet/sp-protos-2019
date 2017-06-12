using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Utils;
using System;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
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

        public NetworkGameObject()
        {
        }

        public NetworkGameObject(NetworkSceneContext context)
        {
            Context = context;
        }

        public void Init(NetworkSceneContext context, int id, bool isServerGameObject = false, Transform transform = null, byte type = 0, bool local = false, int syncGroup = 0)
        {
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

        public virtual object Clone()
        {
            var instance = Context.Pool.Get<NetworkGameObject>();
            instance.Init(Context, UniqueId, IsServerGameObject, Transform, Type, Local, SyncGroup);
            return instance;
        }

        public virtual object DeepClone()
        {
            return Clone();
        }

        public virtual void DeepCopy(NetworkGameObject other)
        {
            Copy(other);
        }

        public virtual void Dispose()
        {
            Invalid = false;
            Type = 0;
            Id = 0;
            Local = false;
            _syncGroup = 0;
            IsServerGameObject = false;

            Transform.Dispose();
            Context.Pool.Return(Transform);
            Transform = null;

            Behaviours.Dispose();
            Context.Pool.Return(Behaviours);
            Behaviours = null;

            Context.Pool.Return(this);
        }

        public virtual void OnDestroy()
        {
        }

        public virtual void Update(float dt)
        {
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
    }

    public interface INetworkBehaviour : ICloneable
    {
        NetworkGameObject GameObject { set; }

        void OnAwake();

        void OnStart();

        void OnDestroy();

        void Dispose();

        void Update(float dt);
    }

    public class NetworkGameObject<B> : NetworkGameObject where B : class, INetworkBehaviour
    {
        public NetworkBehaviourContainer<B> TypedBehaviours{ get; protected set; }

        protected NetworkBehaviourContainerObserver<B> _behaviourObserver;

        public NetworkGameObject()
        {
            TypedBehaviours = new NetworkBehaviourContainer<B>();
            Behaviours = TypedBehaviours;
            _behaviourObserver = new NetworkBehaviourContainerObserver<B>().Init(TypedBehaviours);
        }

        public NetworkGameObject(NetworkSceneContext context) : base(context)
        {
            TypedBehaviours = Context.Pool.Get<NetworkBehaviourContainer<B>>().Init(Context);
            Behaviours = TypedBehaviours;
            _behaviourObserver = Context.Pool.Get<NetworkBehaviourContainerObserver<B>>().Init(TypedBehaviours);
        }

        public void Init(NetworkSceneContext context,
                         int id,
                         bool isServerGameObject = false,
                         Transform transform = null,
                         byte type = 0,
                         bool local = false,
                         int syncGroup = 0)
        {
            base.Init(context, id, isServerGameObject, transform, type, local, syncGroup);

            TypedBehaviours.Init(Context);
            Behaviours = TypedBehaviours;
        }

        public void Init(NetworkSceneContext context,
                         int id,
                         bool isServerGameObject, 
                         Transform transform,
                         byte type,
                         bool local, 
                         NetworkBehaviourContainer<B> typedBehaviours, 
                         NetworkBehaviourContainerObserver<B> behaviourObserver = null,
                         int syncGroup = 0)
        {
            base.Init(context, id, isServerGameObject, transform, type, local, syncGroup);

            Behaviours = TypedBehaviours = typedBehaviours;
            _behaviourObserver = behaviourObserver ?? Context.Pool.Get<NetworkBehaviourContainerObserver<B>>().Init(typedBehaviours);
        }

        public override object Clone()
        {
            var other = Context.Pool.Get<NetworkGameObject<B>>();
            other.Init(Context, UniqueId, IsServerGameObject, Transform, Type, Local, TypedBehaviours, _behaviourObserver);
            return other;
        }

        public override object DeepClone()
        {
            var typedBehaviours = (NetworkBehaviourContainer<B>)TypedBehaviours.Clone();
            var other = Context.Pool.Get<NetworkGameObject<B>>();
            other.Init(Context, UniqueId, IsServerGameObject, Transform, Type, Local, typedBehaviours, null, SyncGroup);
            return other;
        }

        public override void Dispose()
        {
            _behaviourObserver.Dispose();
            _behaviourObserver = null;
            TypedBehaviours = null;

            base.Dispose();
        }

        public override void DeepCopy(NetworkGameObject other)
        {
            base.DeepCopy(other);
            var bother = other as NetworkGameObject<B>;
            if(bother != null)
            {
                TypedBehaviours.Copy(bother.TypedBehaviours);
            }
        }

        public B AddBehaviour(B behaviour, Type type)
        {
            TypedBehaviours.Add(behaviour, type);
            return behaviour;
        }

        public void AddBehaviours(IEnumerable<B> bs, IEnumerable<Type> types)
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
                AddBehaviour(itr.Current, itrType.Current);
            }
            itr.Dispose();
            itrType.Dispose();
        }

        public void AddClonedBehaviours(IEnumerable<B> bs, IEnumerable<Type> types)
        {
            var itr = bs.GetEnumerator();
            var itrType = types.GetEnumerator();
            while(itr.MoveNext())
            {
                itrType.MoveNext();
                var proto = itr.Current as ICloneable;
                if(proto == null)
                {
                    throw new ArgumentException(string.Format("Class {0} is not ICloneable.", itr.Current.GetType()));
                }
                AddBehaviour((B)proto.Clone(), itrType.Current);
            }
            itr.Dispose();
            itrType.Dispose();
        }

        public override void OnDestroy()
        {
            var tmp = Context.Pool.Get<List<B>>();
            var itr = TypedBehaviours.GetEnumerator(tmp);
            while(itr.MoveNext())
            {
                itr.Current.OnDestroy();
            }
            Context.Pool.Return(tmp);
            itr.Dispose();
        }

        public override void Update(float dt)
        {
            var tmp = Context.Pool.Get<List<B>>();
            var itr = TypedBehaviours.GetEnumerator(tmp);
            while(itr.MoveNext())
            {
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
    }

    public class NetworkGameObjectSerializer : IDiffWriteSerializer<NetworkGameObject>
    {
        public void Compare(NetworkGameObject newObj, NetworkGameObject oldObj, Bitset dirty)
        {
            dirty.Set(newObj.Transform != oldObj.Transform);
        }

        public void Serialize(NetworkGameObject newObj, IWriter writer)
        {
            writer.Write(newObj.Id);
            writer.Write(newObj.Type);
            TransformShortSerializer.Instance.Serialize(newObj.Transform, writer);
        }

        public void Serialize(NetworkGameObject newObj, NetworkGameObject oldObj, IWriter writer, Bitset dirty)
        {
            if(Bitset.NullOrGet(dirty))
            {
                TransformShortSerializer.Instance.Serialize(newObj.Transform, oldObj.Transform, writer);
            }
        }
    }

    public class NetworkGameObjectSerializer<Behaviour> : IDiffWriteSerializer<NetworkGameObject> where Behaviour : class, INetworkBehaviour
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

        NetworkGameObjectSerializer _objectSerializer;
        NetworkBehaviourContainerSerializer<Behaviour> _behaviourSerializer;

        public NetworkGameObjectSerializer(NetworkSceneContext context)
        {
            Context = context;
            _objectSerializer = new NetworkGameObjectSerializer();
            _behaviourSerializer = new NetworkBehaviourContainerSerializer<Behaviour>();
        }

        public void RegisterBehaviour<T>(byte type, IDiffWriteSerializer<T> serializer) where T : Behaviour
        {
            _behaviourSerializer.Register(type, serializer);
        }

        public void Compare(NetworkGameObject newObj, NetworkGameObject oldObj, Bitset dirty)
        {
        }

        public void Serialize(NetworkGameObject newObj, IWriter writer)
        {
            _objectSerializer.Serialize(newObj, writer);

            var newBobj = newObj as NetworkGameObject<Behaviour>;
            if(newBobj != null)
            {
                writer.Write(true);
                _behaviourSerializer.Serialize(newBobj.TypedBehaviours, writer);
            }
            else
            {
                writer.Write(false);
            }
        }

        public void Serialize(NetworkGameObject newObj, NetworkGameObject oldObj, IWriter writer, Bitset dirty)
        {
            _objectSerializer.Serialize(newObj, oldObj, writer);

            var newBobj = newObj as NetworkGameObject<Behaviour>;
            if(newBobj != null)
            {
                writer.Write(true);
                var oldBobj = oldObj as NetworkGameObject<Behaviour>;
                var oldBehaviours = oldBobj == null ? new NetworkBehaviourContainer<Behaviour>(Context) : oldBobj.TypedBehaviours;
                _behaviourSerializer.Serialize(newBobj.TypedBehaviours, oldBehaviours, writer);
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

        NetworkGameObjectFactoryDelegate _factory;

        public NetworkGameObjectParser(NetworkSceneContext context, NetworkGameObjectFactoryDelegate factory = null)
        {
            Context = context;
            _factory = factory;
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
                obj.Transform.Copy(TransformShortParser.Instance.Parse(obj.Transform, reader));
            }
            return obj;
        }
    }

    public class NetworkGameObjectParser<Behaviour> : IDiffReadParser<NetworkGameObject> where Behaviour : class, INetworkBehaviour
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

        NetworkGameObjectFactoryDelegate _factory;
        NetworkGameObjectParser _objectParser;
        NetworkBehaviourContainerParser<Behaviour> _behaviourParser;

        public NetworkGameObjectParser(NetworkSceneContext context, NetworkGameObjectFactoryDelegate factory = null)
        {
            Context = context;
            DebugUtils.Assert(factory != null, "factory cannot be null");
            _factory = factory;
            _objectParser = new NetworkGameObjectParser(Context, CreateObject);
            _behaviourParser = new NetworkBehaviourContainerParser<Behaviour>(Context);
        }

        NetworkGameObject CreateObject(int objId, byte objType)
        {
            return _factory(objId, objType);
        }

        public void RegisterBehaviour<T>(byte type, IDiffReadParser<T> parser) where T : Behaviour
        {
            _behaviourParser.Register(type, parser);
        }

        public NetworkGameObject Parse(IReader reader)
        {
            var obj = _objectParser.Parse(reader);

            var bobj = obj as NetworkGameObject<Behaviour>;
            var hasBehaviours = reader.ReadBoolean();
            if(hasBehaviours && bobj != null)
            {
                NetworkBehaviourContainer<Behaviour> behaviourContainer = _behaviourParser.Parse(reader);
                var itr = behaviourContainer.Behaviours.GetEnumerator();
                while(itr.MoveNext())
                {
                    itr.Current.GameObject = obj;
                }
                itr.Dispose();
                bobj.TypedBehaviours.Copy(behaviourContainer);
            }
            return obj;
        }

        public int GetDirtyBitsSize(NetworkGameObject obj)
        {
            return 0;
        }

        public NetworkGameObject Parse(NetworkGameObject obj, IReader reader, Bitset dirty)
        {
            _objectParser.Parse(obj, reader);

            var hasBehaviours = reader.ReadBoolean();
            if(hasBehaviours)
            {
                var bobj = obj as NetworkGameObject<Behaviour>;
                if(bobj != null)
                {
                    _behaviourParser.Parse(bobj.TypedBehaviours, reader);
                }
            }
            return obj;
        }
    }

    public class NetworkServerGameObjectSerializer : NetworkGameObjectSerializer<INetworkBehaviour>
    {
        public NetworkServerGameObjectSerializer(NetworkSceneContext context) : base(context)
        {
        }
    }
}
