using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class BundleDependenciesData
    {
        public string AssetPath;
        public string BundleName = "";
        public List<string> Dependants = new List<string>();
        public List<string> Dependencies = new List<string>();
        public bool IsExplicitlyBundled;
    }
}
