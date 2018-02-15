using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Utils;
using UnityEngine;

namespace SocialPoint.AssetBundlesClient
{
    /// <summary>
    /// Loaded assetBundle contains the references count which can be used to
    /// unload dependent assetBundles automatically.
    /// </summary>
    public class LoadedAssetBundle
    {
        public AssetBundle _assetBundle;
        public int _referencedCount;

        internal event Action unload;

        internal void OnUnload()
        {
            _assetBundle.Unload(false);
            if(unload != null)
            {
                unload();
            }
        }

        public LoadedAssetBundle(AssetBundle assetBundle)
        {
            _assetBundle = assetBundle;
            _referencedCount = 1;
        }
    }

    /// <summary>
    /// Class takes care of loading assetBundle and its dependencies automatically
    /// </summary>
    public class AssetBundleManager : IUpdateable, IDisposable
    {
        static Dictionary<string, LoadedAssetBundle> _loadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();
        static Dictionary<string, string> _downloadingErrors = new Dictionary<string, string>();
        static readonly List<string> _downloadingBundles = new List<string>();
        static readonly List<AssetBundleLoadOperation> _inProgressOperations = new List<AssetBundleLoadOperation>();

        // remote
        static AssetBundlesParsedData _remoteAssetBundlesParsedData = new AssetBundlesParsedData();
        static string _baseDownloadingURL;

        //local
        static string _localAssetBundlesPath;

        //merged
        static HashSet<string> _parsedBundlesNames = new HashSet<string>();
        static AssetBundlesParsedData _mergedAssetBundlesParsedData = new AssetBundlesParsedData();
        static readonly List<string> _mergeIssues = new List<string>();


        [System.Diagnostics.Conditional(DebugFlags.DebugAssetBundlesFlag)]
        static void DebugLog(string msg)
        {
            Log.i("AssetBundleManager", msg);
        }

        public const string DefaultServer = "http://s3.amazonaws.com/int-sp-static-content/static";
        public const string DefaultGame = "basegame";
        public const int DefaultMaxConcurrentDownloads = 2;

        public AssetBundleManagerInstaller.SettingsData Data{ get; set; }

        public int MaxConcurrentDownloads = DefaultMaxConcurrentDownloads;

        public ICoroutineRunner CoroutineRunner{ get; set; }

        IUpdateScheduler _scheduler;

        public IUpdateScheduler Scheduler
        {
            set
            {
                if(_scheduler != null)
                {
                    _scheduler.Remove(this);
                }
                _scheduler = value;
                if(_scheduler != null)
                {
                    _scheduler.Add(this);
                }
            }

            get
            {
                return _scheduler;
            }
        }

        public AssetBundleManager()
        {
            Data = new AssetBundleManagerInstaller.SettingsData{Server = DefaultServer, Game = DefaultGame, TransformNamesToLowercase = false};
        }

        public void Setup()
        {
            // http://s3.amazonaws.com/int-sp-static-content/static/basegame/android_etc/1/test_scene_unity
            _baseDownloadingURL = string.Format("{0}/{1}/{2}", Data.Server, Data.Game, Utility.GetPlatformName());
            DebugLog("BaseDownloadingURL: " + _baseDownloadingURL);

            SetupLocal();
        }

        public void Init(Attr data)
        {
            const string configKey = "config";
            const string bundleDataKey = "bundle_data";

            var dataDic = data != null ? data.AsDic : null;
            if(dataDic != null)
            {
                if(dataDic.ContainsKey(configKey))
                {
                    var configData = dataDic.Get(configKey).AssertDic;
                    if(configData.ContainsKey(bundleDataKey))
                    {
                        var bundleData = configData.Get(bundleDataKey).AssertDic;
                        LoadBundleData(bundleData.Get(bundleDataKey).AssertList, false);
                    }
                }
            }
        }

        static void ClearTemporaryData()
        {
            _remoteAssetBundlesParsedData.Clear();
            _parsedBundlesNames.Clear();
        }

