using System.Collections.Generic;
using System;
using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public class LockstepReplay : IDisposable, INetworkShareable
    {
        LockstepClient _client;
        LockstepCommandFactory _commandFactory;
        Dictionary<int, ClientTurnData> _turns;
        LockstepConfig _config;
        LockstepGameParams _gameParams;

        public LockstepReplay(LockstepClient clientLockstep, LockstepCommandFactory commandFactory)
        {
            _turns = new Dictionary<int, ClientTurnData>();
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
            _gameParams = null;
            _turns.Clear();
        }

        public void Record()
        {
            Clear();

            _client.TurnApplied -= OnTurnApplied;
            _client.TurnApplied += OnTurnApplied;
        }

        public void Replay()
        {
            if(_config == null)
            {
                _config = new LockstepConfig();
            }
            if(_gameParams == null)
            {
                _gameParams = new LockstepGameParams();
            }

            _client.Config = _config;
            _client.GameParams = _gameParams;

            var itr = GetTurnsEnumerator();
            while(itr.MoveNext())
            {
                _client.AddConfirmedTurn(itr.Current);
            }
            itr.Dispose();
        }

        public IEnumerator<ClientTurnData> GetTurnsEnumerator()
        {
            var t = 0;
            var itr = _turns.GetEnumerator();
            while(itr.MoveNext())
            {
                while(t < itr.Current.Key)
                {
                    yield return ClientTurnData.Empty;
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

        public void AddTurn(int num, ClientTurnData turn)
        {
            _turns[num] = turn;
        }

        public void AddCommand(int turnNum, ClientCommandData data)
        {
            ClientTurnData turn;
            if(!_turns.TryGetValue(turnNum, out turn))
            {
                turn = new ClientTurnData();
                _turns[turnNum] = turn;
            }
            turn.AddCommand(data);
        }

        void OnTurnApplied(ClientTurnData turn, int processTime)
        {
            if(_config == null)
            {
                _config = _client.Config;
            }
            if(_gameParams == null)
            {
                _gameParams = _client.GameParams;
            }
            if(!ClientTurnData.IsNullOrEmpty(turn))
            {
                _turns[_client.CurrentTurnNumber] = turn;
            }
        }

        public void Serialize(IWriter writer)
        {
            if(_config == null || _gameParams == null)
            {
                return;
            }
            _config.Serialize(writer);
            _gameParams.Serialize(writer);

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

            _gameParams = new LockstepGameParams();
            _gameParams.Deserialize(reader);

            int count = reader.ReadInt32();
            for(int i = 0; i < count; ++i)
            {
                var num = reader.ReadInt32();
                var turn = new ClientTurnData();
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