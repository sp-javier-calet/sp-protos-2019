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
        List<LockstepCommandData> _commands;
        LockstepConfig _config;

        public LockstepReplay(ClientLockstepController clientLockstep, LockstepCommandFactory commandFactory)
        {
            _commands = new List<LockstepCommandData>();
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
            for(var i = 0; i < _commands.Count; i++)
            {
                _clientLockstep.AddConfirmedCommand(_commands[i]);
            }
        }

        public void Reset()
        {
            _commands.Clear();
            _config = null;
        }

        void OnCommandApplied(LockstepCommandData command)
        {
            if(_config == null)
            {
                _config = _clientLockstep.LockstepConfig;
            }
            _commands.Add(command);
        }

        public void Serialize(IWriter writer)
        {
            if(_config == null)
            {
                return;
            }
            _config.Serialize(writer);
            writer.Write(_commands.Count);
            for(int i = 0; i < _commands.Count; ++i)
            {
                var command = _commands[i];
                command.Serialize(_commandFactory, writer);
            }
        }

        public void Deserialize(IReader reader)
        {
            _config = new LockstepConfig();
            _config.Deserialize(reader);
            int count = reader.ReadInt32();
            for(int i = 0; i < count; ++i)
            {
                var cmd = new LockstepCommandData();
                cmd.Deserialize(_commandFactory, reader);
                _commands.Add(cmd);
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