using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public static class AssetBundlesDependencies
    {
        public static void AddAssetToBundle(string assetPath)
        {
            DependenciesManifest.AddAssetsWithDependencies(AssetDatabase.AssetPathToGUID(assetPath));

            DependenciesManifest.Save();
        }

        public static void RefreshAllDependencies()
        {
            DependenciesManifest.RefreshAll();

            DependenciesManifest.Save();
        }

        public static void RemoveManualBundle(string assetPath)
        {
            DependenciesManifest.RemoveAsset(AssetDatabase.AssetPathToGUID(assetPath));

            DependenciesManifest.Save();
        }
    }
}
