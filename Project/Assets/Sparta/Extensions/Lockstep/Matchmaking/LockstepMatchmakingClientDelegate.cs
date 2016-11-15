using SocialPoint.Lockstep.Network;
using SocialPoint.Base;
using SocialPoint.Attributes;
using System;

namespace SocialPoint.Matchmaking
{
    public class LockstepMatchmakingClientDelegate : IMatchmakingClientDelegate, IDisposable
    {
        ClientLockstepNetworkController _lockstep;
        IMatchmakingClientController _matchmaking;

        public LockstepMatchmakingClientDelegate(ClientLockstepNetworkController lockstep, IMatchmakingClientController matchmaking)
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
