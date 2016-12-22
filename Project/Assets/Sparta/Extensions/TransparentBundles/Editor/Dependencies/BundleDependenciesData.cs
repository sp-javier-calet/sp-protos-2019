using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class BundleDependenciesData
    {
        public string assetPath;
        public string bundleName = "";
        public List<string> dependencies = null;
        public bool IsExplicitlyBundled
        {
            get
            {
                return !string.IsNullOrEmpty(bundleName);
            }
            private set { }
        }
    }
}
