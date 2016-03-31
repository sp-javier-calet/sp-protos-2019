using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SocialPoint.Attributes;


namespace SocialPoint.Tool.Server
{
    public class BuildAssetBundleDelegate : ToolServiceDelegate
    {
        //public BundleManagerData BMData { get; private set; }
        BMDataAccessor.DataFilePaths BkpBMPaths { get; set; }

        string[]                    bundlesToBuild;
        BuildPlatform               bmBuildTarget;
        MobileTextureSubtarget       textureCompressionFormat;

        List<BuiltBundle>           builtBundles;

        void Init()
        {
            //BMData = new BundleManagerData();
            //BMData.Init();

            //BkpBMData = new BundleManagerData(BMData); // copy value types
            BkpBMPaths = new BMDataAccessor.DataFilePaths();
            BkpBMPaths.CopyOver(BMDataAccessor.Paths);
        }

        void ParseParameters(BuildAssetBundleParameters parameters)
        {
            if(parameters.bundlesToBuild == null || parameters.bundlesToBuild.Length == 0)
            {
                throw new Exception("Invalid parameter 'bundlesToBuild': Null or empty array");
            }
            else
            {
                bundlesToBuild = parameters.bundlesToBuild;
            }

            if(String.IsNullOrEmpty(parameters.buildTarget))
            {
                throw new Exception("Invalid parameter 'buildTarget': Empty string");
            }
            else
            {
                switch(parameters.buildTarget.ToLowerInvariant())
                {
                case "ios":
                    {
                        bmBuildTarget = BuildPlatform.IOS;
                        break;
                    }
                case "android":
                    {
                        bmBuildTarget = BuildPlatform.Android;
                        break;
                    }
                case "webplayer":
                    {
                        bmBuildTarget = BuildPlatform.WebPlayer;
                        break;
                    }
                case "standalone":
                    {
                        bmBuildTarget = BuildPlatform.Standalones;
                        break;
                    }
                default:
                    throw new Exception(String.Format("Invalid parameter 'buildTarget': Unsupported target '{0}'",
                                                          parameters.buildTarget));
                }
            }

            if(bmBuildTarget == BuildPlatform.Android)
            {
                if(String.IsNullOrEmpty(parameters.textureCompressionFormat))
                {
                    throw new Exception("Invalid parameter 'textureCompressionFormat': Empty string");
                }

                string txtfmt = BMUrls.StandarizeTextureFormat(parameters.textureCompressionFormat);
                if (txtfmt == String.Empty)
                {
                    throw new Exception(String.Format(
                        "Invalid parameter 'textureCompressionFormat': Unsupported compression '{0}'",
                        parameters.textureCompressionFormat));
                }

                textureCompressionFormat = (MobileTextureSubtarget)Enum.Parse(typeof(MobileTextureSubtarget), txtfmt);
            }

            var bmConfig =  new BMDataAccessor.DataFilePaths(BMDataAccessor.BasePath);

            if(!String.IsNullOrEmpty(parameters.bmConfiger))
            {
                bmConfig.BMConfigerPath = parameters.bmConfiger;
                //string newContent = Utils.GetFileContents(parameters.bmConfiger);
                //BMData.BMConfiger.UpdateFile(newContent);
            }

            if(!String.IsNullOrEmpty(parameters.bundleData))
            {
                bmConfig.BundleDataPath = parameters.bundleData;
                //string newContent = Utils.GetFileContents(parameters.bundleData);
                //BMData.BundleData.UpdateFile(newContent);
            }

            if(!String.IsNullOrEmpty(parameters.buildStates))
            {
                bmConfig.BundleBuildStatePath = parameters.buildStates;
                //string newContent = Utils.GetFileContents(parameters.buildStates);
                //BMData.BuildStates.UpdateFile(newContent);
            }

            if(!String.IsNullOrEmpty(parameters.urls))
            {
                bmConfig.UrlDataPath = parameters.urls;
                //string newContent = (parameters.urls);
                //BMData.Urls.UpdateFile(newContent);
            }

            // If BuildStates path is not the default, merge default buildStates with the new specified one
            BundleBuildState[] defaultBuildStates = null;
            if(bmConfig.BundleBuildStatePath != BMDataAccessor.Paths.BundleBuildStatePath)
            {
                defaultBuildStates = new BundleBuildState[BMDataAccessor.BuildStates.Count];
                BMDataAccessor.BuildStates.CopyTo(defaultBuildStates);
            }

            // Set the new paths and rebuild data structures
            BMDataAccessor.SetNewPaths(bmConfig);

            // merge buildstates if necessary
            if(defaultBuildStates != null)
            {
                MergeInBuildStates(defaultBuildStates);
            }
        }

