using System.Collections.Generic;
using System;
using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public class LockstepReplay : IDisposable, INetworkShareable
    {
        ClientLockstepController _client;
        LockstepCommandFactory _commandFactory;
        Dictionary<int, ClientLockstepTurnData> _turns;
        LockstepConfig _config;
        uint _randomSeed;

        public LockstepReplay(ClientLockstepController clientLockstep, LockstepCommandFactory commandFactory)
        {
            _turns = new Dictionary<int, ClientLockstepTurnData>();
            _client = clientLockstep;
            _commandFactory = commandFactory;
        }

        public int CommandCount
        {
            get
            {
                var count = 0;
                var itr = _turns.GetEnumerator();
                while(itr.MoveNext())
                {
                    count += itr.Current.Value.CommandCount;
                }
                itr.Dispose();
                return count;
            }
        }

        public void Clear()
        {
            _config = null;
            _randomSeed = 0;
            _turns.Clear();
        }

        public void Record()
        {
            _config = null;
            _randomSeed = 0;
            _client.TurnApplied += OnTurnApplied;
        }

        public void Replay()
        {
            if(_config == null)
            {
                _config = new LockstepConfig();
            }
            _client.Config = _config;
            _client.RandomSeed = _randomSeed;
            var itr = GetTurnsEnumerator();
            while(itr.MoveNext())
            {
                _client.AddConfirmedTurn(itr.Current);
            }
            itr.Dispose();
        }

        public IEnumerator<ClientLockstepTurnData> GetTurnsEnumerator()
        {
            var t = 0;
            var itr = _turns.GetEnumerator();
            while(itr.MoveNext())
            {
                while(t < itr.Current.Key)
                {
                    yield return ClientLockstepTurnData.Empty;
                    t++;
                }
                yield return itr.Current.Value;
            }
            itr.Dispose();
        }

        public void Reset()
        {
            _turns.Clear();
            _config = null;
        }

        public void AddTurn(int num, ClientLockstepTurnData turn)
        {
            _turns[num] = turn;
        }

        public void AddCommand(int turnNum, ClientLockstepCommandData data)
        {
            ClientLockstepTurnData turn;
            if(!_turns.TryGetValue(turnNum, out turn))
            {
                turn = new ClientLockstepTurnData();
                _turns[turnNum] = turn;
            }
            turn.AddCommand(data);
        }

        public void AddCommand(int turnNum, ILockstepCommand cmd, ILockstepCommandLogic finish = null)
        {
            AddCommand(turnNum, new ClientLockstepCommandData(cmd, finish));
        }

        void OnTurnApplied(ClientLockstepTurnData turn)
        {
            if(_config == null)
            {
                _config = _client.Config;
            }
            if(_randomSeed == 0)
            {
                _randomSeed = _client.RandomSeed;
            }
            if(!ClientLockstepTurnData.IsNullOrEmpty(turn))
            {
                _turns[_client.CurrentTurnNumber] = turn;
            }
        }

        public void Serialize(IWriter writer)
        {
            if(_config == null)
            {
                return;
            }
            _config.Serialize(writer);
            writer.Write(_randomSeed);
            writer.Write(_turns.Count);
            var itr = _turns.GetEnumerator();
            while(itr.MoveNext())
            {
                writer.Write(itr.Current.Key);
                itr.Current.Value.Serialize(_commandFactory, writer);
            }
            itr.Dispose();
        }

        public void Deserialize(IReader reader)
        {
            _config = new LockstepConfig();
            _config.Deserialize(reader);
            _randomSeed = reader.ReadUInt32();
            int count = reader.ReadInt32();
            for(int i = 0; i < count; ++i)
            {
                var num = reader.ReadInt32();
                var turn = new ClientLockstepTurnData();
                turn.Deserialize(_commandFactory, reader);
                _turns[num] = turn;
            }
        }

        public void Dispose()
        {
            if(_client != null)
            {
                _client.TurnApplied -= OnTurnApplied;
            }
        }
    }
}