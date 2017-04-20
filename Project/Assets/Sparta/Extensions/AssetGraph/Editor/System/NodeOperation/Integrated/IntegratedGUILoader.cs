using UnityEngine;

using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

namespace AssetBundleGraph
{
    public class IntegratedGUILoader : INodeOperation
    {
        public void Setup(BuildTarget target,
            NodeData node,
            ConnectionPointData inputPoint,
            ConnectionData connectionToOutput,
            Dictionary<string, List<Asset>> inputGroupAssets,
            List<string> alreadyCached,
            Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output)
        {
            ValidateLoadPath(
                node.LoaderLoadPath[target],
                node.GetLoaderFullLoadPath(target),
                () =>
                {
                    //can be empty
                    //throw new NodeException(node.Name + ": Load Path is empty.", node.Id);
                },
                () =>
                {
                    throw new NodeException(node.Name + ": Directory not found: " + node.GetLoaderFullLoadPath(target), node.Id);
                }
            );

            Load(target, node, connectionToOutput, inputGroupAssets, Output);
        }

        public void Run(BuildTarget target,
            NodeData node,
            ConnectionPointData inputPoint,
            ConnectionData connectionToOutput,
            Dictionary<string, List<Asset>> inputGroupAssets,
            List<string> alreadyCached,
            Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output)
        {
            Load(target, node, connectionToOutput, inputGroupAssets, Output);
        }
        
        public void Skip(ConnectionData connectionToOutput, Dictionary<string, List<Asset>> inputGroupAssets, Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output)
        {
            Output(connectionToOutput, inputGroupAssets, null);
        }

        void Load(BuildTarget target,
            NodeData node,
            ConnectionData connectionToOutput,
            Dictionary<string, List<Asset>> inputGroupAssets,
            Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output)
        {
            // SOMEWHERE_FULLPATH/PROJECT_FOLDER/Assets/
            var assetsFolderPath = Application.dataPath + AssetBundleGraphSettings.UNITY_FOLDER_SEPARATOR;

            var loaderPath = node.GetLoaderFullLoadPath(target);
            var relativeLoaderPath = loaderPath.Replace(assetsFolderPath, AssetBundleGraphSettings.ASSETS_PATH) + AssetBundleGraphSettings.UNITY_FOLDER_SEPARATOR;

            var outputSource = new List<Asset>();
            var targetFilePaths = FileUtility.GetAllFilePathsInFolder(loaderPath);

            var loaderSaveData = LoaderSaveData.LoadFromDisk();
            targetFilePaths.RemoveAll(x =>
            {
                var bestLoader = loaderSaveData.GetBestLoaderData(x);
                return bestLoader == null || bestLoader.id != node.Id;
            });

            foreach(var targetFilePath in targetFilePaths)
            {

                if(targetFilePath.Contains(AssetBundleGraphSettings.ASSETBUNDLEGRAPH_PATH))
                {
                    continue;
                }

                // already contained into Assets/ folder.
                // imported path is Assets/SOMEWHERE_FILE_EXISTS.
                if(targetFilePath.StartsWith(assetsFolderPath))
                {
                    var relativePath = targetFilePath.Replace(assetsFolderPath, AssetBundleGraphSettings.ASSETS_PATH);

                    var assetType = TypeUtility.GetTypeOfAsset(relativePath);
                    if(assetType == typeof(object))
                    {
                        continue;
                    }

                    outputSource.Add(Asset.CreateNewAssetFromLoader(targetFilePath, relativePath, relativeLoaderPath));
                    continue;
                }

                throw new NodeException(node.Name + ": Invalid Load Path. Path must start with Assets/", node.Name);
            }

            var outputDir = new Dictionary<string, List<Asset>> {
                {"0", outputSource}
            };

            Output(connectionToOutput, outputDir, null);
        }


        public void LoadSingleAsset(BuildTarget target,
            NodeData node,
            ConnectionData connectionToOutput,
            string path,
            Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output
            )
        {
            var outputSource = new List<Asset>();
            Asset asset = null;
            var assetType = TypeUtility.GetTypeOfAsset(path);
            var loaderPath = node.GetLoaderFullLoadPath(target);
            var relativeLoaderPath = loaderPath.Replace(Application.dataPath + AssetBundleGraphSettings.UNITY_FOLDER_SEPARATOR, AssetBundleGraphSettings.ASSETS_PATH) + AssetBundleGraphSettings.UNITY_FOLDER_SEPARATOR;

            if(assetType == typeof(object))
            {
                AssetImporter importer = AssetImporter.GetAtPath(path);
                asset = Asset.CreateNewAssetFromImporter(importer, relativeLoaderPath);
            }
            else
            {
                var absPath = Application.dataPath + AssetBundleGraphSettings.UNITY_FOLDER_SEPARATOR + path;
                asset = Asset.CreateNewAssetFromLoader(absPath, path, relativeLoaderPath);
            }

            outputSource.Add(asset);

            var outputDir = new Dictionary<string, List<Asset>> {
                {"0", outputSource}
            };

            Output(connectionToOutput, outputDir, null);
        }


        public static void ValidateLoadPath(string currentLoadPath, string combinedPath, Action NullOrEmpty, Action NotExist)
        {
            if(string.IsNullOrEmpty(currentLoadPath)) NullOrEmpty();
            if(!Directory.Exists(combinedPath)) NotExist();
        }
    }
}
