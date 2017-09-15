using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Social
{
    public class PlayersRanking : IEnumerable<SocialPlayer>
    {
        public int PlayerPosition{ get; private set; }

        public SocialPlayer LocalPlayer{ get; private set; }

        readonly List<SocialPlayer> _players;

        public PlayersRanking()
        {
            _players = new List<SocialPlayer>();
            PlayerPosition = -1;
        }

        public void Add(SocialPlayer player)
        {
            _players.Add(player);
        }

        public void InsertLocalPlayerInRanking(SocialPlayer localPlayer, int position)
        {
            PlayerPosition = position;
            LocalPlayer = localPlayer;
           
            if(position >= _players.Count)
            {
                _players.Add(localPlayer);
            }
            else
            {
                _players.Insert(position, localPlayer);
            }
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
}