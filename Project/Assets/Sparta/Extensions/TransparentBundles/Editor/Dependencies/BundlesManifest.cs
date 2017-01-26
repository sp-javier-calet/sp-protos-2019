using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class BundlesManifest
    {
        private Dictionary<string, BundleDependenciesData> _manifest = new Dictionary<string, BundleDependenciesData>();

        #region Getters_Setters

        public BundleDependenciesData this[string guid]
        {
            get
            {
                return _manifest[guid];
            }
            set
            {
                _manifest[guid] = value;
            }
        }

        public IEnumerable<BundleDependenciesData> Values
        {
            get
            {
                return _manifest.Values;
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return _manifest.Keys;
            }
        }

        public void Add(string guid, BundleDependenciesData bundleData)
        {
            _manifest.Add(guid, bundleData);
        }

        public bool Remove(string guid)
        {
            return _manifest.Remove(guid);
        }


        public Dictionary<string, BundleDependenciesData> GetDictionary()
        {
            return _manifest;
        }

        public void SetDictionary(Dictionary<string, BundleDependenciesData> manifest)
        {
            _manifest = manifest;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the Bundle Dependency Data stored for this asset as **VALUE**
        /// </summary>
        /// <param name="GUID">GUID of the asset to search</param>
        /// <returns>BundleDependenciesData if the asset is in the manifest and null if it isn't</returns>
        public BundleDependenciesData GetBundleDependencyDataCopy(string GUID)
        {
            return _manifest.ContainsKey(GUID) ? (BundleDependenciesData)_manifest[GUID].Clone() : null;
        }

        /// <summary>
        /// Checks if the asset is registered in the manifest
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        public bool HasAsset(string GUID)
        {
            return _manifest.ContainsKey(GUID);
        }

        /// <summary>
        /// Returns a copy of all the bundles that are userBundled as **VALUE**
        /// </summary>
        /// <returns>Copy of all the User Bundles</returns>
        public List<BundleDependenciesData> GetUserBundlesCopy()
        {
            List<BundleDependenciesData> userBundles = new List<BundleDependenciesData>();
            foreach(var pair in _manifest)
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
