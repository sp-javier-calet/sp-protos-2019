
using SocialPoint.Login;
using SocialPoint.Base;
using SocialPoint.Attributes;
using System;

namespace SocialPoint.Matchmaking
{
    [System.Serializable]
    public class Match
    {
        const string MatchIdAttrKey = "match_id";
        const string PlayerIdAttrKey = "token";
        const string GameInfoAttrKey = "game_info";
        const string ServerInfoAttrKey = "server";

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

        public void ParseAttrDic(AttrDic matchData)
        {
            Id = matchData.GetValue(MatchIdAttrKey).ToString();
            PlayerId = matchData.GetValue(PlayerIdAttrKey).ToString();
            Running = true;
            GameInfo = matchData.Get(GameInfoAttrKey);
            ServerInfo = matchData.Get(ServerInfoAttrKey);
        }

        public AttrDic ToAttrDic()
        {
            var attrDic = new AttrDic();
            attrDic.SetValue(MatchIdAttrKey, Id);
            attrDic.SetValue(PlayerIdAttrKey, PlayerId);
            attrDic.Set(GameInfoAttrKey, GameInfo);
            attrDic.Set(ServerInfoAttrKey, ServerInfo);

            return attrDic;
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
