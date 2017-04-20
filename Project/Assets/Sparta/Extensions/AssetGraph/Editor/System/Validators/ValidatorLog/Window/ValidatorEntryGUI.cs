using UnityEditor;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;

namespace AssetBundleGraph
{
    public class ValidatorEntryGUI
    {
        public class OutdatedInfo
        {
            public bool fileChanged = false;
            public bool graphChanged = false;
        }

        public bool fileRemoved = false;
        public bool validatorRemoved = false;
        public SortedDictionary<BuildTargetGroup, OutdatedInfo> outdatedInfo;
        public BuildTargetGroup currentEditingTarget;

        public NodeData validatorData;
        public InvalidObjectInfo invalidObject;

        private DateTime _saveDataModified;

        public ValidatorEntryGUI(NodeData validatorData, InvalidObjectInfo invalidObject, bool validatorRemovedInGraph, DateTime lastTimeSaveDataModified)
        {
            var enumerator = invalidObject.platformInfo.Keys.GetEnumerator();
            enumerator.MoveNext();
            currentEditingTarget = enumerator.Current;

            outdatedInfo = new SortedDictionary<BuildTargetGroup, OutdatedInfo>();
            Initialize(validatorData, invalidObject, validatorRemovedInGraph, lastTimeSaveDataModified);
        }

        public void Initialize(NodeData validatorData, InvalidObjectInfo invalidObject, bool validatorRemovedInGraph, DateTime lastTimeSaveDataModified)
        {
            this.validatorData = validatorData;
            this.invalidObject = invalidObject;

            if(!invalidObject.platformInfo.ContainsKey(currentEditingTarget))
            {
                var enumerator = invalidObject.platformInfo.Keys.GetEnumerator();
                enumerator.MoveNext();
                currentEditingTarget = enumerator.Current;
            }

            foreach(var target in invalidObject.platformInfo.Keys)
            {
                outdatedInfo[target] = new OutdatedInfo();
            }

            this.validatorRemoved = validatorRemovedInGraph;
            _saveDataModified = lastTimeSaveDataModified;
            CheckIsOutdated();
        }


        public void CheckIsOutdated()
        {
            if(!validatorRemoved)
            {
                foreach(var pair in invalidObject.platformInfo)
                {
                    outdatedInfo[pair.Key].graphChanged = DateTime.Compare(_saveDataModified, pair.Value.lastUpdated) > 0;
                }
            }
            AssetDatabase.Refresh();
            var assetPath = AssetDatabase.GUIDToAssetPath(invalidObject.AssetId);
            fileRemoved = string.IsNullOrEmpty(assetPath);
            if(!fileRemoved)
            {
                var changedTimeFile = File.GetLastWriteTimeUtc(assetPath);
                var changedTimeMeta = File.GetLastWriteTimeUtc(assetPath + ".meta");
                var mostRecentChange = changedTimeMeta > changedTimeFile ? changedTimeMeta : changedTimeFile;

                foreach(var pair in invalidObject.platformInfo)
                {
                    outdatedInfo[pair.Key].fileChanged = DateTime.Compare(mostRecentChange, pair.Value.lastUpdated) > 0;
                }
            }
        }

        public void SwitchTargetTab(BuildTargetGroup newTarget)
        {
            if(invalidObject.platformInfo.ContainsKey(newTarget))
            {
                currentEditingTarget = newTarget;
            }
        }


        public OutdatedInfo GetCurrentOutdatedInfo()
        {
            return outdatedInfo[currentEditingTarget];
        }


        public static int SortByName(ValidatorEntryGUI obj1, ValidatorEntryGUI obj2)
        {
            return (obj1.validatorData.Name + obj1.invalidObject.AssetName).CompareTo(obj2.validatorData.Name + obj2.invalidObject.AssetName);
        }
    }
}
