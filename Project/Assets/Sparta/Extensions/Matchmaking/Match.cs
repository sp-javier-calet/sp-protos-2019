using SocialPoint.Attributes;
using System;

namespace SocialPoint.Matchmaking
{
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
            Running = false;
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
}
