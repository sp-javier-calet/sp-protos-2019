using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Lockstep.Network;

namespace SocialPoint.Lockstep
{
    public class LockstepReplay : IDisposable, INetworkShareable
    {
        ClientLockstepController _clientLockstep;
        LockstepCommandFactory _commandFactory;
        Dictionary<int, ClientLockstepTurnData> _turns;
        LockstepConfig _config;

        public LockstepReplay(ClientLockstepController clientLockstep, LockstepCommandFactory commandFactory)
        {
            _turns = new Dictionary<int, ClientLockstepTurnData>();
            _clientLockstep = clientLockstep;
            _commandFactory = commandFactory;
        }

        public void Record()
        {
            _config = null;
            _clientLockstep.CommandApplied += OnCommandApplied;
        }

        public void Replay()
        {
            _clientLockstep.Init(_config);
            var itr = _turns.GetEnumerator();
            while(itr.MoveNext())
            {
                _clientLockstep.ConfirmTurn(itr.Current.Value);
            }
            itr.Dispose();
        }

        public void Reset()
        {
            _turns.Clear();
            _config = null;
        }

        void OnCommandApplied(ClientLockstepCommandData command, int turn)
        {
            if(_config == null)
            {
                _config = _clientLockstep.Config;
            }
            ClientLockstepTurnData turnData;
            if(!_turns.TryGetValue(turn, out turnData))
            {
                turnData = new ClientLockstepTurnData(turn);
                _turns[turn] = turnData;
            }
            turnData.Commands.Add(command);
        }

        public void Serialize(IWriter writer)
        {
            if(_config == null)
            {
                return;
            }
            _config.Serialize(writer);
            writer.Write(_turns.Count);
            var itr = _turns.GetEnumerator();
            while(itr.MoveNext())
            {
                itr.Current.Value.Serialize(_commandFactory, writer);
            }
            itr.Dispose();
        }

        public void Deserialize(IReader reader)
        {
            _config = new LockstepConfig();
            _config.Deserialize(reader);
            int count = reader.ReadInt32();
            for(int i = 0; i < count; ++i)
            {
                var turn = new ClientLockstepTurnData();
                turn.Deserialize(_commandFactory, reader);
                _turns[turn.Turn] = turn;
            }
        }

        public void Dispose()
        {
            if(_clientLockstep != null)
            {
                _clientLockstep.CommandApplied -= OnCommandApplied;
            }
        }
    }
}