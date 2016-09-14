using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Lockstep.Network;

namespace SocialPoint.Lockstep
{
    public class LockstepRecorder : IDisposable
    {
        ClientLockstepController _clientLockstep;
        LockstepCommandFactory _commandFactory;
        List<LockstepCommandData> _recordedCommands;

        public LockstepRecorder(ClientLockstepController clientLockstep, LockstepCommandFactory commandFactory)
        {
            _recordedCommands = new List<LockstepCommandData>();
            _clientLockstep = clientLockstep;
            _commandFactory = commandFactory;
            _clientLockstep.CommandApplied += OnCommandApplied;
        }

        void OnCommandApplied(LockstepCommandData command)
        {
            _recordedCommands.Add(command);
        }

        public void Serialize(IWriter writer)
        {
            var setup = new ClientSetupMessage( _clientLockstep.LockstepConfig);
            setup.Serialize(writer);
            writer.Write(_recordedCommands.Count);
            for(int i = 0; i < _recordedCommands.Count; ++i)
            {
                var command = _recordedCommands[i];
                command.Serialize(_commandFactory, writer);
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