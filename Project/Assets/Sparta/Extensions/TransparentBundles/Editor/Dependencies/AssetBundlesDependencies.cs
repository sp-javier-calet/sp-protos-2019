using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public static class AssetBundlesDependencies
    {
        public static void AddAssetToBundle(string assetPath, string bundleName)
        {
            var importer = AssetImporter.GetAtPath(assetPath);

            importer.assetBundleName = bundleName;

            DependenciesManifest.AddAssetsWithDependencies(AssetDatabase.AssetPathToGUID(assetPath), bundleName);

            DependenciesManifest.Save();
        }

        public static void RemoveBundleFromAsset(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath);

            importer.assetBundleName = "";

            DependenciesManifest.RemoveAssetWithDependencies(AssetDatabase.AssetPathToGUID(assetPath));

            DependenciesManifest.Save();
        }
    }
}
