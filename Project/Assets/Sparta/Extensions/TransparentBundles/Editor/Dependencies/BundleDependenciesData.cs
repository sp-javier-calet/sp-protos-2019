using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace SocialPoint.TransparentBundles
{
    public class BundleDependenciesData : ICloneable
    {
        public string AssetPath;
        public string BundleName = "";
        public List<string> Dependants = new List<string>();
        public List<string> Dependencies = new List<string>();
        public bool IsExplicitlyBundled;

        public object Clone()
        {
            var clone =  new BundleDependenciesData();
            clone.AssetPath = AssetPath;
            clone.BundleName = BundleName;
            clone.Dependants = new List<string>(Dependants);
            clone.Dependencies = new List<string>(Dependencies);
            clone.IsExplicitlyBundled = IsExplicitlyBundled;

            return clone;
        }
    }
}
