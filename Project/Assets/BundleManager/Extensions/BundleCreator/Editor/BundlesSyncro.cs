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

            for(int i = 0; i < attrs.AsDic["asset_versioning"].AsList.Count; i++)
            {
                var assetVersioningItem = attrs.AsDic["asset_versioning"].AsList[i].AsDic;
                string bundleName = assetVersioningItem["name"].ToString();
                long crc = (long)assetVersioningItem["crc"].AsValue.ToLong();
                bool isLocal = assetVersioningItem["isLocal"].AsValue.ToBool();
                int version = assetVersioningItem["version"].AsValue.ToInt();

                AttrDic newBundle = new AttrDic();
                newBundle["bundleName"] = new AttrString(bundleName);
                newBundle["bundleCRC"] = new AttrLong(crc);
                newBundle["bundleVersion"] = new AttrInt(version);

                if(isLocal)
                {
                    attrNodesList.Add(newBundle);
                }
            }

            localBundlesAttr["bundles"] = attrNodesList;
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