using UnityEngine;
using BM.Extensions;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.AssetVersioning;
using SocialPoint.Base;
using System.IO;
using System;

namespace BM.Extensions
{
    public class PostBuildProcedure : BBPPostBuild
    {
        const string GameJson = "/Resources/game.json";

        static Dictionary<string, AssetVersioningData> _assetVersioningDict = new Dictionary<string, AssetVersioningData>();
        readonly static string _assetVersionJson = Path.Combine(Application.streamingAssetsPath, "json/localAssetVersioningJson.json");

        public void run()
        {
            ExportToJson();

            var synchronizer = new BundlesSynchro();
            synchronizer.Synchro();
        }

        static void LoadAssetVersioningJson(Dictionary<string, AssetVersioningData> dict, string jsonPath)
        {
            dict.Clear();

            //Takes the existing values if the config file already exists
            if(File.Exists(jsonPath))
            {
                string str = File.ReadAllText(jsonPath);

                JsonAttrParser parserJson = new JsonAttrParser();
                AttrDic json = parserJson.Parse(System.Text.ASCIIEncoding.ASCII.GetBytes(str)).AsDic;

                AttrList attrBundles = json.AsDic.Get(BundleCreatorHelper.AttrAssetVersioningKey).AsList;
                for(int i = 0; i < attrBundles.Count; i++)
                {
                    Attr obj = attrBundles[i];

                    string bundleName = obj.AsDic.Get(BundleCreatorHelper.AttrAssetNameKey).ToString();

                    AssetVersioningData assetVersioningData = new AssetVersioningData();
                    assetVersioningData.Version = obj.AsDic.GetValue(BundleCreatorHelper.AttrAssetVersionKey).ToInt();
                    if(obj.AsDic.Get(BundleCreatorHelper.AttrAssetParentKey).GetType() != typeof(AttrEmpty) && !string.IsNullOrEmpty(obj.AsDic.Get(BundleCreatorHelper.AttrAssetParentKey).ToString()))
                    {
                        assetVersioningData.Parent = obj.AsDic.Get(BundleCreatorHelper.AttrAssetParentKey).ToString();
                    }
                    if(obj.AsDic.Get(BundleCreatorHelper.AttrAssetCrcKey).GetType() != typeof(AttrEmpty) && !string.IsNullOrEmpty(obj.AsDic.Get(BundleCreatorHelper.AttrAssetCrcKey).ToString()))
                    {
                        assetVersioningData.CRC = Convert.ToUInt32(obj.AsDic.Get(BundleCreatorHelper.AttrAssetCrcKey).ToString());
                    }
                    if(obj.AsDic.Get(BundleCreatorHelper.AttrAssetLocalKey).GetType() != typeof(AttrEmpty) && !string.IsNullOrEmpty(obj.AsDic.Get(BundleCreatorHelper.AttrAssetLocalKey).ToString()))
                    {
                        assetVersioningData.IsLocal = obj.AsDic.GetValue(BundleCreatorHelper.AttrAssetLocalKey).ToBool();
                    }
                    else
                    {
                        assetVersioningData.IsLocal = false;
                    }

                    dict.Add(bundleName, assetVersioningData);
                }
            }
        }

        static public void ExportToJson()
        {
            LoadAssetVersioningJson(_assetVersioningDict, _assetVersionJson);

            var localAssetVersioningAttr = new AttrDic();
            var attrNodesList = new AttrList();

            //BUNDLES
            foreach(BundleBuildState bundle in BundleManager.buildStates)
            {
                var newBundle = new AttrDic();

                newBundle[BundleCreatorHelper.AttrAssetNameKey] = new AttrString(bundle.bundleName);
                if(_assetVersioningDict != null && _assetVersioningDict.ContainsKey(bundle.bundleName))
                {
                    newBundle[BundleCreatorHelper.AttrAssetVersionKey] = new AttrInt(_assetVersioningDict[bundle.bundleName].Version);
                    newBundle[BundleCreatorHelper.AttrAssetLocalKey] = new AttrBool(_assetVersioningDict[bundle.bundleName].IsLocal);
                }
                else
                {
                    newBundle[BundleCreatorHelper.AttrAssetVersionKey] = new AttrInt(1);
                    newBundle[BundleCreatorHelper.AttrAssetLocalKey] = new AttrBool(true);
                }

                newBundle[BundleCreatorHelper.AttrAssetCrcKey] = new AttrLong(bundle.crc);

                BundleData bData = BundleManager.GetBundleData(bundle.bundleName);
                if(bData != null && bData.parent != null)
                {
                    newBundle[BundleCreatorHelper.AttrAssetParentKey] = new AttrString(bData.parent);
                }

                attrNodesList.Add(newBundle);
            }


            localAssetVersioningAttr[BundleCreatorHelper.AttrAssetVersioningKey] = attrNodesList;

            var serializer = new JsonAttrSerializer();
            var jsonContent = System.Text.Encoding.ASCII.GetString(serializer.Serialize(localAssetVersioningAttr));

            string jsonFilePath = Path.Combine(UnityEngine.Application.persistentDataPath, _assetVersionJson);
            File.Delete(jsonFilePath);
            File.WriteAllText(jsonFilePath, jsonContent);
            Log.d("Saved json file: " + jsonFilePath);

            //Save game.json
            var json = File.ReadAllText(Application.dataPath + GameJson);
            var gameData = new JsonAttrParser().ParseString(json);

            gameData.AsDic[BundleCreatorHelper.AttrAssetConfigKey].AsDic[BundleCreatorHelper.AttrAssetBundlesKey].AsDic[BundleCreatorHelper.AttrAssetVersioningKey] = attrNodesList;

            jsonContent = System.Text.Encoding.ASCII.GetString(serializer.Serialize(gameData));

            jsonFilePath = Application.dataPath + GameJson;
            File.Delete(jsonFilePath);
            File.WriteAllText(jsonFilePath, jsonContent);
        }
    }
}