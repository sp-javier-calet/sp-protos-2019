using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class BundleDependenciesData
    {
        public string AssetPath;
        public string BundleName = "";
        public List<string> Dependencies = null;
        public bool IsExplicitlyBundled
        {
            get
            {
                return !string.IsNullOrEmpty(BundleName);
            }
            private set { }
        }
    }
}
