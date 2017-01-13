using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Social;
using SocialPoint.WAMP;
using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public class SocialManager
    {
        public class PlayersRanking : IEnumerable<SocialPlayer>
        {
            public int PlayerPosition;
            readonly List<SocialPlayer> _players;

            public PlayersRanking()
            {
                _players = new List<SocialPlayer>();
            }

            public void Add(SocialPlayer player)
            {
                _players.Add(player);
            }

            #region IEnumerable implementation

            public IEnumerator<SocialPlayer> GetEnumerator()
            {
                return _players.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _players.GetEnumerator();
            }

            #endregion
        }

        #region Attr keys

        const string UserIdKey = "user_id";

        #endregion

        #region RPC methods

        const string PlayersRankinsMethod = "players.ranking";

        #endregion

        readonly ConnectionManager _connection;

        public SocialPlayerFactory PlayerFactory{ get; private set; }

        public SocialPlayer LocalPlayer{ get; private set; }

        public SocialManager(ConnectionManager connection)
        {
            _connection = connection;
            PlayerFactory = new SocialPlayerFactory();
        }

        public void SetLocalPlayerData(AttrDic data)
        {
            LocalPlayer = PlayerFactory.CreateSocialPlayer(data);
        }

        public WAMPRequest LoadPlayersRanking(Action<Error, PlayersRanking> callback)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LocalPlayer.Uid);

            return _connection.Call(PlayersRankinsMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
                PlayersRanking ranking = null;
                if(Error.IsNullOrEmpty(err))
                {
                    //TODO: Create and parse the response
                }
                if(callback != null)
                {
                    callback(err, ranking);
                }
            });
        }
    }
}