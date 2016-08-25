using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SocialPoint.IO;
using System.IO;
using UnityEngine.Networking;
using SocialPoint.Lockstep.Network;

namespace SocialPoint.Lockstep
{
    public class LockstepRecorder : IDisposable
    {
        ClientLockstepController _clientLockstep;
        LockstepCommandDataFactory _commandDataFactory;
        List<ILockstepCommand> _recordedCommands;

        public LockstepRecorder(ClientLockstepController clientLockstep, LockstepCommandDataFactory commandDataFactory)
        {
            _recordedCommands = new List<ILockstepCommand>();
            _clientLockstep = clientLockstep;
            _commandDataFactory = commandDataFactory;
            _clientLockstep.CommandApplied += OnCommandApplied;
        }

        void OnCommandApplied(ILockstepCommand command)
        {
            _recordedCommands.Add(command);
        }

        public void Serialize(IWriter writer)
        {
            SetLockstepConfigMessage configMessage = new SetLockstepConfigMessage(0, _clientLockstep.LockstepConfig);
            configMessage.Serialize(writer);
            writer.Write(_recordedCommands.Count);
            for(int i = 0; i < _recordedCommands.Count; ++i)
            {
                var command = _recordedCommands[i];
                writer.Write(command.Turn);
                _commandDataFactory.CreateNetworkLockstepCommandData(command).Serialize(writer);
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