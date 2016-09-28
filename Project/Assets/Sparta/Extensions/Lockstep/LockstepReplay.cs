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
        List<ClientLockstepTurnData> _turns;
        LockstepConfig _config;

        public LockstepReplay(ClientLockstepController clientLockstep, LockstepCommandFactory commandFactory)
        {
            _turns = new List<ClientLockstepTurnData>();
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
            for(var i = 0; i < _turns.Count; i++)
            {
                _clientLockstep.AddConfirmedTurn(_turns[i]);
            }
        }

        public void Reset()
        {
            _turns.Clear();
            _config = null;
        }

        void OnCommandApplied(ClientLockstepCommandData command)
        {
            if(_config == null)
            {
                _config = _clientLockstep.Config;
            }
            if(_turns.Count > 0)
            {
                _turns[_turns.Count - 1].AddCommand(command);
            }
        }

        public void Serialize(IWriter writer)
        {
            if(_config == null)
            {
                return;
            }
            _config.Serialize(writer);
            writer.Write(_turns.Count);
            for(var i=0; i<_turns.Count; i++)
            {
                _turns[i].Serialize(_commandFactory, writer);
            }
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
                _turns.Add(turn);
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