        void MergeInBuildStates(BundleBuildState[] other)
        {
            // Iterate over 'other' and merge in only the 'change time' value if it
            for(int i = 0; i < other.Length; ++i)
            {
                var otherBuildState = other[i];
                var currentBuildState = BundleManager.GetBuildStateOfBundle(otherBuildState.bundleName);
                // only if the buildState can be found in the current BuildStates
                if(currentBuildState != null)
                {
                    if(otherBuildState.changeTime == -1)
                    {
                        continue;
                    }
                    else if(currentBuildState.changeTime == -1)
                    {
                        currentBuildState.changeTime = otherBuildState.changeTime;
                    }
                    else
                    {
                        var otherChangeTime = DateTime.FromBinary(otherBuildState.changeTime);
                        var currentChangeTime = DateTime.FromBinary(currentBuildState.changeTime);
                        if(System.DateTime.Compare(currentChangeTime, otherChangeTime) < 0)
                        {
                            currentBuildState.changeTime = otherBuildState.changeTime;
                        }
                    }
                }
            }
        }

        void RestoreBackupFiles(bool force=false)
        {
            /*
            if(force || !Utils.CompareContent(BMData.BMConfiger.text, BkpBMData.BMConfiger.text))
            {
                BkpBMData.BMConfiger.UpdateFile();
            }

            if(force || !Utils.CompareContent(BMData.BundleData.text, BkpBMData.BundleData.text))
            {
                BkpBMData.BundleData.UpdateFile();
            }

            if(force || !Utils.CompareContent(BMData.BuildStates.text, BkpBMData.BuildStates.text))
            {
                BkpBMData.BuildStates.UpdateFile();
            }

            if(force || !Utils.CompareContent(BMData.JSONConfigBundleData.text, BkpBMData.JSONConfigBundleData.text))
            {
                BkpBMData.JSONConfigBundleData.UpdateFile();
            }

            if(force || !Utils.CompareContent(BMData.Urls.text, BkpBMData.Urls.text))
            {
                BkpBMData.Urls.UpdateFile();
            }
            */
            BMDataAccessor.SetNewPaths (BkpBMPaths);
        }

        void PrepareEnvironment()
        {
            if(BMDataAccessor.Urls.bundleTarget != bmBuildTarget)
            {
                Debug.LogWarning(
                    String.Format("BundleManager bundle target and build target mismatch ({0} != {1}). This will force asset reimporting.",
                              Enum.GetName(typeof(BuildPlatform), BMDataAccessor.Urls.bundleTarget),
                              Enum.GetName(typeof(BuildPlatform), bmBuildTarget)));
                BMDataAccessor.Urls.bundleTarget = bmBuildTarget;
                BMDataAccessor.Urls.useEditorTarget = false;
                BMDataAccessor.SaveUrls();
            }

            if(BMDataAccessor.Urls.bundleTarget == BuildPlatform.Android &&
                EditorUserBuildSettings.androidBuildSubtarget != textureCompressionFormat)
            {
                Debug.LogWarning(
                    String.Format("Editor texture compression and build texture compression mismatch ({0} != {1}). This will force asset reimporting.",
                              Enum.GetName(typeof(MobileTextureSubtarget), EditorUserBuildSettings.androidBuildSubtarget),
                              Enum.GetName(typeof(MobileTextureSubtarget), textureCompressionFormat)));
                EditorUserBuildSettings.androidBuildSubtarget = textureCompressionFormat;
            }
        }

        void BuildBundles()
        {
            builtBundles = new List<BuiltBundle>();
            BuildHelper.SetBundleBuiltCallback(OnBundleBuilt);
			BuildHelper.SetBundleErrorCallback(OnBundleBuilt);
            BuildHelper.BuildBundles(bundlesToBuild);
            GetBuildResults();
        }

        /*
         * We can get the build results directly by watching at the bundles creation time
         */
        void GetBuildResults()
        {
            bool successfulExecution = true;
            var results = new BuildAssetBundleResults();
            results.builtBundles = new Dictionary<string, BuiltBundleResult>();
            for(int i = 0; i < builtBundles.Count; ++i)
            {
                BuiltBundle bundle = builtBundles[i];

                results.builtBundles[bundle.bundleName] = BuiltBundleResult.CreateFromBuiltBundle(bundle);
                successfulExecution &= results.builtBundles[bundle.bundleName].isSuccess;
            }

            if(!successfulExecution)
            {
                results.MarkAsFailed("Some bundles finished with errors");
            }

            logResults = results;
        }

        /*
         * This callback function will be passed to the Bundle Manager BuildHelper delegate
         */
        void OnBundleBuilt(BuiltBundle bundle)
        {
            builtBundles.Add(bundle);
        }

        public override void perform(ToolServiceParameters parameters)
        {
            Init();

            try
            {
                ParseParameters((BuildAssetBundleParameters)parameters);
                PrepareEnvironment();
                BuildBundles();
            }
            catch
            {
                // On exit, restore backup files if needed
                RestoreBackupFiles();
                throw;
            }
            // Force an explicit save of the build states
            BMDataAccessor.DumpAndSaveBundleBuildeStates();

            // On exit, restore backup files if needed
            RestoreBackupFiles();
        }
    }
}
