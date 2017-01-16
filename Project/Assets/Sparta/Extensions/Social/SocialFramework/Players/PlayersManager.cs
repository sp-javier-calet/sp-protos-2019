using System;
using SocialPoint.Base;
using SocialPoint.Social;
using SocialPoint.WAMP;
using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public sealed class PlayersManager
    {
        #region Attr keys

        const string UserIdKey = "user_id";

        const string OperationResultKey = "result";

        #endregion

        #region RPC methods

        const string PlayersRankinsMethod = "players.ranking";

        #endregion

        readonly ConnectionManager _connection;
        readonly SocialManager _socialManager;

        PlayersDataFactory _factory;

        public PlayersDataFactory Factory
        { 
            private get
            {
                return _factory;
            }
            set
            {
                _factory = value;
                _factory.PlayerFactory = _socialManager.PlayerFactory;
            }
        }

        public PlayersManager(ConnectionManager connection, SocialManager socialManager)
        {
            _connection = connection;
            _socialManager = socialManager;
        }

        public WAMPRequest LoadPlayersRanking(Action<Error, PlayersRanking> callback)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, _socialManager.LocalPlayer.Uid);

            return _connection.Call(PlayersRankinsMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
                PlayersRanking ranking = null;
                if(Error.IsNullOrEmpty(err))
                {
                    DebugUtils.Assert(rDic.Get(OperationResultKey).IsDic);
                    var result = rDic.Get(OperationResultKey).AsDic;
                    ranking = Factory.CreateRankingData(result);
                }
                if(callback != null)
                {
                    callback(err, ranking);
                }
            });
        }
    }
}