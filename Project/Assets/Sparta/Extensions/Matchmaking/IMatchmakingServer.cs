
using SocialPoint.Attributes;
using SocialPoint.Base;
using System.Collections.Generic;
using System;

namespace SocialPoint.Matchmaking
{
    public interface IMatchmakingServerDelegate
    {
        void OnMatchInfoReceived(Attr matchInfo);
        void OnError(Error err);
    }

    public interface IMatchmakingServer
    {
        void AddDelegate(IMatchmakingServerDelegate dlg);
        void RemoveDelegate(IMatchmakingServerDelegate dlg);

        // called when the match starts, callback should be called with match info
        void LoadInfo(string matchId, List<string> playerIds);

        // called when the match ends, results keys should be unique playerIds
        void NotifyResult(string matchId, AttrDic result);
    }
}
