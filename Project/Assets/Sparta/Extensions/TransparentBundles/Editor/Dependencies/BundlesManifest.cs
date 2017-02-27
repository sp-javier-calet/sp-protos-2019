using System.Collections.Generic;
using System.IO;
using LitJson;

namespace SocialPoint.TransparentBundles
{
    public class BundlesManifest
    {
        Dictionary<string, BundleDependenciesData> _dictionary = new Dictionary<string, BundleDependenciesData>();

        #region Getters_Setters

        public BundleDependenciesData this[string guid]
        {
            get
            {
                return _dictionary[guid];
            }
            set
            {
                _dictionary[guid] = value;
            }
        }

        public IEnumerable<BundleDependenciesData> GetValues()
        {
            return _dictionary.Values;
        }


        public IEnumerable<string> GetKeys()
        {
            return _dictionary.Keys;
        }

        public void Add(string guid, BundleDependenciesData bundleData)
        {
            _dictionary.Add(guid, bundleData);
        }

        public bool Remove(string guid)
        {
            return _dictionary.Remove(guid);
        }

        public Dictionary<string, BundleDependenciesData> GetDictionary()
        {
            return _dictionary;
        }

        public void SetDictionary(Dictionary<string, BundleDependenciesData> manifest)
        {
            _dictionary = manifest;
        }

        #endregion

        public BundlesManifest()
        {
        }

        public BundlesManifest(Dictionary<string, BundleDependenciesData> dictionary)
        {
            _dictionary = dictionary;
        }


        public static BundlesManifest Load(string path)
        {
            var bManifest = new BundlesManifest();
            bManifest._dictionary = JsonMapper.ToObject<Dictionary<string, BundleDependenciesData>>(File.ReadAllText(path));

            return bManifest;
        }

        public void Save(string path)
        {
            var writer = new JsonWriter();
            writer.PrettyPrint = true;
            JsonMapper.ToJson(_dictionary, writer);
            var str = writer.ToString();
            File.WriteAllText(path, str);
        }

        #region Methods

        /// <summary>
        /// Gets the Bundle Dependency Data stored for this asset as **VALUE**
        /// </summary>
        /// <param name="GUID">GUID of the asset to search</param>
        /// <returns>BundleDependenciesData if the asset is in the manifest and null if it isn't</returns>
        public BundleDependenciesData GetBundleDependencyDataCopy(string GUID)
        {
            return _dictionary.ContainsKey(GUID) ? (BundleDependenciesData)_dictionary[GUID].Clone() : null;
        }

        /// <summary>
        /// Checks if the asset is registered in the manifest
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        public bool HasAsset(string GUID)
        {
            return _dictionary.ContainsKey(GUID);
        }

        /// <summary>
        /// Returns a copy of all the bundles that are userBundled as **VALUE**
        /// </summary>
        /// <returns>Copy of all the User Bundles</returns>
        public List<BundleDependenciesData> GetUserBundlesCopy()
        {
            var userBundles = new List<BundleDependenciesData>();
            foreach(var pair in _dictionary)
            {
                if(pair.Value.IsExplicitlyBundled)
                {
                    userBundles.Add((BundleDependenciesData)pair.Value.Clone());
                }
            }

            return userBundles;
        }

        #endregion
    }
}
