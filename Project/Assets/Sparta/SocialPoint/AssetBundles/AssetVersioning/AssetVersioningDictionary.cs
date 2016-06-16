using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.AssetVersioning
{
    public class AssetVersioningDictionary : IAssetVersioningDictionary
    {
        const string kPortraitSuffix = "_portrait";
        const string kThumbSuffix = "_thumb";

        #region Initialization

        public AssetVersioningDictionary()
        {
            _data = new Dictionary<string, AssetVersioningData>();
        }

        #endregion

        #region Memento Pattern

        internal Dictionary<string, AssetVersioningData> GetInternalData()
        {
            return _data;
        }

        internal virtual void SetInternalData(Dictionary<string, AssetVersioningData> orig)
        {
            _data = orig;
        }

        #endregion

        #region IDictionary implementation

        public bool ContainsKey(string key)
        {
            return _data.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _data.Remove(key);
        }

        public bool TryGetValue(string key, out AssetVersioningData value)
        {
            return _data.TryGetValue(key, out value);
        }

        public AssetVersioningData this[string key]
        {
            get
            {
#if UNITY_EDITOR
//                if (!_data.ContainsKey(key))
//                    UnityEngine.Debug.Log("Key not found: " + key);
#endif
                if(!_data.ContainsKey(key))
                {
                    var data = new AssetVersioningData();
                    data.Version = 1;
                    data.Client = DownloadManager.SpamData.Instance.client;
                    return data;
                }

                return _data[key];
            }
            set
            {
                _data[key] = value;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return _data.Keys;
            }
        }

        public ICollection<AssetVersioningData> Values
        {
            get
            {
                return _data.Values;
            }
        }

        #endregion

        #region ICollection implementation

        public void Add(string key, AssetVersioningData value)
        {
            _data.Add(key, value);
        }

        public void Add(KeyValuePair<string, AssetVersioningData> item)
        {
            Add(item);
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(KeyValuePair<string, AssetVersioningData> item)
        {
            return _data.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<string, AssetVersioningData>[] array, int arrayIndex)
        {

        }

        public bool Remove(KeyValuePair<string, AssetVersioningData> item)
        {
            return Remove(item.Key);
        }

        public int Count
        {
            get
            {
                return _data.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        static bool IsThumb(string key)
        {
            return StringUtils.EndsWith(key, kPortraitSuffix) || StringUtils.EndsWith(key, kThumbSuffix);
        }

        public IList<string> GetLocalBundles()
        {
            var result = new List<string>();
            var itr = _data.GetEnumerator();
            while(itr.MoveNext())
            {
                var pair = itr.Current;
                if(DownloadManager.Instance.IsLocalBundleVersion(pair.Key, pair.Value.Version, pair.Value.Client) && !IsThumb(pair.Key))
                {
                    result.Add(pair.Key);
                }
            }
            itr.Dispose();

            return result;
        }

        public IList<string> GetLocalTextureNames()
        {
            var result = new List<string>();
            var itr = _data.GetEnumerator();
            while(itr.MoveNext())
            {
                var pair = itr.Current;
                if(DownloadManager.Instance.IsLocalBundleVersion(pair.Key, pair.Value.Version, pair.Value.Client) && IsThumb(pair.Key))
                {
                    result.Add(pair.Key.Replace("/GUI/Thumbnails/", ""));
                }
            }
            itr.Dispose();
            
            return result;
        }

        #endregion

        #region IEnumerable implementation

        public IEnumerator<KeyValuePair<string, AssetVersioningData>> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        #endregion

        #region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Private data

        Dictionary<string, AssetVersioningData> _data;

        #endregion

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendFormat("EntityDictionary: {0}", GetType().Name).AppendLine();

            var itr = Values.GetEnumerator();
            while(itr.MoveNext())
            {
                var e = itr.Current;
                sb.AppendFormat("\tId: {0}", e).AppendLine();
                sb.AppendFormat("\t{0}", e).AppendLine();
            }
            itr.Dispose();

            return sb.ToString();
        }
    }
}
