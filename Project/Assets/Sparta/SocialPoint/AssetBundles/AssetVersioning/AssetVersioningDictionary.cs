using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.AssetVersioning
{
    public class AssetVersioningData
    {
		public string Client;
        public int Version;
        public bool IsLocal;
        public string Parent;
        public uint CRC;
    }
    public class AssetVersioningDictionary : IDictionary<string, AssetVersioningData>
    {

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
                if (!_data.ContainsKey(key))
                {
                    AssetVersioningData data = new AssetVersioningData();
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

        bool IsThumb(string key)
        {
            return StringUtils.EndsWith(key, "_portrait") || StringUtils.EndsWith(key, "_thumb");
        }

        public List<string> GetLocalBundles()
        {
            List<string> result = new List<string>();
			foreach(KeyValuePair<string, AssetVersioningData> pair in _data)
            {
                if( DownloadManager.Instance.IsLocalBundleVersion(pair.Key, pair.Value.Version, pair.Value.Client) && !IsThumb(pair.Key))
                {
                    result.Add(pair.Key);
                }
            }

            return result;
        }

        public List<string> GetLocalTextureNames()
        {
            List<string> result = new List<string>();
			foreach(KeyValuePair<string, AssetVersioningData> pair in _data)
            {
                if( DownloadManager.Instance.IsLocalBundleVersion(pair.Key, pair.Value.Version, pair.Value.Client) && IsThumb(pair.Key))
                {
                    result.Add(pair.Key.Replace("/GUI/Thumbnails/", ""));
                }
            }
            
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

            foreach(AssetVersioningData e in this.Values)
            {
                sb.AppendFormat("\tId: {0}", e).AppendLine();
                sb.AppendFormat("\t{0}", e.ToString()).AppendLine();
            }

            return sb.ToString();
        }
    }
}
