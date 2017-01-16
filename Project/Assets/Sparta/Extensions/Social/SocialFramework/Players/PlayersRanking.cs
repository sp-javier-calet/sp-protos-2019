using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Social
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
}