        static void MergeParsedData()
        {
            var iterNames = _parsedBundlesNames.GetEnumerator();
            while(iterNames.MoveNext())
            {
                var assetBundleName = iterNames.Current;

                AssetBundleParsedData remoteAsset;
                _remoteAssetBundlesParsedData.TryGetValue(assetBundleName, out remoteAsset);
                int remoteAssetVersion = remoteAsset != null ? remoteAsset.Version : 0;

                AssetBundleParsedData localAsset;
                _mergedAssetBundlesParsedData.TryGetValue(assetBundleName, out localAsset);
                int localAssetVersion = localAsset != null ? localAsset.Version : 0;

                if(remoteAssetVersion > localAssetVersion && remoteAssetVersion > 0)
                {
                    remoteAsset.RemoteIsNewest = true;
                    if(_mergedAssetBundlesParsedData.ContainsKey(assetBundleName))
                    {
                        _mergedAssetBundlesParsedData[assetBundleName] = remoteAsset;
                    }
                    else
                    {
                        _mergedAssetBundlesParsedData.Add(assetBundleName, remoteAsset);
                    }
                }
            }
            iterNames.Dispose();

            UpdateDependencies();
            CheckMixedDependencies();
            ClearTemporaryData();
        }

        static void UpdateDependencies()
        {
            var iter = _mergedAssetBundlesParsedData.GetEnumerator();
            while(iter.MoveNext())
            {
                var dependencies = iter.Current.Value.Dependencies;
                var iterDeps = dependencies.GetEnumerator();
                var updatedDependencies = new HashSet<AssetBundleParsedData>();
                while(iterDeps.MoveNext())
                {
                    AssetBundleParsedData updatedDep;
                    if(_mergedAssetBundlesParsedData.TryGetValue(iterDeps.Current.Name, out updatedDep))
                    {
                        updatedDependencies.Add(updatedDep);
                    }
                }
                iterDeps.Dispose();
                iter.Current.Value.Dependencies = updatedDependencies;
            }
            iter.Dispose();
        }

        static void CheckMixedDependencies()
        {
            var iter = _mergedAssetBundlesParsedData.GetEnumerator();
            while(iter.MoveNext())
            {
                var assetBundle = iter.Current.Value;
                var remoteIsNewest = assetBundle.RemoteIsNewest;

                var dependencies = assetBundle.Dependencies;
                var iterDeps = dependencies.GetEnumerator();
                while(iterDeps.MoveNext())
                {
                    var assetBundleDep = iterDeps.Current;
                    var remoteDepIsNewest = assetBundleDep.RemoteIsNewest;

                    if(remoteIsNewest != remoteDepIsNewest)
                    {
                        string issue = string.Format("'{0}' '{1}' - '{2}' '{3}' dependency.", remoteIsNewest ? "Remote:" : "Local:", assetBundle.Name, remoteDepIsNewest ? "Remote:" : "Local:", assetBundleDep.Name);
                        _mergeIssues.Add(issue);
                    }
                }
                iterDeps.Dispose();
            }
            iter.Dispose();
        }

        static void LoadBundleData(AttrList bundlesAttrList, bool isLocal)
        {
            var assetBundlesParsedData = isLocal ? _mergedAssetBundlesParsedData : _remoteAssetBundlesParsedData;

            var dependencies = new Dictionary<string, List<string>>();

            for(int i = 0, bundlesAttrCount = bundlesAttrList.Count; i < bundlesAttrCount; i++)
            {
                var item = bundlesAttrList[i].AssertDic;
                var itemName = item.Get("name").AsValue.ToString();
                if(string.IsNullOrEmpty(itemName))
                {
                    continue;
                }
                var itemVersion = item.Get("version").AsValue.ToInt();

                var dependenciesList = item.Get("dependencies").AssertList;
                if(dependenciesList.Count > 0)
                {
                    var dependenciesArray = new string[dependenciesList.Count];
                    for(int j = 0, dependenciesListCount = dependenciesList.Count; j < dependenciesListCount; j++)
                    {
                        var dep = dependenciesList[j].AsValue.ToString();
                        dependenciesArray[j] = dep;
                    }
                    dependencies.Add(itemName, new List <string>(dependenciesArray));
                }

                if(!assetBundlesParsedData.ContainsKey(itemName))
                {
                    var assetBundleData = new AssetBundleParsedData(itemName, itemVersion);
                    assetBundlesParsedData.Add(itemName, assetBundleData);
                }
                else
                {
                    DebugLog(string.Format("Skipped BundleData {0} because it is repeated", itemName));
                }

                if(!_parsedBundlesNames.Contains(itemName))
                {
                    _parsedBundlesNames.Add(itemName);
                }
            }

            // fill bundle data dependencies.
            var depIter = dependencies.GetEnumerator();
            while(depIter.MoveNext())
            {
                var item = depIter.Current;

                AssetBundleParsedData assetBundleData;
                if(assetBundlesParsedData.TryGetValue(item.Key, out assetBundleData))
                {
                    var itemDependencies = item.Value;
                    for(int i = 0, itemDependenciesCount = itemDependencies.Count; i < itemDependenciesCount; i++)
                    {
                        AssetBundleParsedData bundleDataDep;
                        var dep = itemDependencies[i];
                        if(assetBundlesParsedData.TryGetValue(dep, out bundleDataDep))
                        {
                            assetBundleData.Dependencies.Add(bundleDataDep);
                        }
                    }
                }
            }
            depIter.Dispose();

            // expand those dependencies
            var iter = assetBundlesParsedData.GetEnumerator();
            while(iter.MoveNext())
            {
                var assetBundleData = iter.Current.Value;
                assetBundleData.Dependencies = ExpandDependencies(assetBundleData);
            }
            iter.Dispose();

            if(isLocal)
            {
                _mergedAssetBundlesParsedData = assetBundlesParsedData;
            }
            else
            {
                _remoteAssetBundlesParsedData = assetBundlesParsedData;
                if(_remoteAssetBundlesParsedData.Count > 0)
                {
                    MergeParsedData();
                }
            }
        }

