using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Utils;
using System.Collections.ObjectModel;

namespace SocialPoint.Multiplayer
{
    public interface INetworkBehaviourContainer
    {
        int Count{ get; }

        bool Contains(object obj);

        bool Remove(object obj);

        int Remove<T>();

        T Get<T>(int idx) where T : class;

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
        const int MaxCommonBehaviours = 5;
        static Type[] CommonBehaviourTypes = new Type[MaxCommonBehaviours];

        static bool CheckCommonBehavioursContainsType(Type type)
        {
            for(int i = 0; i < MaxCommonBehaviours; ++i)
            {
                if(CommonBehaviourTypes[i] != null && (CommonBehaviourTypes[i].IsAssignableFrom(type) || type.IsAssignableFrom(CommonBehaviourTypes[i])))
                {
                    return true;
                }
            }

            return false;
        }

        public static void AddCommonBehaviourType(int idx, Type type)
        {
            SocialPoint.Base.DebugUtils.Assert(idx >= 0);
            SocialPoint.Base.DebugUtils.Assert(idx < MaxCommonBehaviours);
            SocialPoint.Base.DebugUtils.Assert(type != null);
            SocialPoint.Base.DebugUtils.Assert(CommonBehaviourTypes[idx] == null);
            SocialPoint.Base.DebugUtils.Assert(CheckCommonBehavioursContainsType(type) == false);

            CommonBehaviourTypes[idx] = type;
        }

        object[] _commonBehaviours = new object[MaxCommonBehaviours];

        // Lenght of these four variables must match always.
        List<Behaviour> _behaviours = new List<Behaviour>();
        List<Type> _behavioursTypes = new List<Type>();
        List<ICopyable> _behavioursCopyable = new List<ICopyable>();
        List<ICloneable> _behavioursCloneable = new List<ICloneable>();

        public ReadOnlyCollection<Behaviour> Behaviours { get { return _behaviours.AsReadOnly(); } }
        public ReadOnlyCollection<Type> BehavioursTypes { get { return _behavioursTypes.AsReadOnly(); } }

        // Lenght of these three variables must match always.
        List<Behaviour> _serializableBehaviours = new List<Behaviour>();
        List<Type> _serializableBehavioursTypes = new List<Type>();
        List<byte> _serializersCodes = new List<byte>();

        public ReadOnlyCollection<Behaviour> SerializableBehaviours { get { return _serializableBehaviours.AsReadOnly(); } }
        public ReadOnlyCollection<Type> SerializableBehavioursTypes { get { return _serializableBehavioursTypes.AsReadOnly(); } }
        public ReadOnlyCollection<byte> SerializersCodes { get { return _serializersCodes.AsReadOnly(); } }

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

        public void Copy(NetworkBehaviourContainer<Behaviour> other)
        {
            var otherCount = other._behaviours.Count;
            var count = _behaviours.Count;

            var countDiff = count - otherCount;
            for(int i = 0; i < countDiff; ++i)
            {
                RemoveAt(i);
            }

            if(countDiff != 0)
            {
                Resize(otherCount);
            }

            for(var i = 0; i < other._behaviours.Count; ++i)
            {
                var otherCopyable = other._behavioursCopyable[i];
                var thisCopyable = _behavioursCopyable[i];

                if(otherCopyable == null)
                {
                    if(thisCopyable != null)
                    {
                        RemoveAt(i);
                    }

                    continue;
                }

                if(thisCopyable == null)
                {
                    var otherCloneable = other._behavioursCloneable[i];
                    var clone = otherCloneable.Clone();
                    SocialPoint.Base.DebugUtils.Assert(clone.GetType() == otherCloneable.GetType(), "Cloned object of different type");
                    var newBehaviour = (Behaviour)clone;
                    var otherType = other._behavioursTypes[i];
                    Add(i, newBehaviour, otherType);
                }
                else
                {
                    thisCopyable.Copy(otherCopyable);
                }
            }
        }

        public object Clone()
        {
            var container = new NetworkBehaviourContainer<Behaviour>();
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
                var behaviour = _behaviours[i] as IDisposable;
                if(behaviour != null)
                {
                    behaviour.Dispose();
                }
            }
            _behaviours.Clear();
            _behavioursTypes.Clear();
            _behavioursCopyable.Clear();
            _behavioursCloneable.Clear();

            _serializableBehaviours.Clear();
            _serializableBehavioursTypes.Clear();
            _serializersCodes.Clear();

            for(int i = 0; i < _commonBehaviours.Length; ++i)
            {
                _commonBehaviours[i] = null;
            }

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

            bool isOk = RemoveAt(index);

