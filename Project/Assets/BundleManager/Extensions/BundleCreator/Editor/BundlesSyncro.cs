using System.IO;
using SocialPoint.Attributes;
using UnityEngine;

public class BundlesSynchro
{
    private string localAssetVersioningJson = Application.dataPath.Replace("/Assets", "") + "/Assets/StreamingAssets/json/localAssetVersioningJson.json";
    private string localBundlesJsonSPAM = Application.dataPath.Replace("/Assets", "") + "/Assets/StreamingAssets/json/localBundlesJsonSpam.json";

    public void Synchro()
    {
        //Takes the existing values if the config file already exists
        if(File.Exists(localAssetVersioningJson))
        {
            string JSONassetVersioning = File.ReadAllText(localAssetVersioningJson);

            JsonAttrParser parser = new JsonAttrParser();
            var attrs = parser.ParseString(JSONassetVersioning);

            AttrDic localBundlesAttr = new AttrDic();
            AttrList attrNodesList = new AttrList();

            for(int i = 0; i < attrs.AsDic[BundleCreatorHelper.AttrAssetVersioningKey].AsList.Count; i++)
            {
                var assetVersioningItem = attrs.AsDic[BundleCreatorHelper.AttrAssetVersioningKey].AsList[i].AsDic;
                string bundleName = assetVersioningItem[BundleCreatorHelper.AttrAssetNameKey].ToString();
                int version = assetVersioningItem[BundleCreatorHelper.AttrAssetVersionKey].AsValue.ToInt();

                AttrDic newBundle = new AttrDic();
                newBundle[BundleCreatorHelper.AttrAssetBundleNameKey] = new AttrString(bundleName);
                newBundle[BundleCreatorHelper.AttrAssetBundleVersionKey] = new AttrInt(version);

                attrNodesList.Add(newBundle);
            }

            localBundlesAttr[BundleCreatorHelper.AttrAssetBundlesKey] = attrNodesList;
            var jsonContent = LocalBundlesToJson(localBundlesAttr);

            SaveJsonFile(localBundlesJsonSPAM, jsonContent);

            Debug.Log("Bundles Successfully Copied!");
        }
        else
        {
            Debug.LogError("Bundles Synchro aborted: the localAssetVersioningJson cannot be found at the specified path --> " + localAssetVersioningJson);
        }
    }

    public string LocalBundlesToJson(AttrDic localBundlesAttr)
    {
        JsonAttrSerializer serializer = new JsonAttrSerializer();
        return System.Text.Encoding.ASCII.GetString(serializer.Serialize(localBundlesAttr));
    }

    static void SaveJsonFile(string jsonFileName, string contents)
    {
        string jsonFilePath = Path.Combine(UnityEngine.Application.persistentDataPath, jsonFileName);
        File.Delete(jsonFilePath);
        File.WriteAllText(jsonFilePath, contents);
        SocialPoint.Base.DebugUtils.Log("Saved json file: " + jsonFilePath);
    }
}