
using SocialPoint.Login;
using SocialPoint.Base;
using SocialPoint.Attributes;
using System;

namespace SocialPoint.Matchmaking
{
    public struct Match
    {
        public string Id;
        public string PlayerId;
        public Attr Info;
    }

    public interface IMatchmakingClientDelegate
    {
        void OnWaiting(int waitTime);
        void OnMatched(Match match);
        void OnError(Error err);
    }

    public interface IMatchmakingClient
    {
        void AddDelegate(IMatchmakingClientDelegate dlg);
        void RemoveDelegate(IMatchmakingClientDelegate dlg);

        /**
         * should look for a match and call OnMatched or OnError
         */
        void Start();

        /**
         * should stop looking for a match
         */
        void Stop();

        /**
         * should be called if the match finishes or has an error
         * matchmaking should clear the cache in this case
         */
        void Clear();
    }
}