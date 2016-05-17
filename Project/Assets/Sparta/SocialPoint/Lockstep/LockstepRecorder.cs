using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SocialPoint.Utils;
using System.IO;
using UnityEngine.Networking;
using SocialPoint.Lockstep.Network;

namespace SocialPoint.Lockstep
{
    public class LockstepRecorder : IDisposable
    {
        ClientLockstepController _clientLockstep;
        NetworkLockstepCommandDataFactory _commandDataFactory;
        List<ILockstepCommand> _recordedCommands;

        public LockstepRecorder(ClientLockstepController clientLockstep, NetworkLockstepCommandDataFactory commandDataFactory)
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

        public void Serialize(NetworkWriter networkWriter)
        {
            SetLockstepConfigMessage configMessage = new SetLockstepConfigMessage(_clientLockstep.LockstepConfig);
            configMessage.Serialize(networkWriter);
            networkWriter.Write(_recordedCommands.Count);
            for(int i = 0; i < _recordedCommands.Count; ++i)
            {
                var command = _recordedCommands[i];
                networkWriter.Write(command.Turn);
                _commandDataFactory.CreateNetworkLockstepCommandData(command).Serialize(networkWriter);
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