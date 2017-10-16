using SocialPoint.Base;
using SocialPoint.Attributes;

namespace SocialPoint.Matchmaking
{
    public interface IMatchmakingClientDelegate
    {
        void OnStart();

        void OnSearchOpponent();

        void OnWaiting(int waitTime);

        void OnMatched(Match match);

        void OnStopped(bool successful);

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
        void Start(AttrDic extraData, bool searchForActiveMatch, string connectId);

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
