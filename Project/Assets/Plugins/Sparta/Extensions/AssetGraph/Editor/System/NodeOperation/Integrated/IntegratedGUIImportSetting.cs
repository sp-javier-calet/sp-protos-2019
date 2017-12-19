using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace AssetBundleGraph
{

    /**
		IntegratedGUIImportSetting is the class for apply specific setting to already imported files.
	*/
    public class IntegratedGUIImportSetting : INodeOperation
    {

        public void Setup(BuildTarget target,
            NodeData node,
            ConnectionPointData inputPoint,
            ConnectionData connectionToOutput,
            Dictionary<string, List<Asset>> inputGroupAssets,
            List<string> alreadyCached,
            Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output)
        {
            var incomingAssets = inputGroupAssets.SelectMany(v => v.Value).ToList();

            Action<Type, Type, Asset> multipleAssetTypeFound = (Type expectedType, Type foundType, Asset foundAsset) =>
            {
                throw new NodeException(string.Format("{3} :ImportSetting expect {0}, but different type of incoming asset is found({1} {2})",
                    expectedType.FullName, foundType.FullName, foundAsset.fileNameAndExtension, node.Name), node.Id);
            };

            Action<Type> unsupportedType = (Type unsupported) =>
            {
                throw new NodeException(string.Format("{0} :Incoming asset type is not supported by ImportSetting (Incoming type:{1}). Perhaps you want to use Modifier instead?",
                    node.Name, (unsupported != null) ? unsupported.FullName : "null"), node.Id);
            };

            Action<Type, Type> incomingTypeMismatch = (Type expected, Type incoming) =>
            {
                throw new NodeException(string.Format("{0} :Incoming asset type is does not match with this ImportSetting (Expected type:{1}, Incoming type:{2}).",
                    node.Name, (expected != null) ? expected.FullName : "null", (incoming != null) ? incoming.FullName : "null"), node.Id);
            };

            Action<ConfigStatus> errorInConfig = (ConfigStatus _) =>
            {
                // give a try first in sampling file
                if(incomingAssets.Any())
                {

                    SaveSampleFile(node, TypeUtility.FindTypeOfAsset(incomingAssets[0].importFrom));

                    ValidateInputSetting(node, target, incomingAssets, multipleAssetTypeFound, unsupportedType, incomingTypeMismatch, (ConfigStatus eType) =>
                    {
                        if(eType == ConfigStatus.NoSampleFound)
                        {
                            throw new NodeException(node.Name + " :ImportSetting has no sampling file. Please configure it from Inspector.", node.Id);
                        }
                        if(eType == ConfigStatus.TooManySamplesFound)
                        {
                            throw new NodeException(node.Name + " :ImportSetting has too many sampling file. Please fix it from Inspector.", node.Id);
                        }
                    });
                }
            };

            ValidateInputSetting(node, target, incomingAssets, multipleAssetTypeFound, unsupportedType, incomingTypeMismatch, errorInConfig);

            // ImportSettings does not add, filter or change structure of group, so just pass given group of assets
            Output(connectionToOutput, inputGroupAssets, null);
        }

        public void Run(BuildTarget target,
            NodeData node,
            ConnectionPointData inputPoint,
            ConnectionData connectionToOutput,
            Dictionary<string, List<Asset>> inputGroupAssets,
            List<string> alreadyCached,
            Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output)
        {
            var incomingAssets = inputGroupAssets.SelectMany(v => v.Value).ToList();

            ApplyImportSetting(node, incomingAssets);

            Output(connectionToOutput, inputGroupAssets, null);
        }

        public void Skip(ConnectionData connectionToOutput, Dictionary<string, List<Asset>> inputGroupAssets, Action<ConnectionData, Dictionary<string, List<Asset>>, List<string>> Output)
        {
            Output(connectionToOutput, inputGroupAssets, null);
        }

        public static void RemoveConfigFile(string nodeId)
        {
            var path = FileUtility.PathCombine(AssetBundleGraphSettings.IMPORTER_SETTINGS_PLACE, nodeId);
            if(Directory.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }
            AssetDatabase.Refresh();
        }

        public static void SaveSampleFile(NodeData node, Type assetType)
        {
            var samplingDirectoryPath = FileUtility.PathCombine(AssetBundleGraphSettings.IMPORTER_SETTINGS_PLACE, node.Id);
            if(!Directory.Exists(samplingDirectoryPath))
            {
                Directory.CreateDirectory(samplingDirectoryPath);
            }

            var filePath = FileUtility.PathCombine(samplingDirectoryPath, AssetBundleGraphSettings.PLACEHOLDER_FILE[assetType]);

            AssetDatabase.CopyAsset(AssetGraphRelativePaths.ASSET_PLACEHOLDER_FOLDER + AssetBundleGraphSettings.PLACEHOLDER_FILE[assetType], filePath);

            AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
        }

        public static void CopySampleFile(NodeData source, NodeData destination)
        {
            var samplingDirectoryPath = FileUtility.PathCombine(AssetBundleGraphSettings.IMPORTER_SETTINGS_PLACE, source.Id);
            var destinationPath = FileUtility.PathCombine(AssetBundleGraphSettings.IMPORTER_SETTINGS_PLACE, destination.Id);
            if(!Directory.Exists(samplingDirectoryPath))
            {
                Debug.Log("No config found to copy");
                return;
            }
            if(!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }


            var file = Directory.GetFiles(samplingDirectoryPath, "config.*", SearchOption.TopDirectoryOnly)[0];

            if(Path.DirectorySeparatorChar != AssetBundleGraphSettings.UNITY_FOLDER_SEPARATOR)
            {
                file = file.Replace(Path.DirectorySeparatorChar.ToString(), AssetBundleGraphSettings.UNITY_FOLDER_SEPARATOR.ToString());
            }

            destinationPath = FileUtility.PathCombine(destinationPath, "config" + Path.GetExtension(file));

            AssetDatabase.CopyAsset(file, destinationPath);

            AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
        }

        public static ConfigStatus GetConfigStatus(NodeData node)
        {
            var sampleFileDir = FileUtility.PathCombine(AssetBundleGraphSettings.IMPORTER_SETTINGS_PLACE, node.Id);

            if(!Directory.Exists(sampleFileDir))
            {
                return ConfigStatus.NoSampleFound;
            }

            var sampleFiles = FileUtility.GetFilePathsInFolder(sampleFileDir)
                .Where(path => !path.EndsWith(AssetBundleGraphSettings.UNITY_METAFILE_EXTENSION))
                .ToList();

            if(sampleFiles.Count == 0)
            {
                return ConfigStatus.NoSampleFound;
            }
            if(sampleFiles.Count == 1)
            {
                return ConfigStatus.GoodSampleFound;
            }

            return ConfigStatus.TooManySamplesFound;
        }

        public static void ResetConfig(NodeData node)
        {
            var sampleFileDir = FileUtility.PathCombine(AssetBundleGraphSettings.IMPORTER_SETTINGS_PLACE, node.Id);
            FileUtility.RemakeDirectory(sampleFileDir);
        }

        public static AssetImporter GetReferenceAssetImporter(string nodeId)
        {
            var sampleFileDir = FileUtility.PathCombine(AssetBundleGraphSettings.IMPORTER_SETTINGS_PLACE, nodeId);

            UnityEngine.Assertions.Assert.IsTrue(Directory.Exists(sampleFileDir));

            var sampleFiles = FileUtility.GetFilePathsInFolder(sampleFileDir)
                .Where(path => !path.EndsWith(AssetBundleGraphSettings.UNITY_METAFILE_EXTENSION))
                .ToList();

            UnityEngine.Assertions.Assert.IsTrue(sampleFiles.Count == 1);

            return AssetImporter.GetAtPath(sampleFiles[0]);
        }

        public static UnityEngine.Object GetReferenceAsset(string nodeId)
        {
            var sampleFileDir = FileUtility.PathCombine(AssetBundleGraphSettings.IMPORTER_SETTINGS_PLACE, nodeId);

            UnityEngine.Assertions.Assert.IsTrue(Directory.Exists(sampleFileDir));

            var sampleFiles = FileUtility.GetFilePathsInFolder(sampleFileDir)
                .Where(path => !path.EndsWith(AssetBundleGraphSettings.UNITY_METAFILE_EXTENSION))
                .ToList();

            UnityEngine.Assertions.Assert.IsTrue(sampleFiles.Count == 1);

            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(sampleFiles[0]);
        }
        private void ApplyImportSetting(NodeData node, List<Asset> assets)
        {

            if(!assets.Any())
            {
                return;
            }

            var referenceImporter = GetReferenceAssetImporter(node.Id);
            var configurator = new ImportSettingsConfigurator(referenceImporter);
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach(var asset in assets)
                {
                    var importer = AssetImporter.GetAtPath(asset.importFrom);
                    if(!configurator.IsEqual(importer))
                    {
                        configurator.OverwriteImportSettings(importer);

                        // if the importsettings are applied manually we need to reimport the asset.
                        if(!PreProcessor.isPreProcessing)
                        {
                            AssetDatabase.ImportAsset(asset.importFrom, ImportAssetOptions.ForceUpdate);
                        }
                    }
                }
                AssetDatabase.StopAssetEditing();
            }
            catch(Exception e)
            {
                AssetDatabase.StopAssetEditing();
                throw e;
            }
        }

        public enum ConfigStatus
        {
            NoSampleFound,
            TooManySamplesFound,
            GoodSampleFound
        }

        public static void ValidateInputSetting(
            NodeData node,
            BuildTarget target,
            List<Asset> incomingAssets,
            Action<Type, Type, Asset> multipleAssetTypeFound,
            Action<Type> unsupportedType,
            Action<Type, Type> incomingTypeMismatch,
            Action<ConfigStatus> errorInConfig
        )
        {
            Type expectedType = TypeUtility.FindIncomingAssetType(incomingAssets);
            if(multipleAssetTypeFound != null)
            {
                if(expectedType != null)
                {
                    foreach(var a in incomingAssets)
                    {
                        Type assetType = TypeUtility.FindTypeOfAsset(a.importFrom);
                        if(assetType != expectedType)
                        {
                            multipleAssetTypeFound(expectedType, assetType, a);
                        }
                    }
                }
            }

            if(unsupportedType != null)
            {
                if(expectedType != null)
                {
                    if(expectedType == typeof(UnityEditor.TextureImporter) ||
                        expectedType == typeof(UnityEditor.ModelImporter) ||
                        expectedType == typeof(UnityEditor.AudioImporter)
                    )
                    {
                        // good. do nothing
                    }
                    else
                    {
                        unsupportedType(expectedType);
                    }
                }
            }

            var status = GetConfigStatus(node);

            if(errorInConfig != null)
            {
                if(status != ConfigStatus.GoodSampleFound)
                {
                    errorInConfig(status);
                    return;
                }
            }

            if(incomingTypeMismatch != null)
            {
                // if there is no incoming assets, there is no way to check if 
                // right type of asset is coming in - so we'll just skip the test
                if(incomingAssets.Any() && status == ConfigStatus.GoodSampleFound)
                {
                    var importer = GetReferenceAssetImporter(node.Id);
                    if(importer != null)
                    {
                        Type targetType = importer.GetType();
                        if(targetType != expectedType)
                        {
                            incomingTypeMismatch(targetType, expectedType);
                        }
                    }
                }
            }
        }
    }
}
