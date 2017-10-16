using System.Collections.Generic;
using Jitter.LinearMath;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public class PrefabDummySet : INetworkShareable
    {
        public Dictionary<string, StateDummySet> _stateDummies;

        public PrefabDummySet()
        {
            _stateDummies = new Dictionary<string, StateDummySet>();
        }

#if UNITY_EDITOR
        public void AddDummyOffset(string animation, string dummyType, JVector offset)
        {
            if (!_stateDummies.ContainsKey(animation))
            {
                _stateDummies.Add(animation, new StateDummySet());
            }

            _stateDummies[animation].SetDummyOffset(dummyType, offset);
        }
#endif

        public bool GetDummyOffset(string animation, string dummyType, out JVector offset)
        {
            if(_stateDummies.ContainsKey(animation))
            {
                return _stateDummies[animation].GetDummyOffset(dummyType, out offset);
            }

            // Not found
            offset = JVector.Zero;
            return false;
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(_stateDummies.Count);

            var iterator = _stateDummies.GetEnumerator();
            while(iterator.MoveNext())
            {
                writer.Write(iterator.Current.Key);
                writer.Write(iterator.Current.Value);
            }
            iterator.Dispose();
        }

        public void Deserialize(IReader reader)
        {
            int n = reader.ReadInt32();

            for(int i = 0; i < n; ++i)
            {
                string id = reader.ReadString();
                var offsets = new StateDummySet();
                offsets.Deserialize(reader);

                _stateDummies.Add(id, offsets);
            }
        }
    }

    public class StateDummySet : INetworkShareable
    {
        public Dictionary<string, JVector> _stateOffsets;

        public StateDummySet()
        {
            _stateOffsets = new Dictionary<string, JVector>();
        }

        public void SetDummyOffset(string dummyType, JVector offset)
        {
            if(_stateOffsets.ContainsKey(dummyType))
            {
                _stateOffsets[dummyType] = offset;
            }
            else
            {
                _stateOffsets.Add(dummyType, offset);
            }
        }

        public bool GetDummyOffset(string dummyType, out JVector offset)
        {
            if(_stateOffsets.ContainsKey(dummyType))
            {
                offset = _stateOffsets[dummyType];
                return true;
            }
            else
            {
                offset = JVector.Zero;
                return false;
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(_stateOffsets.Count);
            var iterator = _stateOffsets.GetEnumerator();
            while(iterator.MoveNext())
            {
                writer.Write(iterator.Current.Key);
                writer.Write(iterator.Current.Value.X);
                writer.Write(iterator.Current.Value.Y);
                writer.Write(iterator.Current.Value.Z);
            }
            iterator.Dispose();
        }

        public void Deserialize(IReader reader)
        {
            int n = reader.ReadInt32();
            for(int i = 0; i < n; ++i)
            {
                string key = reader.ReadString();

                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                JVector value = new JVector(x, y, z);

                _stateOffsets.Add(key, value);
            }
        }
    }
}
