using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Social
{
    public class AlliancesRanking : IEnumerable<AllianceBasicData>
    {
        public int Score;

        public int PlayerAlliancePosition;

        public AllianceBasicData PlayerAllianceData;

        readonly List<AllianceBasicData> _rankingData;

        public AlliancesRanking()
        {
            _rankingData = new List<AllianceBasicData>();
        }

        public void Add(AllianceBasicData data)
        {
            _rankingData.Add(data);
        }

        [System.Obsolete("Use GetEnumerator() instead")]
        public List<AllianceBasicData> GetList()
        {
            return _rankingData;
        }

        #region IEnumerable implementation

        public IEnumerator<AllianceBasicData> GetEnumerator()
        {
            return _rankingData.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _rankingData.GetEnumerator();
        }

        #endregion
    }
}