        static HashSet<AssetBundleParsedData> ExpandDependencies(AssetBundleParsedData assetBundleData)
        {
            var expandedDependencies = new HashSet<AssetBundleParsedData>();
            var currentDependenciesItr = assetBundleData.Dependencies.GetEnumerator();
            while(currentDependenciesItr.MoveNext())
            {
                var item = currentDependenciesItr.Current;
                expandedDependencies.Add(item);
                var newDependencies = ExpandDependencies(item);
                var newDependenciesItr = newDependencies.GetEnumerator();
                while(newDependenciesItr.MoveNext())
                {
                    expandedDependencies.Add(newDependenciesItr.Current);
                }
                newDependenciesItr.Dispose();
            }
            currentDependenciesItr.Dispose();
            return expandedDependencies;
        }

        /// <summary>
        /// Retrieves an asset bundle that has previously been requested via LoadAssetBundle.
        /// Returns null if the asset bundle or one of its dependencies have not been downloaded yet.
        /// </summary>
        public static LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, out string error)
        {
            if(_downloadingErrors.TryGetValue(assetBundleName, out error))
            {
                return null;
            }

            LoadedAssetBundle bundle;
            _loadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if(bundle == null)
            {
                return null;
            }

            var assetBundleData = GetAssetBundleParsedData(assetBundleName);
            if(assetBundleData != null)
            {
                var dependencies = assetBundleData.Dependencies;

                // No dependencies are recorded, only the bundle itself is required.
                if(dependencies.Count == 0)
                {
                    return bundle;
                }
                var iter = dependencies.GetEnumerator();
                while(iter.MoveNext())
                {
                    var dependency = iter.Current;

                    if(_downloadingErrors.TryGetValue(dependency.Name, out error))
                    {
                        iter.Dispose();
                        return null;
                    }
                    // Wait all the dependent assetBundles being loaded.
                    LoadedAssetBundle dependentBundle;
                    _loadedAssetBundles.TryGetValue(dependency.Name, out dependentBundle);
                    if(dependentBundle == null)
                    {
                        iter.Dispose();
                        return null;
                    }
                }
                iter.Dispose();
                return bundle;
            }

            error = string.Format("Failed parsing bundle {0}", assetBundleName);
            return null;
        }

        /// <summary>
        /// Returns true if certain asset bundle has been downloaded without checking
        /// whether the dependencies have been loaded.
        /// </summary>
        public bool IsAssetBundleDownloaded(string assetBundleName)
        {
            return _loadedAssetBundles.ContainsKey(assetBundleName);
        }

        // Starts the download of the asset bundle identified by the given name, and asset bundles
        // that this asset bundle depends on.
        static void LoadAssetBundle(string assetBundleName)
        {
            DebugLog("Loading Asset Bundle : " + assetBundleName);

            // Check if the assetBundle has already been processed.
            bool isAlreadyProcessed = LoadAssetBundleInternal(assetBundleName);

            // Load dependencies.
            if(!isAlreadyProcessed)
            {
                LoadDependencies(assetBundleName);
            }
        }

        // Sets up download operation for the given asset bundle if it's not downloaded already.
        static bool LoadAssetBundleInternal(string assetBundleName)
        {
            // Already loaded.
            LoadedAssetBundle bundle;
            _loadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if(bundle != null)
            {
                bundle._referencedCount++;
                return true;
            }

            if(_downloadingBundles.Contains(assetBundleName))
            {
                return true;
            }

            var assetBundleData = GetAssetBundleParsedData(assetBundleName);
            if(assetBundleData == null)
            {
                return false;
            }

            if(assetBundleData.RemoteIsNewest)
            {
                AddAssetBundleLoadOperation(assetBundleData);
            }
            else
            {
                AddAssetBundleLoadLocalOperation(assetBundleData);
            }

            _downloadingBundles.Add(assetBundleName);

            return false;
        }

