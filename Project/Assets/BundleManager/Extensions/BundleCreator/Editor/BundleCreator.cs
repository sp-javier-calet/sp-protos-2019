using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class BundleCreator
{
    public string CreateEvoBundle(Object[] assetPrefabs, bool buildBundle = false)
    {
        bool checkingOk = true;
		
        string assetName = "";
		
        string parentBundle = "";
		
        List<string> bundlesToBuild = new List<string>();

        System.Array.Sort(assetPrefabs, delegate(Object o1, Object o2) {
            return o1.name.CompareTo(o2.name);
        });

        Object parentAsset = assetPrefabs[0];
        parentBundle = parentAsset.name.ToLower();
        assetName = parentAsset.name.Substring(0, parentAsset.name.IndexOf("_EVO"));	

        checkingOk &= CreateBundle(parentBundle, 0.3f, false);
		
        if(!checkingOk)
        {
            Debug.LogError("Bundle Creator ERROR: Creating asset bundle '" + parentBundle + "'");
			
            return "Bundle Creator ERROR: Creating asset bundle '" + parentBundle + "'";
        }	
				
        for(int i = 0; i < assetPrefabs.Length && checkingOk; i++)
        {
            //Prefabs
            string prefabPath = AssetDatabase.GetAssetPath(assetPrefabs[i]);
			
            if(!prefabPath.Contains(assetName))
            {
                checkingOk = false;
                break;
            }

            var bundleName = assetPrefabs[i].name.ToLower();

            if(bundleName != parentBundle)
            {
                checkingOk &= CreateBundle(bundleName, 0.3f + 0.3f * 0.6f * (i + 1f) / assetPrefabs.Length, false);
            }

            AddPathToBundle(prefabPath, bundleName, 0.3f + 0.3f * 1f * (i + 1f) / assetPrefabs.Length);

            bundlesToBuild.Add(bundleName);
        }
		
        if(!checkingOk)
        {			
            Debug.LogError("Bundle Creator ERROR: Some Bundles cannot be created");
			
            return "Bundle Creator ERROR: Some Bundles cannot be created";			
        }
		
        if(buildBundle)
        {			
            EditorUtility.DisplayProgressBar("Bundle Creator", "Building Bundles... ", 0.9f);
            BuildHelper.BuildBundles(bundlesToBuild.ToArray());			
        }
		
        return string.Empty;
    }

    public string CreateSimpleBundle(Object[] assetPrefabs, bool scene = false, bool buildBundle = false)
    {
        bool checkingOk = true;
		
        List<string> bundlesToBuild = new List<string>();
		
        for(int i = 0; i < assetPrefabs.Length && checkingOk; i++)
        {
            //Prefabs
            string prefabPath = AssetDatabase.GetAssetPath(assetPrefabs[i]);

            var bundleName = assetPrefabs[i].name.ToLower();
			
            checkingOk &= CreateBundle(bundleName, 0.6f + 0.3f * (i + 1f) / assetPrefabs.Length, scene);

            AddPathToBundle(prefabPath, bundleName, 0.6f * (i + 1f) / assetPrefabs.Length);

            string possibleHierarchy = CheckPossibleHierarchy(bundleName, bundlesToBuild);

            bundlesToBuild.Add(bundleName);    

            if(possibleHierarchy.Length > 0)
            {
                if(EditorUtility.DisplayDialog("Bundle Creator", "Possible Hierarchy Found\n------------------------------\n\n" +
                   possibleHierarchy + "\n          " + bundleName + "\n\n The new bundle '" + bundleName +
                   "' contains assets that are already contained in the bundle '" + possibleHierarchy +
                   "'. Would you like to create a hierarchy in order to optimize memory?", "Yes", "No"))
                {
                    BundleManager.SetParent(bundleName, possibleHierarchy);
                }
            }           
        }
		
        if(!checkingOk)
        {
            Debug.LogError("Bundle Creator ERROR: Some Bundles cannot be created");
			
            return "Bundle Creator ERROR: Some Bundles cannot be created";
        }
		
        if(buildBundle)
        {
            EditorUtility.DisplayProgressBar("Bundle Creator", "Building Bundles... ", 0.9f);
            BuildHelper.BuildBundles(bundlesToBuild.ToArray());
        }
		
        return string.Empty;
    }

    public string CreateSimpleCombinedBundle(Object[] assetPrefabs, bool buildBundle = false)
    {
        bool checkingOk = true;
		
        string bundleName = assetPrefabs[0].name.ToLower();
		
        checkingOk &= CreateBundle(bundleName, 0.3f, false);
		
        for(int i = 0; i < assetPrefabs.Length && checkingOk; i++)
        {
            string prefabPath = AssetDatabase.GetAssetPath(assetPrefabs[i]);
			
            AddPathToBundle(prefabPath, bundleName, 0.3f + 0.3f * (i + 1f) / assetPrefabs.Length);
        }
		
        if(!checkingOk)
        {
            Debug.LogError("Bundle Creator ERROR: Some Bundles cannot be created");
			
            return "Bundle Creator ERROR: Some Bundles cannot be created";
        }
		
        if(buildBundle)
        {
            EditorUtility.DisplayProgressBar("Bundle Creator", "Building Bundles...  ", 0.9f);
            BuildHelper.BuildBundles(new string[] { bundleName });
        }
		
        return string.Empty;
    }

    bool CreateBundle(string bundleName, float progress, bool sceneBundle)
    {
        EditorUtility.DisplayProgressBar("Bundle Creator", "Removing old bundle... " + bundleName, progress);
        BundleManager.RemoveBundle(bundleName);

        EditorUtility.DisplayProgressBar("Bundle Creator", "Creating bundle... " + bundleName, progress);
        return BundleManager.CreateNewBundle(bundleName, "", sceneBundle);
    }

    void AddPathToBundle(string path, string bundleName, float progress)
    {
        EditorUtility.DisplayProgressBar("Bundle Creator", "Adding content to the bundle... " + bundleName, progress);
        if(BundleManager.CanAddPathToBundle(path, bundleName))
        {
            BundleManager.AddPathToBundle(path, bundleName);
        }
    }

    string CheckPossibleHierarchy(string bundleName, List<string> bundlesToBuild)
    {
        BundleData bd = BundleManager.GetBundleData(bundleName);

        List<string> includes = bd.includeGUIDs;
        List<string> dependences = bd.dependGUIDs;
       
        List<BundleData> bundlesToCompare = new List<BundleData>(BundleManager.bundles);
        for(int i = 0; i < bundlesToBuild.Count; i++)
        {
            var data = BundleManager.GetBundleData(bundlesToBuild[i]);
            if(data != null)
            {
                bundlesToCompare.Add(data);
            }
        }

        KeyValuePair<string, int> maxSharedBundle = new KeyValuePair<string, int>();

        for(int i = 0; i < bundlesToCompare.Count; i++)
        {
            BundleData bundleToCompare = bundlesToCompare[i];

            if(bundleName != bundleToCompare.name)
            {
                int sharedAssetNum = 0;
                sharedAssetNum += FindSharedAssets(includes, bundleToCompare);
                sharedAssetNum += FindSharedAssets(dependences, bundleToCompare);
                if(sharedAssetNum > maxSharedBundle.Value)
                {
                    maxSharedBundle = new KeyValuePair<string, int>(bundleToCompare.name, sharedAssetNum);
                }
            }
        }

        return maxSharedBundle.Key;
    }

    int FindSharedAssets(List<string> guidsList, BundleData bundleToCompare)
    {
        int sharedAssets = 0;
        for(int j = 0; j < guidsList.Count; j++)
        {
            if(bundleToCompare.includeGUIDs.Contains(guidsList[j]) || bundleToCompare.dependGUIDs.Contains(guidsList[j]))
            {
                sharedAssets++;
            }
        }
        return sharedAssets;
    }
}
