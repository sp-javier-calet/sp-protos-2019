using System;
using System.Collections.Generic;
using UnityEditor;

namespace SocialPoint.TransparentBundles
{
    public class BundleDependenciesData : ICloneable
    {
        public string GUID;
        public string AssetPath;
        public string BundleName;
        public List<string> Dependants = new List<string>();
        public List<string> Dependencies = new List<string>();
        public bool IsLocal;
        public bool IsExplicitlyBundled;

        public BundleDependenciesData()
        {
        }

        public BundleDependenciesData(string GUID)
        {
            this.GUID = GUID;
            AssetPath = AssetDatabase.GUIDToAssetPath(GUID);
        }


        public object Clone()
        {
            var clone = new BundleDependenciesData(GUID);
            clone.BundleName = BundleName;
            clone.Dependants = new List<string>(Dependants);
            clone.Dependencies = new List<string>(Dependencies);
            clone.IsLocal = IsLocal;
            clone.IsExplicitlyBundled = IsExplicitlyBundled;

            return clone;
        }
    }
}
