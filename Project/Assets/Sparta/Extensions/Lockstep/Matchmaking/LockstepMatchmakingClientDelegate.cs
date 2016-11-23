﻿using SocialPoint.Base;
using SocialPoint.Attributes;
using SocialPoint.Lockstep;
using System;

namespace SocialPoint.Matchmaking
{
    public class LockstepMatchmakingClientDelegate : IMatchmakingClientDelegate, IDisposable
    {
        LockstepNetworkClient _lockstep;
        IMatchmakingClient _matchmaking;

        public LockstepMatchmakingClientDelegate(LockstepNetworkClient lockstep, IMatchmakingClient matchmaking)
        {
            _lockstep = lockstep;
            _lockstep.PlayerFinishSent += OnPlayerFinishSent;
            _matchmaking = matchmaking;
        }

        public void Dispose()
        {
            _lockstep.PlayerFinishSent -= OnPlayerFinishSent;
        }

        public void OnWaiting(int waitTime)
        {
        }

        public void OnMatched(Match match)
        {
            _lockstep.PlayerId = match.PlayerId;
        }

        public void OnError(Error err)
        {
        }

        void OnPlayerFinishSent(Attr data)
        {
            _matchmaking.Clear();
        }
    }
}
