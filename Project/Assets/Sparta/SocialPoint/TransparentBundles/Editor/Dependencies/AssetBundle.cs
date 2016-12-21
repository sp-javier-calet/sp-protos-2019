using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class AssetBundle {
    
    public static void AddAssetToBundle(string assetPath, string bundleName)
    {
        DependenciesManifest manifest = DependenciesManifest.GetInstance();
        var importer = AssetImporter.GetAtPath(assetPath);

        importer.assetBundleName = bundleName;

        manifest.AddAssetsWithDependencies(AssetDatabase.AssetPathToGUID(assetPath), bundleName);

        manifest.Save();
    }

    public static void RemoveBundleFromAsset(string assetPath)
    {
        DependenciesManifest manifest = DependenciesManifest.GetInstance();
        var importer = AssetImporter.GetAtPath(assetPath);

        importer.assetBundleName = "";

        manifest.RemoveAssetWithDependencies(AssetDatabase.AssetPathToGUID(assetPath));

        manifest.Save();
    }



    [MenuItem("Assets/Assign Bundle 1")]
    public static void AddAssetsToBundle()
    {

        Object[] objects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);


        foreach(Object obj in objects)
        {
            AddAssetToBundle(AssetDatabase.GetAssetPath(obj), "bundle");
        }
    }



    [MenuItem("Assets/Assign Bundle 2")]
    public static void AddAssetsToBundle2()
    {
        Object[] objects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);


        foreach(Object obj in objects)
        {
            AddAssetToBundle(AssetDatabase.GetAssetPath(obj), "bundle2");
        }
    }



    [MenuItem("Assets/Assign Bundle 3")]
    public static void AddAssetsToBundle3()
    {
        Object[] objects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);


        foreach(Object obj in objects)
        {
            AddAssetToBundle(AssetDatabase.GetAssetPath(obj), "bundle3");
        }
    }

    [MenuItem("Assets/Remove from bundle")]
    public static void remove()
    {
        Object[] objects = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);


        foreach(Object obj in objects)
        {
            RemoveBundleFromAsset(AssetDatabase.GetAssetPath(obj));
        }
    }

    [MenuItem("Assets/BuildBundles")]
    public static void BuildBundles()
    {
        BuildPipeline.BuildAssetBundles("Assets/Output/", BuildAssetBundleOptions.DeterministicAssetBundle, BuildTarget.StandaloneWindows64);
    }

}
