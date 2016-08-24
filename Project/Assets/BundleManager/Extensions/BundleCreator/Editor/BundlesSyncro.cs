using System.IO;
using SocialPoint.Attributes;
using SocialPoint.Base;
using UnityEngine;

public class BundlesSynchro
{
    string localAssetVersioningJson = Application.dataPath.Replace("/Assets", "") + "/Assets/StreamingAssets/json/localAssetVersioningJson.json";
    string localBundlesJsonSPAM = Application.dataPath.Replace("/Assets", "") + "/Assets/StreamingAssets/json/localBundlesJsonSpam.json";

    public void Synchro()
    {
        //Takes the existing values if the config file already exists
        if(File.Exists(localAssetVersioningJson))
        {
            string JSONassetVersioning = File.ReadAllText(localAssetVersioningJson);

            var parser = new JsonAttrParser();
            var attrs = parser.ParseString(JSONassetVersioning);

            var localBundlesAttr = new AttrDic();
            var attrNodesList = new AttrList();

            for(int i = 0; i < attrs.AsDic[BundleCreatorHelper.AttrAssetVersioningKey].AsList.Count; i++)
            {
                var assetVersioningItem = attrs.AsDic[BundleCreatorHelper.AttrAssetVersioningKey].AsList[i].AsDic;
                string bundleName = assetVersioningItem[BundleCreatorHelper.AttrAssetNameKey].ToString();
                int version = assetVersioningItem[BundleCreatorHelper.AttrAssetVersionKey].AsValue.ToInt();

                var newBundle = new AttrDic();
                newBundle[BundleCreatorHelper.AttrAssetBundleNameKey] = new AttrString(bundleName);
                newBundle[BundleCreatorHelper.AttrAssetBundleVersionKey] = new AttrInt(version);

                attrNodesList.Add(newBundle);
            }

            localBundlesAttr[BundleCreatorHelper.AttrAssetBundlesKey] = attrNodesList;
            var jsonContent = LocalBundlesToJson(localBundlesAttr);

            SaveJsonFile(localBundlesJsonSPAM, jsonContent);

            Log.d("Bundles Successfully Copied!");
        }
        else
        {
            Log.e("Bundles Synchro aborted: the localAssetVersioningJson cannot be found at the specified path --> " + localAssetVersioningJson);
        }
    }

    public string LocalBundlesToJson(AttrDic localBundlesAttr)
    {
        var serializer = new JsonAttrSerializer();
        return System.Text.Encoding.ASCII.GetString(serializer.Serialize(localBundlesAttr));
    }

    static void SaveJsonFile(string jsonFileName, string contents)
    {
        string jsonFilePath = Path.Combine(UnityEngine.Application.persistentDataPath, jsonFileName);
        File.Delete(jsonFilePath);
        File.WriteAllText(jsonFilePath, contents);
        Log.d("Saved json file: " + jsonFilePath);
    }
}