            int serializablesIndex = _serializableBehaviours.IndexOf(b);
            if(serializablesIndex != -1)
            {
                _serializableBehaviours.RemoveAt(serializablesIndex);
                _serializableBehavioursTypes.RemoveAt(serializablesIndex);
                _serializersCodes.RemoveAt(serializablesIndex);
            }

            return isOk;
        }

        public bool RemoveAt(int index)
        {
            var b = _behaviours[index];
            if(b == null)
            {
                return false;
            }

            var behaviour = _behaviours[index] as IDisposable;
            if(behaviour != null)
            {
                behaviour.Dispose();
            }

            _behaviours[index] = null;
            _behavioursTypes[index] = null;
            _behavioursCopyable[index] = null;
            _behavioursCloneable[index] = null;

            for(int i = 0; i < MaxCommonBehaviours; ++i)
            {
                if(_commonBehaviours[i] == b)
                {
                    _commonBehaviours[i] = null;
                    break;
                }
            }

            if(OnRemoved != null)
            {
                OnRemoved(b);
            }

            return true;
        }

        void Resize(int size)
        {
            _behaviours.Resize(size);
            _behavioursTypes.Resize(size);
            _behavioursCopyable.Resize(size);
            _behavioursCloneable.Resize(size);
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

        public T Get<T>(int idx) where T : class
        {
            SocialPoint.Base.DebugUtils.Assert(idx < MaxCommonBehaviours);
            SocialPoint.Base.DebugUtils.Assert(_commonBehaviours[idx] == null || _commonBehaviours[idx] is T);

            return (T)_commonBehaviours[idx];
        }

        public T Get<T>() where T : class
        {
            return (T)Get(typeof(T));
        }

        public object Get(Type behaviorType)
        {
            for(var i = 0; i < _behaviours.Count; i++)
            {
                var bType = _behavioursTypes[i];
                if(behaviorType.IsAssignableFrom(bType))
                {
                    return _behaviours[i];
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

        public delegate bool FindCodeDelegate(Type type, out byte code);

        public void ComputeSerializableBehaviours(FindCodeDelegate FindCode)
        {
            SocialPoint.Base.DebugUtils.Assert(_serializableBehaviours.Count == _serializableBehavioursTypes.Count);
            SocialPoint.Base.DebugUtils.Assert(_serializableBehaviours.Count == _serializersCodes.Count);

            if(_serializableBehaviours.Count > 0)
            {
                return;
            }

            for(int i = 0; i < _behavioursTypes.Count; ++i)
            {
                var behaviourType = _behavioursTypes[i];
                byte code;

                if(behaviourType != null && FindCode(behaviourType, out code))
                {
                    _serializableBehaviours.Add(_behaviours[i]);
                    _serializableBehavioursTypes.Add(behaviourType);
                    _serializersCodes.Add(code);
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

            var index = _behaviours.Count;
            Resize(index + 1);
            Add(index, b, type);
        }

        void Add(int index, Behaviour b, Type type)
        {
            _behaviours[index] = b;
            _behavioursTypes[index] = type;
            _behavioursCopyable[index] = b as ICopyable;
            _behavioursCloneable[index] = b as ICloneable;

            for(int i = 0; i < MaxCommonBehaviours; ++i)
            {
                if(CommonBehaviourTypes[i] != null && CommonBehaviourTypes[i].IsAssignableFrom(type))
                {
                    _commonBehaviours[i] = b;
                    break;
                }
            }

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

            var tmp = new List<Behaviour>();
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
        MemoryStream _memStream = new MemoryStream(64 * 1024);
        List<byte> _removedObjects = new List<byte>();

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

        public void Serialize(NetworkBehaviourContainer<Behaviour> newObj, IWriter writer)
        {
            _memStream.SetLength(0);
            _memStream.Seek(0, SeekOrigin.Begin);
            var memWriter = new SystemBinaryWriter(_memStream);

            newObj.ComputeSerializableBehaviours(_serializer.FindCode);
            var serializableBehaviours = newObj.SerializableBehaviours;
            var serializableBehavioursTypes = newObj.SerializableBehavioursTypes;

            writer.Write(serializableBehaviours.Count);
            
            for(int i = 0; i < serializableBehaviours.Count; ++i)
            {
                _serializer.SerializeTyped(serializableBehaviours[i], serializableBehavioursTypes[i], memWriter);
                writer.WriteByteArray(_memStream.GetBuffer(), (int)_memStream.Length);
                _memStream.SetLength(0);
                _memStream.Seek(0, SeekOrigin.Begin);
            }
        }

        public void Serialize(NetworkBehaviourContainer<Behaviour> newObj, NetworkBehaviourContainer<Behaviour> oldObj, IWriter writer, Bitset dirty)
        {
            _memStream.SetLength(0);
            _memStream.Seek(0, SeekOrigin.Begin);
            var memWriter = new SystemBinaryWriter(_memStream);

            newObj.ComputeSerializableBehaviours(_serializer.FindCode);
            oldObj.ComputeSerializableBehaviours(_serializer.FindCode);

            var newSerializableBehaviours = newObj.SerializableBehaviours;
            var newSerializableBehavioursTypes = newObj.SerializableBehavioursTypes;
            var newSerializersCodes = newObj.SerializersCodes;
            var oldSerializableBehaviours = oldObj.SerializableBehaviours;
            var oldSerializableBehavioursTypes = oldObj.SerializableBehavioursTypes;
            var oldSerializersCodes = oldObj.SerializersCodes;

            writer.Write(newSerializableBehaviours.Count);

            for(int i = 0; i < newSerializableBehaviours.Count; ++i)
            {
                bool existsInOldObj = oldSerializersCodes.Contains(newSerializersCodes[i]);
                if(oldSerializersCodes.Contains(newSerializersCodes[i]))
                {
                    _serializer.SerializeTyped(newSerializableBehaviours[i], newSerializableBehavioursTypes[i],
                        oldSerializableBehaviours[i], oldSerializableBehavioursTypes[i], memWriter, dirty);
                }
                else
                {
                    _serializer.SerializeTyped(newSerializableBehaviours[i], newSerializableBehavioursTypes[i], memWriter);
                }
                writer.Write(existsInOldObj);
                writer.WriteByteArray(_memStream.GetBuffer(), (int)_memStream.Length);
                _memStream.SetLength(0);
                _memStream.Seek(0, SeekOrigin.Begin);
            }

            _removedObjects.Clear();
            for(int i = 0; i < oldSerializersCodes.Count; ++i)
            {
                if(!newSerializersCodes.Contains(oldSerializersCodes[i]))
                {
                    _removedObjects.Add(oldSerializersCodes[i]);
                }
            }

            writer.Write(_removedObjects.Count);
            for(var i = 0; i < _removedObjects.Count; i++)
            {
                writer.Write(_removedObjects[i]);
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
        TypedDiffReadParser<Behaviour> _behaviourParser;
        MemoryStream _memStream = new MemoryStream(64 * 1024);

        public NetworkBehaviourContainerParser()
        {
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

        public NetworkBehaviourContainer<Behaviour> Parse(IReader reader)
        {
            var obj = new NetworkBehaviourContainer<Behaviour>();
            var behaviourNum = reader.ReadInt32();
            _memStream.SetLength(0);
            _memStream.Seek(0, SeekOrigin.Begin);
            var memReader = new SystemBinaryReader(_memStream);
            for(var i = 0; i < behaviourNum; i++)
            {
                var bytes = reader.ReadByteArray();
                _memStream.Write(bytes, 0, bytes.Length);
                _memStream.Seek(0, SeekOrigin.Begin);
                var code = memReader.ReadByte();
                Behaviour behaviour;
                if(_behaviourParser.TryParse(code, memReader, out behaviour))
                {
                    obj.Add(behaviour);
                }
                _memStream.Seek(0, SeekOrigin.Begin);
            }

            return obj;
        }

        public NetworkBehaviourContainer<Behaviour> Parse(NetworkBehaviourContainer<Behaviour> obj, IReader reader, Bitset dirty)
        {
            _memStream.SetLength(0);
            _memStream.Seek(0, SeekOrigin.Begin);
            var memReader = new SystemBinaryReader(_memStream);

            obj.ComputeSerializableBehaviours(_behaviourParser.FindCode);

            var behaviourNum = reader.ReadInt32();
            for(var i = 0; i < behaviourNum; i++)
            {
                var isDiff = reader.ReadBoolean();
                var bytes = reader.ReadByteArray();
                _memStream.Write(bytes, 0, bytes.Length);
                _memStream.Seek(0, SeekOrigin.Begin);
                var code = memReader.ReadByte();

                Behaviour behaviour;
                if(isDiff)
                {
                    int index = obj.SerializersCodes.IndexOf(code);
                    if(index != -1)
                    {
                        _behaviourParser.TryParse(code, obj.SerializableBehaviours[index], memReader, out behaviour);
                    }
                }
                else
                {
                    if(_behaviourParser.TryParse(code, memReader, out behaviour))
                    {
                        obj.Add(behaviour);
                    }
                }
                _memStream.Seek(0, SeekOrigin.Begin);
            }
           
            behaviourNum = reader.ReadInt32();
            for(var i = 0; i < behaviourNum; i++)
            {
                var code = reader.ReadByte();
                int index = obj.SerializersCodes.IndexOf(code);
                if(index != -1)
                {
                    obj.Remove(obj.SerializableBehaviours[index]);
                }
            }

            return obj;
        }
    }
}
