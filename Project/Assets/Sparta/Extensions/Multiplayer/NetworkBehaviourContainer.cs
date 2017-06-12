using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public interface INetworkBehaviourContainer
    {
        int Count{ get; }

        bool Contains(object obj);

        bool Remove(object obj);

        int Remove<T>();

        T Get<T>() where T : class;

        object Get(Type behavior);

        void GetAll<T>(List<T> result) where T : class;

        void Dispose();
    }

    public interface INetworkBehaviourContainer<Behaviour> : INetworkBehaviourContainer
    {
        void Add(Behaviour b);

        List<Behaviour>.Enumerator GetEnumerator(List<Behaviour> result);
    }

    public interface INetworkBehaviourProvider
    {
        INetworkBehaviourContainer Behaviours{ get; }
    }

    public static class NetworkBehaviourExtensions
    {
        public static void AddCloned<T>(this INetworkBehaviourContainer<T> container, IEnumerable<T> bs)
        {
            var itr = bs.GetEnumerator();
            while(itr.MoveNext())
            {
                var proto = itr.Current as ICloneable;
                if(proto == null)
                {
                    throw new ArgumentException(string.Format("Class {0} is not ICloneable.", itr.Current.GetType()));
                }
                container.Add((T)proto.Clone());
            }
            itr.Dispose();
        }

        public static void Add<T>(this INetworkBehaviourContainer<T> container, IEnumerable<T> bs)
        {
            if(bs == null)
            {
                return;
            }
            var itr = bs.GetEnumerator();
            while(itr.MoveNext())
            {
                container.Add(itr.Current);
            }
            itr.Dispose();
        }

        public static bool ContainsBehaviour<Behaviour>(this INetworkBehaviourProvider provider, Behaviour b)
        {
            var bs = provider.Behaviours;
            if(bs == null)
            {
                return false;
            }
            return bs.Contains(b);
        }

        public static bool RemoveBehaviour<Behaviour>(this INetworkBehaviourProvider provider, Behaviour b)
        {
            var bs = provider.Behaviours;
            if(bs == null)
            {
                return false;
            }
            return bs.Remove(b);
        }

        public static int RemoveBehaviour<T>(this INetworkBehaviourProvider provider)
        {
            var bs = provider.Behaviours;
            if(bs == null)
            {
                return 0;
            }
            return bs.Remove<T>();
        }

        public static T GetBehaviour<T>(this INetworkBehaviourProvider provider) where T : class
        {
            var bs = provider.Behaviours;
            if(bs == null)
            {
                return null;
            }
            return bs.Get<T>();
        }

        public static object GetBehaviour(this INetworkBehaviourProvider provider, Type behaviorType)
        {
            var bs = provider.Behaviours;
            if(bs == null)
            {
                return null;
            }
            return bs.Get(behaviorType);
        }

        public static void GetBehaviours<T>(this INetworkBehaviourProvider provider, List<T> result) where T : class
        {
            result.Clear();
            var bs = provider.Behaviours;
            if(bs == null)
            {
                return;
            }
            bs.GetAll<T>(result);
        }
    }

    public class NetworkBehaviourContainer<Behaviour> : INetworkBehaviourContainer<Behaviour>, ICloneable where Behaviour : class
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

        List<Behaviour> _behaviours = new List<Behaviour>();
        List<Type> _behavioursTypes = new List<Type>();
        List<ICopyable> _behavioursCopyable = new List<ICopyable>();
        List<ICloneable> _behavioursCloneable = new List<ICloneable>();

        public List<Behaviour> Behaviours { get { return _behaviours; } }

        public List<Type> BehavioursTypes { get { return _behavioursTypes; } }

        public Action<Behaviour> OnAdded;
        public Action<Behaviour> OnRemoved;

        public int Count
        {
            get
            {
                return _behaviours.Count;
            }
        }

        public List<Behaviour>.Enumerator GetEnumerator(List<Behaviour> tmp)
        {
            tmp.Clear();
            tmp.AddRange(_behaviours);
            return tmp.GetEnumerator();
        }

        public NetworkBehaviourContainer()
        {
        }

        public NetworkBehaviourContainer(NetworkSceneContext context)
        {
            Init(context);
        }

        public NetworkBehaviourContainer<Behaviour> Init(NetworkSceneContext context)
        {
            Context = context;

            return this;
        }

        public void Copy(NetworkBehaviourContainer<Behaviour> other)
        {
            Context = other.Context;
            var thisUpdated = Context.Pool.Get<List<object>>();

            for(var i = 0; i < other._behaviours.Count; i++)
            {
                var otherCopyable = other._behavioursCopyable[i];
                if(otherCopyable == null)
                {
                    continue;
                }
                var otherType = other._behavioursTypes[i];
                var found = false;
                for(var j = 0; j < _behaviours.Count; j++)
                {
                    var thisCopyable = _behavioursCopyable[j];
                    if(thisCopyable == null || thisUpdated.Contains(thisCopyable))
                    {
                        continue;
                    }
                    var thisType = _behavioursTypes[j];
                    if(otherType == thisType)
                    {
                        thisCopyable.Copy(otherCopyable);
                        thisUpdated.Add(thisCopyable);
                        found = true;
                        break;
                    }
                }
                if(!found)
                {
                    var otherCloneable = other._behavioursCloneable[i];
                    if(otherCloneable != null)
                    {
                        var clone = otherCloneable.Clone();
                        SocialPoint.Base.DebugUtils.Assert(clone.GetType() == otherCloneable.GetType(), "Cloned object of different type");
                        var newBehaviour = (Behaviour)clone;
                        var newCopyable = clone as ICopyable;
                        if(newCopyable != null)
                        {
                            newCopyable.Copy(otherCopyable);
                        }
                        Add(newBehaviour, otherType);
                        thisUpdated.Add(newBehaviour);
                    }
                }
            }
            for(var i = 0; i < _behaviours.Count; i++)
            {
                var thisCopyable = _behavioursCopyable[i];
                if(thisCopyable == null)
                {
                    continue;
                }
                if(!thisUpdated.Contains(thisCopyable))
                {
                    Remove(thisCopyable);
                }
            }

            Context.Pool.Return(thisUpdated);
        }

        public object Clone()
        {
            var container = new NetworkBehaviourContainer<Behaviour>(Context);
            for(var i = 0; i < _behaviours.Count; i++)
            {
                var cloneable = _behavioursCloneable[i];
                if(cloneable == null)
                {
                    continue;
                }
                container.Add((Behaviour)cloneable.Clone(), _behavioursTypes[i]);
            }
            return container;
        }


        public virtual void Dispose()
        {
            for(int i = 0; i < _behaviours.Count; ++i)
            {
                var behaviour = _behaviours[i] as INetworkBehaviour;
                if(behaviour != null)
                {
                    behaviour.Dispose();
                }
            }
            _behaviours.Clear();
            _behavioursTypes.Clear();
            _behavioursCopyable.Clear();
            _behavioursCloneable.Clear();

            OnAdded = null;
            OnRemoved = null;
        }

        public bool Contains(object obj)
        {
            var b = obj as Behaviour;
            if(b == null)
            {
                return false;
            }
            return _behaviours.Contains(b);
        }

        public bool Remove(object obj)
        {
            var b = obj as Behaviour;
            if(b == null)
            {
                return false;
            }
            int index = _behaviours.IndexOf(b);
            if(index == -1)
            {
                return false;
            }

            _behaviours.RemoveAt(index);
            _behavioursTypes.RemoveAt(index);
            _behavioursCopyable.RemoveAt(index);
            _behavioursCloneable.RemoveAt(index);
            if(OnRemoved != null)
            {
                OnRemoved(b);
            }
            return true;
        }

        public int Remove<T>()
        {
            var behaviours = _behaviours.FindAll(b => b is T);
            for(var i = 0; i < behaviours.Count; i++)
            {
                Remove(behaviours[i]);
            }
            return behaviours.Count;
        }

        public T Get<T>() where T : class
        {
            return (T)Get(typeof(T));
        }

        public object Get(Type behaviorType)
        {
            for(var i = 0; i < _behaviours.Count; i++)
            {
                var b = _behaviours[i];
                var bType = _behavioursTypes[i];
                if(behaviorType.IsAssignableFrom(bType))
                {
                    return b;
                }
            }
            return null;
        }

        public void GetAll(Type type, List<Behaviour> list)
        {
            list.Clear();
            for(var i = 0; i < _behaviours.Count; i++)
            {
                var b = _behaviours[i];
                if(type.IsInstanceOfType(b))
                {
                    list.Add(b);
                }
            }
        }

        public void GetAll<T>(List<T> list) where T : class
        {
            list.Clear();
            for(var i = 0; i < _behaviours.Count; i++)
            {
                var b = _behaviours[i] as T;
                if(b != null)
                {
                    list.Add(b);
                }
            }
        }

        public void Add(Behaviour b)
        {
            Add(b, b.GetType());
        }

        public void Add(Behaviour b, Type type)
        {
            if(b == null)
            {
                return;
            }
            if(_behaviours.Contains(b))
            {
                return;
            }
            _behaviours.Add(b);
            _behavioursTypes.Add(type);
            _behavioursCopyable.Add(b as ICopyable);
            _behavioursCloneable.Add(b as ICloneable);
            if(OnAdded != null)
            {
                OnAdded(b);
            }
        }

        public override int GetHashCode()
        {
            var hash = 0;
            for(var i = 0; i < _behaviours.Count; i++)
            {
                hash = CryptographyUtils.HashCombine(hash, _behaviours[i].GetHashCode());
            }
            return hash;
        }

        public override bool Equals(System.Object obj)
        {
            return Equals(obj as NetworkBehaviourContainer<Behaviour>);
        }

        public bool Equals(NetworkBehaviourContainer<Behaviour> container)
        {
            if((object)container == null)
            {
                return false;
            }
            return Compare(this, container);
        }

        public static bool operator ==(NetworkBehaviourContainer<Behaviour> a, NetworkBehaviourContainer<Behaviour> b)
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

        public static bool operator !=(NetworkBehaviourContainer<Behaviour> a, NetworkBehaviourContainer<Behaviour> b)
        {
            return !(a == b);
        }

        static bool Compare(NetworkBehaviourContainer<Behaviour> a, NetworkBehaviourContainer<Behaviour> b)
        {
            if(a.Count != b.Count)
            {
                return false;
            }
            var tmp = a.Context.Pool.Get<List<Behaviour>>();
            var itr = a.GetEnumerator(tmp);
            var result = false;
            while(itr.MoveNext())
            {
                if(!b.Contains(itr.Current))
                {
                    break;
                }
            }
            result = true;
            a.Context.Pool.Return(tmp);
            return result;
        }

        public override string ToString()
        {
            return string.Format("[NetworkBehaviourContainer<{0}>:{1}]", typeof(Behaviour), Count);
        }
    }

    public class NetworkBehaviourContainerObserver<Behaviour> : IDisposable where Behaviour : class
    {
        NetworkBehaviourContainer<Behaviour> _container;
        public List<Behaviour> Added = new List<Behaviour>();
        public List<Behaviour> Removed = new List<Behaviour>();

        public NetworkBehaviourContainerObserver<Behaviour> Init(NetworkBehaviourContainer<Behaviour> container)
        {
            _container = container;
            _container.OnAdded += OnAdded;
            _container.OnRemoved += OnRemoved;

            return this;
        }

        public void Dispose()
        {
            Clear();

            if(_container != null)
            {
                _container.OnAdded -= OnAdded;
                _container.OnRemoved -= OnRemoved;
                _container = null;
            }

            _container.Context.Pool.Return(this);
        }

        public void Clear()
        {
            Added.Clear();
            Removed.Clear();
        }

        void OnAdded(Behaviour behaviour)
        {
            Removed.Remove(behaviour);
            Added.Add(behaviour);
        }

        void OnRemoved(Behaviour behaviour)
        {
            Added.Remove(behaviour);
            Removed.Add(behaviour);
        }
    }

    public class NetworkBehaviourContainerSerializer<Behaviour> : IDiffWriteSerializer<NetworkBehaviourContainer<Behaviour>> where Behaviour : class
    {
        TypedDiffWriteSerializer<Behaviour> _serializer;

        public NetworkBehaviourContainerSerializer()
        {
            _serializer = new TypedDiffWriteSerializer<Behaviour>();
        }

        public void Register<T>(byte type, IDiffWriteSerializer<T> serializer) where T : Behaviour
        {
            _serializer.Register<T>(type, serializer);
        }

        public void Compare(NetworkBehaviourContainer<Behaviour> newObj, NetworkBehaviourContainer<Behaviour> oldObj, Bitset dirty)
        {
        }

        Dictionary<byte, KeyValuePair<Behaviour, Type>> GetSerializableBehaviours(NetworkBehaviourContainer<Behaviour> obj)
        {
            var all = obj.Behaviours;
            var allTypes = obj.BehavioursTypes;
            var behaviours = new Dictionary<byte, KeyValuePair<Behaviour, Type>>();
            byte code;
            for(var i = 0; i < all.Count; i++)
            {
                var behaviour = all[i];
                var type = allTypes[i];
                if(_serializer.FindCode(type, out code))
                {
                    if(behaviours.ContainsKey(code))
                    {
                        throw new InvalidOperationException("A container cannot have multiple serializable behaviours of the same type: " + code.ToString());
                    }
                    behaviours[code] = new KeyValuePair<Behaviour, Type>(behaviour, type);
                }
            }
            return behaviours;
        }

        public void Serialize(NetworkBehaviourContainer<Behaviour> newObj, IWriter writer)
        {
            var memStream = new MemoryStream();
            var memWriter = new SystemBinaryWriter(memStream);
            var behaviours = GetSerializableBehaviours(newObj);
            writer.Write(behaviours.Count);
            var itr = behaviours.GetEnumerator();
            while(itr.MoveNext())
            {
                _serializer.SerializeTyped(itr.Current.Value.Key, itr.Current.Value.Value, memWriter);
                writer.WriteByteArray(memStream.ToArray());
                memStream.Seek(0, SeekOrigin.Begin);
            }
            itr.Dispose();
        }

        public void Serialize(NetworkBehaviourContainer<Behaviour> newObj, NetworkBehaviourContainer<Behaviour> oldObj, IWriter writer, Bitset dirty)
        {
            var memStream = new MemoryStream();
            var memWriter = new SystemBinaryWriter(memStream);
            var newBehaviours = GetSerializableBehaviours(newObj);
            var oldBehaviours = GetSerializableBehaviours(oldObj);
            writer.Write(newBehaviours.Count);
            var itr = newBehaviours.GetEnumerator();
            while(itr.MoveNext())
            {
                KeyValuePair<Behaviour, Type> oldBehaviour;
                if(oldBehaviours.TryGetValue(itr.Current.Key, out oldBehaviour))
                {
                    _serializer.SerializeTyped(itr.Current.Value.Key, itr.Current.Value.Value, oldBehaviour.Key, oldBehaviour.Value, memWriter, dirty);
                }
                else
                {
                    _serializer.SerializeTyped(itr.Current.Value.Key, itr.Current.Value.Value, memWriter);
                }
                writer.Write(oldBehaviour.Key != null);
                writer.WriteByteArray(memStream.ToArray());
                memStream.Seek(0, SeekOrigin.Begin);
            }
            itr.Dispose();
            var removed = new List<byte>();
            itr = oldBehaviours.GetEnumerator();
            while(itr.MoveNext())
            {
                KeyValuePair<Behaviour, Type> newBehaviour;
                if(!newBehaviours.TryGetValue(itr.Current.Key, out newBehaviour))
                {
                    removed.Add(itr.Current.Key);
                }
            }
            itr.Dispose();
            writer.Write(removed.Count);
            for(var i = 0; i < removed.Count; i++)
            {
                writer.Write(removed[i]);
            }
        }

        public void Serialize(NetworkBehaviourContainer<Behaviour> newBehaviours, NetworkBehaviourContainer<Behaviour> oldBehaviours, IWriter writer)
        {
            var dirty = new Bitset();
            Compare(newBehaviours, oldBehaviours, dirty);
            dirty.Reset();
            dirty.Write(writer);
            Serialize(newBehaviours, oldBehaviours, writer, dirty);
        }
    }

    public class NetworkBehaviourContainerParser<Behaviour> : IDiffReadParser<NetworkBehaviourContainer<Behaviour>> where Behaviour : class
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

        TypedDiffReadParser<Behaviour> _behaviourParser;

        public NetworkBehaviourContainerParser(NetworkSceneContext context)
        {
            Context = context;
            _behaviourParser = new TypedDiffReadParser<Behaviour>();
        }

        public void Register<T>(byte type, IDiffReadParser<T> parser) where T : Behaviour
        {
            _behaviourParser.Register<T>(type, parser);
        }

        public int GetDirtyBitsSize(NetworkBehaviourContainer<Behaviour> obj)
        {
            return 0;
        }

        Dictionary<byte, Behaviour> GetSerializableBehaviours(NetworkBehaviourContainer<Behaviour> obj)
        {
            var all = obj.Behaviours;
            var allTypes = obj.BehavioursTypes;
            var behaviours = new Dictionary<byte, Behaviour>();
            byte code;
            for(var i = 0; i < all.Count; i++)
            {
                var behaviour = all[i];
                var type = allTypes[i];
                if(_behaviourParser.FindCode(type, out code))
                {
                    if(behaviours.ContainsKey(code))
                    {
                        throw new InvalidOperationException("A container cannot have multiple serializable behaviours of the same type: " + code.ToString());
                    }
                    behaviours[code] = behaviour;
                }
            }
            return behaviours;
        }

        public NetworkBehaviourContainer<Behaviour> Parse(IReader reader)
        {
            var obj = new NetworkBehaviourContainer<Behaviour>(Context);
            var behaviourNum = reader.ReadInt32();
            var memSteam = new MemoryStream();
            var memReader = new SystemBinaryReader(memSteam);
            for(var i = 0; i < behaviourNum; i++)
            {
                var bytes = reader.ReadByteArray();
                memSteam.Write(bytes, 0, bytes.Length);
                memSteam.Seek(0, SeekOrigin.Begin);
                var code = memReader.ReadByte();
                Behaviour behaviour;
                if(_behaviourParser.TryParse(code, memReader, out behaviour))
                {
                    obj.Add(behaviour);
                }
                memSteam.Seek(0, SeekOrigin.Begin);
            }
            return obj;
        }

        public NetworkBehaviourContainer<Behaviour> Parse(NetworkBehaviourContainer<Behaviour> obj, IReader reader, Bitset dirty)
        {
            var memSteam = new MemoryStream();
            var memReader = new SystemBinaryReader(memSteam);
            var oldBehaviours = GetSerializableBehaviours(obj);
            var behaviourNum = reader.ReadInt32();
            for(var i = 0; i < behaviourNum; i++)
            {
                var isDiff = reader.ReadBoolean();
                var bytes = reader.ReadByteArray();
                memSteam.Write(bytes, 0, bytes.Length);
                memSteam.Seek(0, SeekOrigin.Begin);
                var code = memReader.ReadByte();
                Behaviour behaviour;
                if(isDiff)
                {
                    if(oldBehaviours.TryGetValue(code, out behaviour))
                    {
                        _behaviourParser.TryParse(code, behaviour, memReader, out behaviour);
                    }
                }
                else
                {
                    if(_behaviourParser.TryParse(code, memReader, out behaviour))
                    {
                        obj.Add(behaviour);
                    }
                }
                memSteam.Seek(0, SeekOrigin.Begin);
            }
            behaviourNum = reader.ReadInt32();
            for(var i = 0; i < behaviourNum; i++)
            {
                var code = reader.ReadByte();
                Behaviour behaviour;
                if(oldBehaviours.TryGetValue(code, out behaviour))
                {
                    obj.Remove(behaviour);
                }
            }
            return obj;
        }
    }
}
