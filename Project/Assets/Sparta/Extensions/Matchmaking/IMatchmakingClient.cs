
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
        public bool Running;
        public Attr GameInfo;
        public Attr ServerInfo;

        [Obsolete("Use GameInfo instead")]
        public Attr Info
        {
            get
            {
                return GameInfo;
            }
        }

        public override string ToString()
        {
            return string.Format("[Match:{0}\n" +
                "PlayerId={1} Running={2}\n" +
                "Game={3} Server={4}]",
                Id, PlayerId, Running, GameInfo, ServerInfo);
        }
    }

    public interface IMatchmakingClientDelegate
    {
        void OnWaiting(int waitTime);
        void OnMatched(Match match);
        void OnError(Error err);
    }

    public static class MatchmakingClientErrorCode
    {
        public const int Timeout = 301;
    }

    public interface IMatchmakingClient
    {
        string Room{ get; set; }

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