        static void AddAssetBundleLoadOperation(AssetBundleParsedData assetBundleData)
        {
            const string slash = "/";
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(_baseDownloadingURL);
            stringBuilder.Append(slash);
            stringBuilder.Append(assetBundleData.Version);
            stringBuilder.Append(slash);
            stringBuilder.Append(assetBundleData.Name);
            var url = stringBuilder.ToString();

            _inProgressOperations.Add(new AssetBundleDownloadFromUnityWebRequestOperation(assetBundleData.Name, assetBundleData.Version, url));
        }

        // Where we get all the dependencies and load them all.
        static void LoadDependencies(string assetBundleName)
        {
            var assetBundleData = GetAssetBundleParsedData(assetBundleName);
            if(assetBundleData == null)
            {
                return;
            }

            var dependencies = assetBundleData.Dependencies;
            if(dependencies.Count == 0)
            {
                return;
            }

            // Record and load all dependencies.
            var itr = dependencies.GetEnumerator();
            while(itr.MoveNext())
            {
                var item = itr.Current;
                LoadAssetBundleInternal(item.Name);
            }
            itr.Dispose();
        }

        /// <summary>
        /// Unloads assetbundle and its dependencies.
        /// </summary>
        public void UnloadAssetBundle(string assetBundleName)
        {
            string error;
            LoadedAssetBundle bundle = GetLoadedAssetBundle(assetBundleName, out error);
            if(bundle == null)
            {
                return;
            }

            if(--bundle._referencedCount == 0)
            {
                bundle.OnUnload();
                _loadedAssetBundles.Remove(assetBundleName);

                DebugLog(assetBundleName + " has been unloaded successfully");
            }
        }

        #region IDisposable implementation

        public void Dispose()
        {
            _loadedAssetBundles.Clear();
            _downloadingErrors.Clear();
            _downloadingBundles.Clear();
            _inProgressOperations.Clear();
            _remoteAssetBundlesParsedData.Clear();
            _parsedBundlesNames.Clear();
            _mergedAssetBundlesParsedData.Clear();
            _mergeIssues.Clear();
            _scheduler.Remove(this);
        }

        #endregion

        #region IUpdateable implementation

        public void Update()
        {
            // Update all in progress operations
            for(int i = 0; i < _inProgressOperations.Count;)
            {
                var operation = _inProgressOperations[i];
                if(operation.Update())
                {
                    i++;
                }
                else
                {
                    _inProgressOperations.RemoveAt(i);
                    ProcessFinishedOperation(operation);
                }
            }
        }

        #endregion

        static void ProcessFinishedOperation(AssetBundleLoadOperation operation)
        {
            var download = operation as AssetBundleDownloadOperation;
            if(download == null)
            {
                return;
            }

            if(download.Error == null)
            {
                _loadedAssetBundles.Add(download.AssetBundleName, download.AssetBundleLoaded);
            }
            else
            {
                if(!_downloadingErrors.ContainsKey(download.AssetBundleName))
                {
                    string msg = string.Format("Failed downloading bundle {0} from {1}: {2}",
                                     download.AssetBundleName, download.GetSourceURL(), download.Error);
                    _downloadingErrors.Add(download.AssetBundleName, msg);
                }
            }

            _downloadingBundles.Remove(download.AssetBundleName);
        }

        IEnumerator WaitForReady()
        {
            while(!Caching.ready)
            {
                yield return null;
            }

            if(_downloadingBundles.Count > MaxConcurrentDownloads)
            {
                yield return null;
            }
        }

        static AssetBundleParsedData GetAssetBundleParsedData(string assetBundleName)
        {
            AssetBundleParsedData assetBundleData;
            _mergedAssetBundlesParsedData.TryGetValue(assetBundleName, out assetBundleData);
            return assetBundleData;
        }

        static void SetupLocal()
        {
            _localAssetBundlesPath = Path.Combine(PathsManager.StreamingAssetsPath, Utility.GetPlatformName());
            LoadBundleData(GetLocalBundlesDataAttrList(), true);
        }

