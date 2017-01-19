using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public class PlayersDataFactory
    {
        #region Attr keys

        const string RankingKey = "ranking";
        const string RankingRankKey = "my_rank";
        const string RankingMeKey = "me";

        #endregion

        public SocialPlayerFactory PlayerFactory{ protected get; set; }

        public PlayersRanking CreateRankingData(AttrDic dic)
        {
            var ranking = CreateCustomRanking();
            ParseRankingData(ranking, dic);
            ParseCustomRanking(ranking, dic);
            return ranking;
        }

        public void ParseRankingData(PlayersRanking ranking, AttrDic dic)
        {
            var rankDir = dic.Get(RankingKey).AsDic;
            var itr = rankDir.GetEnumerator();
            while(itr.MoveNext())
            {
                var info = PlayerFactory.CreateSocialPlayer(itr.Current.Value.AsDic);
                ranking.Add(info);
            }
            itr.Dispose();

            if(dic.ContainsKey(RankingMeKey) && dic.ContainsKey(RankingRankKey))
            {
                int myRank = dic.GetValue(RankingRankKey).ToInt();
                var myData = PlayerFactory.CreateSocialPlayer(dic.Get(RankingMeKey).AsDic);
                ranking.InsertLocalPlayerInRanking(myData, myRank);
            }
        }

        #region Extensible methods

        protected virtual PlayersRanking CreateCustomRanking()
        {
            return new PlayersRanking();
        }

        protected virtual void ParseCustomRanking(PlayersRanking ranking, AttrDic dic)
        {
        }

        #endregion
    }
}
