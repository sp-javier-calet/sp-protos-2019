using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using SocialPoint.AssetSerializer.Helpers;
using SocialPoint.AssetSerializer.Exceptions;
using BM.Extensions;

#pragma warning disable 0618

public class BuiltBundle
{
    public string           bundleName;
    public List<string>     includs;
    public uint             crc = 0;
    public long             size = 0;
    public string           parent;
    public string           fullPath;
    public string           errorMessage;
    public List<string>     serializationErrors;
    public bool             isSerializationError;
    public bool             isScene;
    public bool             isSuccess;
    public bool             isSkipped;
    public bool             neededBuild;

    public static BuiltBundle CreateBuildResult(BundleData bundle, bool _isSuccess, bool _isSkipped, bool _neededBuild)
    {
        var builtBundle = new BuiltBundle(bundle);
        builtBundle.SetBuildResult(_isSuccess, _isSkipped, _neededBuild);
        return builtBundle;
    }

    public static BuiltBundle CreateSuccessfulResult(BundleData bundle, BundleBuildState bundleSate, bool _neededBuild)
    {
        var builtBundle = new BuiltBundle(bundle);
        builtBundle.SetBuildResult(true, false, _neededBuild);
        builtBundle.SetBuildState(bundleSate.crc, bundleSate.size);
        return builtBundle;
    }

    public static BuiltBundle CreateErrorResult(BundleData bundle, bool _isSerializationError, string _errMessage, string[] _serializationErrors=null)
    {
        var builtBundle = new BuiltBundle(bundle);
        builtBundle.SetBuildResult(false, false, false);
        builtBundle.SetErrorResult(_isSerializationError, _errMessage, _serializationErrors: _serializationErrors);
        return builtBundle;
    }

    public static BuiltBundle CreateErrorResult(string bundleName, string _errMessage)
    {
        var builtBundle = new BuiltBundle(bundleName);
        builtBundle.SetBuildResult(false, false, false);
        builtBundle.SetErrorResult(false, _errMessage);
        return builtBundle;
    }

    BuiltBundle(BundleData bundle)
    {
        bundleName = bundle.name;
        includs = bundle.includs;
        parent = bundle.parent;
        //Copied from 'GenerateOutputPathForBundle'
        fullPath = Path.Combine(BuildConfiger.InterpretedOutputPath, bundleName + "." + BuildConfiger.BundleSuffix);
        isScene = bundle.sceneBundle;
        serializationErrors = new List<string>();
    }

    BuiltBundle(string _bundleName) //Errored bundles may not have information
    {
        bundleName = _bundleName;
        includs = new List<string>();
        parent = "";
        //Copied from 'GenerateOutputPathForBundle'
        fullPath = "";
        isScene = false;
        serializationErrors = new List<string>();
    }

    void SetBuildResult(bool _isSuccess, bool _isSkipped, bool _neededBuild)
    {
        isSuccess = _isSuccess;
        isSkipped = _isSkipped;
        neededBuild = _neededBuild;
    }

    void SetBuildState(uint _crc, long _size)
    {
        crc = _crc;
        size = _size;
    }

    void SetErrorResult(bool _isSerializationError, string _errMessage, string[] _serializationErrors=null)
    {
        isSerializationError = _isSerializationError;
        errorMessage = _errMessage;
        if(_isSerializationError && _serializationErrors != null)
        {
            serializationErrors = RemoveDuplicatedSerializationErrors(_serializationErrors);
        }
    }

    List<string> RemoveDuplicatedSerializationErrors(string[] _serializationErrors)
    {
        HashSet<string> uniqueErrorsHash = new HashSet<string>();
        for(int i = 0; i < _serializationErrors.Length; ++i)
        {
            uniqueErrorsHash.Add(_serializationErrors[i]);
        }

        return uniqueErrorsHash.ToList();
    }

    public bool HasErrorMesage()
    {
        return errorMessage != null || errorMessage != String.Empty;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(string.Format("[BuiltBundle: {0}", bundleName));
        if(!isSuccess)
        {
            sb.Append(" failed");
            if(HasErrorMesage())
            {
                sb.Append(isSerializationError ? ", during serialization" : ", during build").Append(",\n").Append(errorMessage);
            }
        }
        else
        {
            if(isSkipped)
            {
                sb.Append(" skipped");
            }
            else
            {
                sb.Append(isSuccess ? " successful" : " failed");
            }
        }

        sb.Append("]");
        return sb.ToString();
    }
}

