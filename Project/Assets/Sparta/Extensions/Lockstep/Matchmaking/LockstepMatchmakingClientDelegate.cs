using SocialPoint.Base;
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

        public void OnMatched(Match match, bool reconnect)
        {
            _lockstep.PlayerId = match.PlayerId;
            _lockstep.MatchId = match.Id;
            if(_lockstep.SendTrack != null)
            {
                var data = new AttrDic();
                data.SetValue("match_id", match.Id);
                data.SetValue("player_id", match.PlayerId);
                data.SetValue("reconnect", reconnect);
                _lockstep.SendTrack("log_battle_match", data, null);
            }
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
