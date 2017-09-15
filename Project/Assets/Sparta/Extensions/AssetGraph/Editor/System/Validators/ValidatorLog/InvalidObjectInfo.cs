using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace AssetBundleGraph
{
    public class InvalidObjectInfo
    {
        private const string ASSET_NAME = "asset_name";
        private const string ASSET_ID = "asset_id";
        public string AssetName;
        public string AssetId;
        public SortedDictionary<BuildTargetGroup, PlatformDependentInfo> platformInfo = new SortedDictionary<BuildTargetGroup, PlatformDependentInfo>();

        public InvalidObjectInfo(Asset asset, string message, BuildTargetGroup target)
        {
            AssetId = AssetDatabase.AssetPathToGUID(asset.importFrom);
            AssetName = asset.fileNameAndExtension;
            platformInfo[target] = new PlatformDependentInfo(message);
        }

        public void UpdateInfo(InvalidObjectInfo obj, BuildTargetGroup target)
        {
            platformInfo[target] = obj.platformInfo[target];
        }

        public InvalidObjectInfo(Dictionary<string, object> jsonObject)
        {
            AssetName = jsonObject[ASSET_NAME] as string;
            AssetId = jsonObject[ASSET_ID] as string;

            foreach(var pair in jsonObject)
            {
                try
                {
                    var enumValue = (BuildTargetGroup)Enum.Parse(typeof(BuildTargetGroup), pair.Key);

                    platformInfo[enumValue] = new PlatformDependentInfo(pair.Value as Dictionary<string, object>);
                }
                catch { }
            }
        }

        public Dictionary<string, object> ToJsonDictionary()
        {
            var dict = new Dictionary<string, object>();

            dict[ASSET_NAME] = AssetName;
            dict[ASSET_ID] = AssetId;

            foreach(var pair in platformInfo)
            {
                dict[pair.Key.ToString()] = pair.Value.ToJsonDictionary();
            }

            return dict;
        }
    }
}