public class BuildProcessException : Exception
{
    public BuildProcessException(string message) : base(message)
    {
    }
}

/**
 * Build helper contains APIs for bundle building.
 * You can use this to custom your own build progress.
 */ 
public class BuildHelper
{
    public delegate void BundleBuiltDelegate(BuiltBundle bundle);
    public delegate void BundleErrorDelegate(BuiltBundle bundle);
    static BundleBuiltDelegate OnBundleBuiltCallback = DefaultBundleBuiltCallback;
    static BundleErrorDelegate OnBundleErrorCallback = DefaultBundleErrorCallback;

    static void DefaultBundleBuiltCallback(BuiltBundle bundle)
    {
        Debug.Log("[DefaultBundleBuiltCallback: " + bundle.ToString() + "]");
    }

    static void DefaultBundleErrorCallback(BuiltBundle bundle)
    {
        Debug.LogError("[DefaultBundleErrorCallback: " + bundle.ToString() + "]");
    }

    public static void SetBundleBuiltCallback(BundleBuiltDelegate callback)
    {
        OnBundleBuiltCallback = callback;
    }

    public static void SetBundleErrorCallback(BundleErrorDelegate callback)
    {
        OnBundleErrorCallback = callback;
    }

    /**
     * Copy the configeration files to target directory.
     */ 
    public static void ExportBMDatasToOutput()
    {
        string exportPath = BuildConfiger.InterpretedOutputPath;
        if(!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        uint crc = 0;
        if(!BuildAssetBundle(new string[] {
            BMDataAccessor.Paths.BundleDataPath,
            BMDataAccessor.Paths.BundleBuildStatePath,
            BMDataAccessor.Paths.BMConfigerPath
        }, Path.Combine(exportPath, "BM.data"), out crc))
        {
            Debug.LogError("Failed to build bundle of config files.");
        }

        BuildHelper.ExportBundleDataFileToOutput();
        BuildHelper.ExportBundleBuildDataFileToOutput();
        BuildHelper.ExportBMConfigerFileToOutput();
    }

    /**
     * Copy the bundle datas to target directory.
     */ 
    public static void ExportBundleDataFileToOutput()
    {
        string exportPath = BuildConfiger.InterpretedOutputPath;
        if(!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        File.Copy(BMDataAccessor.Paths.BundleDataPath, 
                  Path.Combine(exportPath, Path.GetFileName(BMDataAccessor.Paths.BundleDataPath)), 
                    true);
    }
    
    /**
     * Copy the bundle build states to target directory.
     */ 
    public static void ExportBundleBuildDataFileToOutput()
    {
        string exportPath = BuildConfiger.InterpretedOutputPath;
        if(!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        File.Copy(BMDataAccessor.Paths.BundleBuildStatePath, 
                  Path.Combine(exportPath, Path.GetFileName(BMDataAccessor.Paths.BundleBuildStatePath)), 
                    true);
    }
    
    /**
     * Copy the bundle manager configeration file to target directory.
     */ 
    public static void ExportBMConfigerFileToOutput()
    {
        string exportPath = BuildConfiger.InterpretedOutputPath;
        if(!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        File.Copy(BMDataAccessor.Paths.BMConfigerPath, 
                  Path.Combine(exportPath, Path.GetFileName(BMDataAccessor.Paths.BMConfigerPath)), 
                    true);
    }
    
    /**
     * Detect if the bundle needs to be rebuilt:
     * - TRUE. If the bundle file doesn't exist
     * - TRUE. If the bundle definition has been changed(dependency tree changes, deterministic or compressed changed) after its last build(by checking the file modification time)
     * - TRUE. If the bundle have different dependencies than it had in its last build
     * - TRUE. If any of the dependent asset files have been changed from the last build(by checking the asset files modification time)
     * - TRUE. If the bundle has a parent and the parent needs to be rebuilt
     * - TRUE. If the bundle changed its platform target or its androidSubTarget(android texture compression format)
     * - FALSE. Otherwise
     */ 
    public static bool IsBundleNeedBuild(BundleData bundle)
    {   
        string outputPath = GenerateOutputPathForBundle(bundle.name);
        if(!File.Exists(outputPath))
        {
            return true;
        }
        
        BundleBuildState bundleBuildState = BundleManager.GetBuildStateOfBundle(bundle.name);
        DateTime lastBuildTime = BMUtils.GetLastWriteTime(outputPath);
        DateTime bundleChangeTime = bundleBuildState.changeTime == -1 ? DateTime.MaxValue : DateTime.FromBinary(bundleBuildState.changeTime);
        if(System.DateTime.Compare(lastBuildTime, bundleChangeTime) < 0)
        {
            return true;
        }
        
        string[] assetPaths = GetAssetsFromPaths(BundleManager.GUIDsToPaths(bundle.includeGUIDs.ToArray()), bundle.sceneBundle);
        string[] dependencies = AssetDatabase.GetDependencies(assetPaths);
        if(!EqualStrArray(dependencies, bundleBuildState.lastBuildDependencies))
        {
            return true;
        } // Build depenedencies list changed.
        
        foreach(string file in dependencies)
        {
            if(DateTime.Compare(lastBuildTime, BMUtils.GetLastWriteTime(file)) < 0)
            {
                return true;
            }
            string meta = file + ".meta";
            if(DateTime.Compare(lastBuildTime, BMUtils.GetLastWriteTime(meta)) < 0)
            {
                return true;
            }
        }

        if(bundle.parent != "")
        {
            BundleData parentBundle = BundleManager.GetBundleData(bundle.parent);
            if(parentBundle != null)
            {
                if(IsBundleNeedBuild(parentBundle))
                {
                    return true;
                }
            }
            else
            {
                Debug.LogError("Cannot find bundle");
                OnBundleErrorCallback(BuiltBundle.CreateErrorResult(bundle, false, String.Format("Cannot find parent {0} bundle of {1}", bundle.parent, bundle.name)));
                return false;
            }
        }

        if((int)BMDataAccessor.Urls.bundleTarget != bundleBuildState.platform)
        {
            return true;
        }

        if(BMDataAccessor.Urls.bundleTarget == BuildPlatform.Android &&
            (int)EditorUserBuildSettings.androidBuildSubtarget != bundleBuildState.androidSubTarget)
        {
            return true;
        }
        
        return false;
    }
    
    /**
     * Build all bundles.
     */
    public static void BuildAll()
    {   
        BuildBundles(BundleManager.bundles.Select(bundle => bundle.name).ToArray());
    }
    
    /**
     * Force rebuild all bundles.
     */
    public static void RebuildAll()
    {
        foreach(BundleBuildState bundle in BundleManager.buildStates)
            bundle.lastBuildDependencies = null;
        
        BuildAll();
    }
    
    /**
     * Build bundles.
     */
    public static void BuildBundles(string[] bundles)
    {
        if(BuildConfiger.BundleBuildTarget == BuildPlatform.Android)
        {
            var prevTxtFmt = BuildConfiger.BundleTextureFormat;
            BuildConfiger.BundleTextureFormat = EditorUserBuildSettings.androidBuildSubtarget.ToString();
            if(prevTxtFmt != BuildConfiger.BundleTextureFormat)
            {
                BMDataAccessor.SaveUrls();
            }
        }

        Dictionary<string, List<string>> buildingRoutes = new Dictionary<string, List<string>>();
        foreach(string bundle in bundles)
            AddBundleToBuildList(bundle, ref buildingRoutes);

        try
        {
            foreach(var buildRoute in buildingRoutes)
            {
                BundleData bundle = BundleManager.GetBundleData(buildRoute.Key);
                if(bundle != null)
                {
                    BuildBundleTree(bundle, buildRoute.Value);
                }
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            // delete BM_JSON_Data folder so we dont generate garbage in the project
            BundleManagerJSON.JSONHelper.RemoveSerializedDataFile();
            // delete BM_JSON_PREFAB folder for possible temporal prefabs
            BundleManagerJSON.JSONHelper.Clear();
            // call post build procedures
            CallBuildBundleProcedures<BBPPostBuild>();

            AssetDatabase.Refresh();
        }
    }

    //CUSTOM:
    /**
	 * This step will call any defined BuildBundleProcedures that derive from T at runtime. It will load using reflection the
	 * defined clases
	 */
    public static void CallBuildBundleProcedures<T>() where T : BuildBundleProcedure
    {
        if(BuildConfiger.UseCustomBuildBundleProcedures)
        {
            string[] procedures = BuildConfiger.BuildBundleProceduresArray();
            Type baseType = typeof(T);

            for(int i = 0; i < procedures.Length; ++i)
            {
                // does the defined procedure type exist in the assembly ?
                var procName = procedures[i];
                Type procType = Type.GetType(procName);
                if(procType == null)
                {
                    Debug.LogError(String.Format("Invalid BuildBundleProcedure: '{0}' type not found.", procName));
                    continue;
                }

                // is the defined procedure a subtype of the calling procedure ?
                else if(!baseType.IsAssignableFrom(procType))
                {
                    continue;
                }
                Debug.Log(String.Format("Calling custom {1}: {0}", baseType.Name, procName));
                
                var procHndl = (T)Activator.CreateInstance(procType);
                procHndl.run();
            }
        }
    }
    //
    
    internal static void AddBundleToBuildList(string bundleName, ref Dictionary<string, List<string>> buildingRoutes)
    {
        //Check bundle name consistency
        if(bundleName.Contains(' '))
        {
            Debug.LogError("Bundle name cannot contain spaces " + bundleName);
            OnBundleErrorCallback(BuiltBundle.CreateErrorResult(bundleName, "Bundle name cannot contain spaces " + bundleName));
            return;
        }
        BundleData bundle = BundleManager.GetBundleData(bundleName);
        if(bundle == null)
        {
            Debug.LogError("Cannot find bundle " + bundleName);
            OnBundleErrorCallback(BuiltBundle.CreateErrorResult(bundleName, "Cannot find bundle " + bundleName));
            return;
        }
            
        if(BuildHelper.IsBundleNeedBuild(bundle))
        {
            string rootName = BundleManager.GetRootOf(bundle.name);
            if(buildingRoutes.ContainsKey(rootName))
            {
                if(!buildingRoutes[rootName].Contains(bundle.name))
                {
                    buildingRoutes[rootName].Add(bundle.name);
                }
                else
                {
                    Debug.LogError("Bundle name duplicated: " + bundle.name);
                    OnBundleErrorCallback(BuiltBundle.CreateErrorResult(bundleName, "Bundle name duplicated: " + bundle.name));
                    return;
                }
            }
            else
            {
                List<string> buildingList = new List<string>();
                buildingList.Add(bundle.name);
                buildingRoutes.Add(rootName, buildingList);
            }
        }
        else
        {
            OnBundleBuiltCallback(BuiltBundle.CreateBuildResult(bundle, true, true, false));
        }
    }

    /*
     * The parameter isNeededBuild specifies that this bundle AND every child is requested to be rebuilt and
     * that their dependencies may have changed. A false value does not enfoce a needed build.
     */
    internal static bool BuildBundleTree(BundleData bundle, List<string> requiredBuildList, bool isNeededBuild=false)
    {
        Debug.Log("<color=orange>BuildBundleTree</color>");

        BuildPipeline.PushAssetDependencies();

        if(BundleManagerJSON.JSONConfig.ExportWithBundle)
        {
            bool serialization_succ = true;

            Debug.Log("<color=orange>BuildUnityObjectAnnotatorSingleton - BeginBuilding</color>");
            
            if(BundleManagerJSON.JSONConfig.ExcludedComponents != null && BundleManagerJSON.JSONConfig.ExcludedComponents.Length > 0)
            {
                ComponentHelper.ComponentsExcluded = BundleManagerJSON.JSONConfig.ExcludedComponents;
            }

            BuildUnityObjectAnnotatorSingleton.Clear();
            BuildUnityObjectAnnotatorSingleton.BeginBuilding();

            try
            {
                bundle.jsonDataAssetsGUIDS = BundleManagerJSON.JSONHelper.BuildJSONFromBundleData(bundle);
            }
            catch(SerializationProcessException e)
            {
                serialization_succ = false;
                OnBundleErrorCallback(BuiltBundle.CreateErrorResult(bundle, true, e.Message, e.serialization_errors));
                // Also try to restore the assets to their original state(in case the behaviours were removed)
                if(bundle.sceneBundle)
                {
                    EditorApplication.OpenScene(EditorApplication.currentScene);
                }
            }
            catch(Exception e)
            {
                serialization_succ = false;
                OnBundleErrorCallback(BuiltBundle.CreateErrorResult(bundle, true, e.Message));
                
                // Also try to restore the assets to their original state(in case the behaviours were removed)
                if(bundle.sceneBundle)
                {
                    EditorApplication.OpenScene(EditorApplication.currentScene);
                }
            }
            finally
            {
                BuildUnityObjectAnnotatorSingleton.EndBuilding();
            }

            if(!serialization_succ)
            {
                Debug.LogError(bundle.name + " build failed.");
                BuildPipeline.PopAssetDependencies();
                return false;
            }

            #if UNITY_EDITOR_OSX

            // Debug contents on bundle creation from OSX editor
            foreach(string jsonDataGUID in bundle.jsonDataAssetsGUIDS)
            {
                TextAsset jsonDataAsset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(jsonDataGUID), typeof(TextAsset)) as TextAsset;
                var tmpPath = "/tmp/" + BMDataAccessor.GetUniquePlatformAssetDir();
                if(!Directory.Exists(tmpPath))
                {
                    Directory.CreateDirectory(tmpPath);
                }
                using(System.IO.StreamWriter file = new System.IO.StreamWriter(tmpPath + "/" + jsonDataAsset.name + ".json"))
                {
                    file.Write(jsonDataAsset.text);
                }
            }
            #endif
        }

        bool bundleNeededBuild = isNeededBuild | IsBundleNeedBuild(bundle);

        bool succ = false;
        try
        {
            succ = BuildSingleBundle(bundle);
            BuiltBundle buildResults;
            if(succ)
            {
                BundleBuildState buildState = BundleManager.GetBuildStateOfBundle(bundle.name);
                buildResults = BuiltBundle.CreateSuccessfulResult(bundle, buildState, bundleNeededBuild);
            }
            else
            {
                buildResults = BuiltBundle.CreateBuildResult(bundle, false, false, bundleNeededBuild);
            }
            OnBundleBuiltCallback(buildResults);
        }
        catch(BuildProcessException e)
        {
            succ = false;
            OnBundleErrorCallback(BuiltBundle.CreateErrorResult(bundle, false, e.Message));
        }

        if(!succ)
        {
            Debug.LogError(bundle.name + " build failed.");
            BuildPipeline.PopAssetDependencies();
            return false;
        }
        else
        {
            Debug.Log(bundle.name + " build succeed.");
        }

        if(!BundleManagerJSON.JSONConfig.UsePrefabCopies && bundle.jsonDataAssetsGUIDS.Count > 0)
        {
            if(BundleManagerJSON.JSONConfig.ExcludedComponents != null && BundleManagerJSON.JSONConfig.ExcludedComponents.Length > 0)
            {
                ComponentHelper.ComponentsExcluded = BundleManagerJSON.JSONConfig.ExcludedComponents;
            }
            BundleManagerJSON.JSONHelper.ReassignBehavioursFromBundleData(bundle);
        }

        // Reload scene or restore prefabs to leave the original objects intact
        if(bundle.sceneBundle && BundleManagerJSON.JSONConfig.ExportWithBundle)
        {
            EditorApplication.OpenScene(EditorApplication.currentScene);
        }
        
        foreach(string childName in bundle.children)
        {
            BundleData child = BundleManager.GetBundleData(childName);
            if(child == null)
            {
                Debug.LogError("Cannnot find bundle [" + childName + "]. Sth wrong with the bundle config data.");
                BuildPipeline.PopAssetDependencies();
                return false;
            }

            bool isDependingBundle = false;
            if(!bundleNeededBuild)
            {
                foreach(string requiredBundle in requiredBuildList)
                {
                    if(BundleManager.IsBundleDependOn(requiredBundle, childName))
                    {
                        isDependingBundle = true;
                        break;
                    }
                }
            }
            
            if(bundleNeededBuild || isDependingBundle || !BuildConfiger.DeterministicBundle)
            {
                succ = BuildBundleTree(child, requiredBuildList, isNeededBuild: bundleNeededBuild);
                if(!succ)
                {
                    BuildPipeline.PopAssetDependencies();
                    return false;
                }
            }
        }
        
        BuildPipeline.PopAssetDependencies();

        AssetDatabase.Refresh();

        return true;
    }
    
    // Get scene or plain assets from include paths
    internal static string[] GetAssetsFromPaths(string[] includeList, bool onlySceneFiles)
    {
        // Get all the includes file's paths
        List<string> files = new List<string>();
        foreach(string includPath in includeList)
        {
            files.AddRange(GetAssetsFromPath(includPath, onlySceneFiles));
        }
        
        return files.ToArray();
    }

    // Get scene or plain assets from path
    internal static string[] GetAssetsFromPath(string path, bool onlySceneFiles)
    {
        if(!File.Exists(path) && !Directory.Exists(path))
        {
            return new string[]{};
        }
        
        bool isDir = (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        bool isSceneFile = Path.GetExtension(path) == ".unity";
        if(!isDir)
        {
            if(onlySceneFiles && !isSceneFile)
            {
                // If onlySceneFiles is true, we can't add file without "unity" extension
                return new string[]{};
            }
            
            return new string[]{path};
        }
        else
        {
            string[] subFiles = null;
            if(onlySceneFiles)
            {
                subFiles = FindSceneFileInDir(path, SearchOption.AllDirectories);
            }
            else
            {
                subFiles = FindAssetsInDir(path, SearchOption.AllDirectories);
            }
            
            return subFiles;
        }
    }
    
    private static string[] FindSceneFileInDir(string dir, SearchOption option)
    {
        return Directory.GetFiles(dir, "*.unity", option);
    }
    
    private static string[] FindAssetsInDir(string dir, SearchOption option)
    {
        List<string> files = new List<string>(Directory.GetFiles(dir, "*.*", option));
        files.RemoveAll(x => x.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase) || x.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase));
        return files.ToArray();
    }
    
    private static bool BuildAssetBundle(string[] assetsList, string outputPath, out uint crc)
    {
        crc = 0;
        
        if(assetsList.Length == 0)
        {
            throw new BuildProcessException("No assets were provided for the asset bundle");
        }

        // Load all of assets in this bundle
        List<UnityEngine.Object> assets = new List<UnityEngine.Object>();
        List<string> assetsNames = new List<string>();
        foreach(string assetPath in assetsList)
        {
            // This special case is used on prefabs with its components removed (we use instances instead of the root prefab asset)
            if(BundleManagerJSON.JSONConfig.ExportWithBundle)
            {
                var prefabInstance = BundleManagerJSON.JSONHelper.FindPrefab(assetPath);
                if(prefabInstance != null)
                {
                    var baseAsset = AssetDatabase.LoadAssetAtPath(assetPath, prefabInstance.GetType());
                    assetsNames.Add(baseAsset.name);
                    assets.Add(prefabInstance);
                    continue;
                }
            }

            UnityEngine.Object[] assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            if(assetsAtPath != null || assetsAtPath.Length != 0)
            {
                for(int i = 0; i < assetsAtPath.Length; ++i)
                {
                    //Missing Components cause to get null entries
                    if (assetsAtPath[i] != null)
                    {
                        assetsNames.Add(assetsAtPath[i].name);
                        assets.Add(assetsAtPath[i]);
                    }
                }
            }
            else
            {
                throw new BuildProcessException("Cannnot load [" + assetPath + "] as asset object");
            }
        }

        // Build bundle
#if UNITY_4_2 || UNITY_4_1 || UNITY_4_0
        bool succeed = BuildPipeline.BuildAssetBundleExplicitAssetNames(    assets.ToArray(),
                                                                            assetsNames.ToArray(),
                                                                            outputPath, 
                                                                            CurrentBuildAssetOpts,
                                                                            BuildConfiger.UnityBuildTarget);
#else
        bool succeed = BuildPipeline.BuildAssetBundleExplicitAssetNames(assets.ToArray(),
                                                                            assetsNames.ToArray(),
                                                                            outputPath,
                                                                            out crc,
                                                                            CurrentBuildAssetOpts,
                                                                            BuildConfiger.UnityBuildTarget);
#endif
        return succeed;
    }

    private static BuildAssetBundleOptions CurrentBuildAssetOpts
    {
        get
        {
            return  (BMDataAccessor.BMConfiger.compress ? 0 : BuildAssetBundleOptions.UncompressedAssetBundle) |
                (!BMDataAccessor.BMConfiger.deterministicBundle ? 0 : BuildAssetBundleOptions.DeterministicAssetBundle) |
                BuildAssetBundleOptions.CollectDependencies;
        }
    }
    
    private static bool BuildSceneBundle(string[] sceneList, string outputPath, out uint crc)
    {
        crc = 0;

        if(sceneList.Length == 0)
        {
            throw new BuildProcessException("No scenes were provided for the scene bundle");
        }

#if UNITY_4_2 || UNITY_4_1 || UNITY_4_0
        string error = BuildPipeline.BuildPlayer (sceneList, outputPath, BuildConfiger.UnityBuildTarget, BuildOptions.BuildAdditionalStreamedScenes | CurrentBuildSceneOpts);
#else
        string error = BuildPipeline.BuildStreamedSceneAssetBundle(sceneList, outputPath, BuildConfiger.UnityBuildTarget, out crc, CurrentBuildSceneOpts);
#endif
        return error == "";
    }

    private static BuildOptions CurrentBuildSceneOpts
    {
        get
        {
            return  BMDataAccessor.BMConfiger.compress ? 0 : BuildOptions.UncompressedAssetBundle;
        }
    }
    
    private static bool BuildSingleBundle(BundleData bundle)
    {
        // Prepare bundle output dictionary
        string outputPath = GenerateOutputPathForBundle(bundle.name);
        string bundleStoreDir = Path.GetDirectoryName(outputPath);
        if(!Directory.Exists(bundleStoreDir))
        {
            Directory.CreateDirectory(bundleStoreDir);
        }
        
        // Start build

        List<string> guids = new List<string>();
        guids.AddRange(bundle.includeGUIDs);
        guids.AddRange(bundle.jsonDataAssetsGUIDS);
        
        string[] assetPaths = GetAssetsFromPaths(BundleManager.GUIDsToPaths(guids.ToArray()), bundle.sceneBundle);
        bool succeed = false;
        uint crc = 0;

        BuildProcessException exc = null;
        try
        {
            if(bundle.sceneBundle)
            {
                succeed = BuildSceneBundle(assetPaths, outputPath, out crc);
            }
            else
            {
                succeed = BuildAssetBundle(assetPaths, outputPath, out crc);
            }
        }
        catch(BuildProcessException e)
        {
            succeed = false;
            exc = e;
        }
        
        // Remember the assets for next time build test
        BundleBuildState buildState = BundleManager.GetBuildStateOfBundle(bundle.name);
        if(succeed)
        {
            if(buildState.changeTime == -1)
            {
                buildState.changeTime = BMUtils.GetLastWriteTime(outputPath).ToBinary();
            }
            buildState.lastBuildDependencies = AssetDatabase.GetDependencies(assetPaths);
            buildState.version++;
            if(buildState.version == int.MaxValue)
            {
                buildState.version = 0;
            }

            buildState.crc = crc;
            FileInfo bundleFileInfo = new FileInfo(outputPath);
            buildState.size = bundleFileInfo.Length;
            buildState.platform = (int)BMDataAccessor.Urls.bundleTarget;
            buildState.androidSubTarget = BMDataAccessor.Urls.bundleTarget != BuildPlatform.Android ? -1 :
                (int)EditorUserBuildSettings.androidBuildSubtarget;
        }
        
        BMDataAccessor.SaveBundleBuildeStates();

        //reraise the exception if ocurred after finishing steps have been done
        if(exc != null)
        {
            throw exc;
        }

        return succeed;
    }
    
    private static bool EqualStrArray(string[] strList1, string[] strList2)
    {
        if(strList1 == null || strList2 == null)
        {
            return false;
        }
        
        if(strList1.Length != strList2.Length)
        {
            return false;
        }
        
        for(int i = 0; i < strList1.Length; ++i)
        {
            if(strList1[i] != strList2[i])
            {
                return false;
            }
        }
        
        return true;
    }
    
    private static string GenerateOutputPathForBundle(string bundleName)
    {
        return Path.Combine(BuildConfiger.InterpretedOutputPath, bundleName + "." + BuildConfiger.BundleSuffix);
    }
}
