using System.Collections.Generic;
using System;
using UnityEditor;

namespace AssetBundleGraph
{
    public class ValidatorEntryData
    {
        const string VALIDATOR = "validator";
        const string OBJECTS = "objects";

        public NodeData ValidatorData;
        public List<InvalidObjectInfo> ObjectsWithError = new List<InvalidObjectInfo>();

        public ValidatorEntryData(Dictionary<string, object> jsonObject)
        {
            ValidatorData = new NodeData(jsonObject[VALIDATOR] as Dictionary<string, object>);
            var listObj = jsonObject[OBJECTS] as Dictionary<string, object>;
            foreach(var dictObj in listObj)
            {
                ObjectsWithError.Add(new InvalidObjectInfo(dictObj.Value as Dictionary<string, object>));
            }
        }

        public ValidatorEntryData(NodeData validator, params InvalidObjectInfo[] failingAssets)
        {
            ValidatorData = validator;
            ObjectsWithError.AddRange(failingAssets);
        }

        public void AddObjectAndUpdateInfo(NodeData validator, InvalidObjectInfo asset, BuildTargetGroup target)
        {
            ValidatorData = validator;
            var existingObj = ObjectsWithError.Find(x => x.AssetId == asset.AssetId);
            if(existingObj != null)
            {
                existingObj.UpdateInfo(asset, target);
            }
            else
            {
                ObjectsWithError.Add(asset);
            }
        }

        public void RemovePlatformForObject(string assetId, BuildTargetGroup target)
        {
            var existingObject = ObjectsWithError.Find(x => x.AssetId == assetId);
            if(existingObject != null)
            {
                existingObject.platformInfo.Remove(target);
                CleanEmptyObjects();
            }
        }

        public void RemovePlatformForAllObjects(BuildTargetGroup target)
        {
            ObjectsWithError.ForEach(x => x.platformInfo.Remove(target));
            CleanEmptyObjects();
        }

        public int CleanEmptyObjects()
        {
            return ObjectsWithError.RemoveAll(x => x.platformInfo.Count == 0);
        }

        public Dictionary<string, object> ToJsonDictionary()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary[VALIDATOR] = ValidatorData.ToJsonDictionary();

            Dictionary<string, object> objDict = new Dictionary<string, object>();
            foreach(var obj in ObjectsWithError)
            {
                objDict.Add(obj.AssetId, obj.ToJsonDictionary());
            }

            dictionary[OBJECTS] = objDict;

            return dictionary;
        }
    }
}