        static AttrList GetLocalBundlesDataAttrList()
        {
            const string bundleDataFile = "local_bundle_data.json";
            const string bundleDataKey = "local_bundle_data";

            string jsonPath = Path.Combine(PathsManager.StreamingAssetsPath, bundleDataFile);
            string json = FileUtils.ReadAllText(jsonPath);

            var bundlesAttrDic = new JsonAttrParser().ParseString(json).AssertDic;
            var bundleDataAttrList = bundlesAttrDic.Get(bundleDataKey).AssertList;
            return bundleDataAttrList;
        }

        static void AddAssetBundleLoadLocalOperation(AssetBundleParsedData assetBundleData)
        {
            const string slash = "/";
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(_localAssetBundlesPath);
            stringBuilder.Append(slash);
            stringBuilder.Append(assetBundleData.Name);
            var fullPath = stringBuilder.ToString();

            _inProgressOperations.Add(new AssetBundleLoadLocalOperation(assetBundleData.Name, fullPath));
        }

        public IEnumerator LoadAssetAsyncRequest(string assetBundleName, string assetName, Type type, Action<AssetBundleLoadAssetOperation> onRequestChanged)
        {
            if(Data.TransformNamesToLowercase)
            {
                assetBundleName = assetBundleName.ToLower();
                assetName = assetName.ToLower();
            }

            yield return WaitForReady();

            // Load asset from assetBundle.
            AssetBundleLoadAssetOperation request = LoadAssetAsync(assetBundleName, assetName, type);
            if(request == null)
            {
                yield break;
            }
            yield return CoroutineRunner.StartCoroutine(request);

            if(onRequestChanged != null)
            {
                onRequestChanged(request);
            }
        }

        public IEnumerator LoadLevelAsyncRequest(string assetBundleName, string levelName, AssetBundleLoadLevelOperation.LoadSceneBundleMode loadSceneMode, Action<AssetBundleLoadLevelOperation> onRequestChanged)
        {
            yield return WaitForReady();

            // Load level from assetBundle.
            AssetBundleLoadLevelOperation request = LoadLevelAsync(assetBundleName, levelName, loadSceneMode);
            if(request == null)
            {
                yield break;
            }
            yield return CoroutineRunner.StartCoroutine(request);

            if(onRequestChanged != null)
            {
                onRequestChanged(request);
            }
        }

        /// <summary>
        /// Starts a load operation for an asset from the given asset bundle.
        /// </summary>
        static AssetBundleLoadAssetOperation LoadAssetAsync(string assetBundleName, string assetName, Type type)
        {
            DebugLog("Loading " + assetName + " from " + assetBundleName + " bundle");

            LoadAssetBundle(assetBundleName);
            var operation = new AssetBundleLoadAssetOperationFull(assetBundleName, assetName, type);

            _inProgressOperations.Add(operation);

            return operation;
        }

        /// <summary>
        /// Starts a load operation for a level from the given asset bundle.
        /// </summary>
        static AssetBundleLoadLevelOperation LoadLevelAsync(string assetBundleName, string levelName, AssetBundleLoadLevelOperation.LoadSceneBundleMode loadSceneMode)
        {
            DebugLog("Loading " + levelName + " from " + assetBundleName + " bundle");

            LoadAssetBundle(assetBundleName);
            var operation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, loadSceneMode);

            _inProgressOperations.Add(operation);

            return operation;
        }

        public int NumberOfInProgressOperations()
        {
            return _inProgressOperations.Count;
        }
    }
    // End of AssetBundleManager.

    #region AssetBundleParsedData

    public class AssetBundleParsedData
    {
        public readonly string Name;
        public readonly int Version;
        public HashSet<AssetBundleParsedData> Dependencies = new HashSet<AssetBundleParsedData>();
        public bool RemoteIsNewest;

        public AssetBundleParsedData(string name, int version)
        {
            Name = name;
            Version = version;
        }

        public override string ToString()
        {
            const string tab = "\t";

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("AssetBundleData: ");
            stringBuilder.AppendLine("- name: " + Name);
            stringBuilder.AppendLine("- version: " + Version);
            stringBuilder.AppendLine("- dependencies: ");

            var itr = Dependencies.GetEnumerator();
            while(itr.MoveNext())
            {
                var item = itr.Current;
                stringBuilder.AppendLine(tab + item.Name);
            }
            itr.Dispose();
            return stringBuilder.ToString();
        }
    }

    public class AssetBundlesParsedData : Dictionary<string, AssetBundleParsedData>
    {
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            var itr = GetEnumerator();
            while(itr.MoveNext())
            {
                var item = itr.Current;
                stringBuilder.AppendLine(item.Value.ToString());
            }
            itr.Dispose();
            return stringBuilder.ToString();
        }
    }

    #endregion
}
