
using SocialPoint.Attributes;
using SocialPoint.Base;
using System.Collections.Generic;
using System;
using SocialPoint.Network;

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

        AttrDic ClientsVersions { get; set; }

        //TODO: Fix backport. This getter is only used to do some logs in LockstepNetworkServer, it should be refactored in a cleaner way
        HttpRequest InfoRequest { get; }

        //TODO: Fix backport. This getter is only used to do some logs in LockstepNetworkServer, it should be refactored in a cleaner way
        HttpRequest NotifyRequest { get; }

        //TODO: Fix backport. This getter is only used to do some logs in LockstepNetworkServer, it should be refactored in a cleaner way
        HttpResponse InfoResponse { get; }

        //TODO: Fix backport. This getter is only used to do some logs in LockstepNetworkServer, it should be refactored in a cleaner way
        HttpResponse NotifyResponse { get; }

        void AddDelegate(IMatchmakingServerDelegate dlg);

        void RemoveDelegate(IMatchmakingServerDelegate dlg);

        // called when the match starts, callback should be called with match info
        void LoadInfo(string matchId, List<string> playerIds);

        // called when the match ends, results keys should be unique playerIds
        void NotifyResults(string matchId, AttrDic results, AttrDic customData);
    }
}
