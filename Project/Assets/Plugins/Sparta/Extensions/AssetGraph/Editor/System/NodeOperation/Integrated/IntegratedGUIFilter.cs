using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace AssetBundleGraph
{
    public class IntegratedGUIFilter : INodeOperation
    {
        private readonly List<ConnectionData> connectionsToChild;
        public IntegratedGUIFilter(List<ConnectionData> connectionsToChild)
        {
            this.connectionsToChild = connectionsToChild;
        }

        public void Setup(BuildTarget target,
            NodeData node,
            ConnectionPointData inputPoint,
            ConnectionData connectionToOutput,
            Dictionary<string, List<Asset>> inputGroupAssets,
            List<string> alreadyCached,
            Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output)
        {
            node.ValidateOverlappingFilterCondition(true);
            Filter(node, inputGroupAssets, Output);
        }

        public void Run(BuildTarget target,
            NodeData node,
            ConnectionPointData inputPoint,
            ConnectionData connectionToOutput,
            Dictionary<string, List<Asset>> inputGroupAssets,
            List<string> alreadyCached,
            Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output)
        {
            Filter(node, inputGroupAssets, Output);
        }
        
        public void Skip(ConnectionData connectionToOutput, Dictionary<string, List<Asset>> inputGroupAssets, Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output)
        {
            Output(connectionToOutput, inputGroupAssets, null);
        }

        private class FilterableAsset
        {
            public Asset asset;
            public bool isFiltered = false;

            public FilterableAsset(Asset asset)
            {
                this.asset = asset;
            }
        }

        private void Filter(NodeData node, Dictionary<string, List<Asset>> inputGroupAssets, Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output)
        {

            foreach(var connToChild in connectionsToChild)
            {

                var filter = node.FilterConditions.Find(fc => fc.ConnectionPoint.Id == connToChild.FromNodeConnectionPointId);
                UnityEngine.Assertions.Assert.IsNotNull(filter);

                var output = new Dictionary<string, List<Asset>>();

                foreach(var groupKey in inputGroupAssets.Keys)
                {
                    var filteringKeyword = string.IsNullOrEmpty(filter.FilterKeyword) ? WildcardDeep : filter.FilterKeyword;
                    var assets = inputGroupAssets[groupKey];
                    var filteringAssets = new List<FilterableAsset>();
                    assets.ForEach(a => filteringAssets.Add(new FilterableAsset(a)));


                    // filter by keyword first
                    List<FilterableAsset> keywordContainsAssets = filteringAssets.Where(
                        assetData =>
                        !assetData.isFiltered &&
                        (filter.IsExclusion ^ GlobMatch(filteringKeyword, assetData.asset.importFrom.Replace(assetData.asset.sourceBasePath, string.Empty)))
                    ).ToList();

                    List<FilterableAsset> finalFilteredAsset = new List<FilterableAsset>();

                    // then, filter by type
                    foreach(var a in keywordContainsAssets)
                    {
                        if(filter.FilterKeytype != AssetBundleGraphSettings.DEFAULT_FILTER_KEYTYPE)
                        {
                            var assumedType = TypeUtility.FindTypeOfAsset(a.asset.importFrom);
                            if(assumedType == null || filter.FilterKeytype != assumedType.ToString())
                            {
                                continue;
                            }
                        }
                        finalFilteredAsset.Add(a);
                    }

                    // mark assets as exhausted.
                    foreach(var a in finalFilteredAsset)
                    {
                        a.isFiltered = true;
                    }

                    output[groupKey] = finalFilteredAsset.Select(v => v.asset).ToList();
                }

                Output(connToChild, output, null);
            }
        }

        private const char WildcardMultiChar = '*';
        private const string WildcardDeep = "**";
        private const char WildcardOneChar = '?';

        private static bool GlobMatch(string pattern, string value)
        {
            bool deep = pattern.Contains(WildcardDeep);
            if(deep)
            {
                pattern = pattern.Replace(WildcardDeep, WildcardMultiChar.ToString());
            }
            else if(value.Split(AssetBundleGraphSettings.UNITY_FOLDER_SEPARATOR).Length != pattern.Split(AssetBundleGraphSettings.UNITY_FOLDER_SEPARATOR).Length)
            {
                return false;
            }

            int pos = 0;
            while(pattern.Length != pos)
            {
                switch(pattern[pos])
                {
                    case WildcardOneChar:
                        break;

                    case WildcardMultiChar:
                        for(int i = value.Length; i >= pos; i--)
                        {
                            if(GlobMatch(pattern.Substring(pos + 1), value.Substring(i)))
                            {
                                return true;
                            }
                        }
                        return false;

                    default:
                        if(value.Length == pos || char.ToUpper(pattern[pos]) != char.ToUpper(value[pos]))
                        {
                            return false;
                        }
                        break;
                }

                pos++;
            }
            return value.Length == pos;
        }
    }
}
