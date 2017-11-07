using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Social
{
    public class AlliancesSearch
    {
        public string Filter;
    }

    public class AlliancesSearchResult : IEnumerable<AllianceBasicData>
    {
        public int Score;

        readonly List<AllianceBasicData> _searchData;

        public AlliancesSearchResult()
        {
            _searchData = new List<AllianceBasicData>();
        }

        public void Add(AllianceBasicData data)
        {
            _searchData.Add(data);
        }

        [System.Obsolete("Use GetEnumerator() instead")]
        public List<AllianceBasicData> GetList()
        {
            return _searchData;
        }

        #region IEnumerable implementation

        public IEnumerator<AllianceBasicData> GetEnumerator()
        {
            return _searchData.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _searchData.GetEnumerator();
        }

        #endregion

        public IEnumerator<T> GetEnumeratorAs<T>() where T : AllianceBasicData
        {
            var itr = GetEnumerator();
            while(itr.MoveNext())
            {
                var elm = itr.Current;
                yield return (T)elm;

            }
            itr.Dispose();
        }

        public int Count
        {
            get
            {
                return _searchData.Count;
            }
        }
    }
}