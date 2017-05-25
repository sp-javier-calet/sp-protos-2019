
using SocialPoint.Attributes;
using SocialPoint.Base;
using System.Collections.Generic;
using System;

namespace SocialPoint.Matchmaking
{
    public interface IMatchmakingServerDelegate
    {
        void OnMatchInfoReceived(byte[] matchInfo);

        void OnResultsReceived(AttrDic result);

        void OnError(Error err);
    }

    public interface IMatchmakingServer
    {
        // should return false if not correctly configured
        bool Enabled { get; }

        // should set the server version
        string Version { get; set; }

        void AddDelegate(IMatchmakingServerDelegate dlg);

        void RemoveDelegate(IMatchmakingServerDelegate dlg);

        // called when the match starts, callback should be called with match info
        void LoadInfo(string matchId, List<string> playerIds);

        // called when the match ends, results keys should be unique playerIds
        void NotifyResults(string matchId, AttrDic results, AttrDic customData);

        //TODO: Fix this. It was added to decouple an specific IMatchmakingServerDelegate and IMatchmakingServer implementation from a game's backport
        byte[] GetLastNotificationBody();

        //TODO: Fix this. It was added to decouple an specific IMatchmakingServerDelegate and IMatchmakingServer implementation from a game's backport
        AttrDic GetLastNotificationBodyParams();

        //TODO: Fix this. It was added to decouple an specific IMatchmakingServerDelegate and IMatchmakingServer implementation from a game's backport
        AttrDic GetLastNotificationParams();
    }
}
