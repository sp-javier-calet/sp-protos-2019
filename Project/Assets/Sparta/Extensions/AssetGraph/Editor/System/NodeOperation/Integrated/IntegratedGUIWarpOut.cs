using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace AssetBundleGraph
{
    public class IntegratedGUIWarpOut : INodeOperation
    {
        public void Setup(BuildTarget target, NodeData nodeData, ConnectionPointData inputPoint, ConnectionData connectionToOutput, Dictionary<string, List<Asset>> inputGroupAssets, List<string> alreadyCached, Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output)
        {
            Output(connectionToOutput, inputGroupAssets, null);
        }

        public void Run(BuildTarget target, NodeData nodeData, ConnectionPointData inputPoint, ConnectionData connectionToOutput, Dictionary<string, List<Asset>> inputGroupAssets, List<string> alreadyCached, Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output)
        {
            Output(connectionToOutput, inputGroupAssets, null);
        }

        public void Skip(ConnectionData connectionToOutput, Dictionary<string, List<Asset>> inputGroupAssets, Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output)
        {
            Output(connectionToOutput, inputGroupAssets, null);
        }
    }
}
