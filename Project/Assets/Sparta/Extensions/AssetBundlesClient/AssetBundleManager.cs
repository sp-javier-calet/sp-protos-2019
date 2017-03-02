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

    public class LocalAssetBundleManager : AssetBundleManager
    {
        public override void Setup()
        {
            _baseDownloadingURL = Path.Combine(PathsManager.StreamingAssetsPath, Utility.GetPlatformName());
            LoadBundleData(GetLocalBundlesDataAttrList());
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

        public override IEnumerator LoadAssetAsyncRequest(string assetBundleName, string assetName, Type type, Action<AssetBundleLoadAssetOperation> onRequestChanged)
        {
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

        public override IEnumerator LoadLevelAsyncRequest(string assetBundleName, string levelName, AssetBundleLoadLevelOperation.LoadSceneBundleMode loadSceneMode, Action<AssetBundleLoadLevelOperation> onRequestChanged)
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

        protected override void AddAssetBundleLoadOperation(AssetBundleParsedData assetBundleData)
        {
            const string slash = "/";
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(_baseDownloadingURL);
            stringBuilder.Append(slash);
            stringBuilder.Append(assetBundleData.Name);
            var url = stringBuilder.ToString();

            _inProgressOperations.Add(new AssetBundleLoadLocalOperation(assetBundleData.Name, url));
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

        protected readonly List<AssetBundleLoadOperation> _inProgressOperations = new List<AssetBundleLoadOperation>();
        protected AssetBundlesParsedData _assetBundlesParsedData = new AssetBundlesParsedData();
        protected string _baseDownloadingURL;

        static LocalAssetBundleManager _localAssetBundleManager;

        [System.Diagnostics.Conditional("DEBUG_BUNDLES")]
        static void DebugLog(string msg)
        {
            Log.i("AssetBundleManager", msg);
        }

        public const string DefaultServer = "http://s3.amazonaws.com/int-sp-static-content/static";
        public const string DefaultGame = "basegame";
        public const int DefaultMaxConcurrentDownloads = 2;

        public string Server = DefaultServer;
        public string Game = DefaultGame;
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

            protected get
            {
                return _scheduler;
            }
        }

        public virtual void Setup()
        {
            // http://s3.amazonaws.com/int-sp-static-content/static/basegame/android_etc/1/test_scene_unity
            _baseDownloadingURL = string.Format("{0}/{1}/{2}", Server, Game, Utility.GetPlatformName());
            DebugLog("BaseDownloadingURL: " + _baseDownloadingURL);

            _localAssetBundleManager = new LocalAssetBundleManager();
            _localAssetBundleManager.Scheduler = Scheduler;
            _localAssetBundleManager.CoroutineRunner = CoroutineRunner;
            _localAssetBundleManager.Setup();
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
                        LoadBundleData(bundleData.Get(bundleDataKey).AssertList);
                    }
                }
            }
        }

        protected void LoadBundleData(AttrList bundlesAttrList)
        {
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

                var assetBundleData = new AssetBundleParsedData(itemName, itemVersion);
                _assetBundlesParsedData.Add(itemName, assetBundleData);
            }

            // fill bundle data dependencies.
            var depIter = dependencies.GetEnumerator();
            while(depIter.MoveNext())
            {
                var item = depIter.Current;

                var bundleData = GetAssetBundleParsedData(item.Key);
                if(bundleData != null)
                {
                    var itemDependencies = item.Value;
                    for(int i = 0, itemDependenciesCount = itemDependencies.Count; i < itemDependenciesCount; i++)
                    {
                        var dep = itemDependencies[i];
                        var bundleDataDep = GetAssetBundleParsedData(dep);
                        if(bundleDataDep != null)
                        {
                            bundleData.Dependencies.Add(bundleDataDep);
                        }
                    }
                }
            }
            depIter.Dispose();

            // expand those dependencies
            var iter = _assetBundlesParsedData.GetEnumerator();
            while(iter.MoveNext())
            {
                var assetBundleData = iter.Current.Value;
                assetBundleData.Dependencies = ExpandDependencies(assetBundleData);
            }
            iter.Dispose();
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
        public LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, out string error)
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

        AssetBundleParsedData GetAssetBundleParsedData(string assetBundleName)
        {
            AssetBundleParsedData assetBundleData;
            _assetBundlesParsedData.TryGetValue(assetBundleName, out assetBundleData);
            return assetBundleData;
        }

        static AssetBundleParsedData GetLocalAssetBundleParsedData(string assetBundleName)
        {
            AssetBundleParsedData assetBundleData;
            _localAssetBundleManager._assetBundlesParsedData.TryGetValue(assetBundleName, out assetBundleData);
            return assetBundleData;
        }

        // Starts the download of the asset bundle identified by the given name, and asset bundles
        // that this asset bundle depends on.
        void LoadAssetBundle(string assetBundleName)
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
        bool LoadAssetBundleInternal(string assetBundleName)
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

            AddAssetBundleLoadOperation(assetBundleData);

            _downloadingBundles.Add(assetBundleName);

            return false;
        }

        protected virtual void AddAssetBundleLoadOperation(AssetBundleParsedData assetBundleData)
        {
            const string slash = "/";
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(_baseDownloadingURL);
            stringBuilder.Append(slash);
            stringBuilder.Append(assetBundleData.Version);
            stringBuilder.Append(slash);
            stringBuilder.Append(assetBundleData.Name);
            var url = stringBuilder.ToString();

            //@TODO: replace with DownloadHandlerAssetBundle when we all upgrade to unity 5.5
            // we will need to provide the CRC also..
            // https://unity3d.com/es/learn/tutorials/topics/best-practices/assetbundle-fundamentals#AssetBundleDownloadHandler
            var download = WWW.LoadFromCacheOrDownload(url, assetBundleData.Version);
            _inProgressOperations.Add(new AssetBundleDownloadFromWebOperation(assetBundleData.Name, download));
        }

        // Where we get all the dependencies and load them all.
        void LoadDependencies(string assetBundleName)
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

        void ProcessFinishedOperation(AssetBundleLoadOperation operation)
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

        protected IEnumerator WaitForReady()
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

        bool UseRemoteAsset(string assetBundleName)
        {
            var remoteAsset = GetAssetBundleParsedData(assetBundleName);
            var localAsset = GetLocalAssetBundleParsedData(assetBundleName);

            int remoteAssetVersion = remoteAsset != null ? remoteAsset.Version : 0;
            int localAssetVersion = localAsset != null ? localAsset.Version : 0;

            Debug.LogWarning("remoteAssetVersion: " + remoteAssetVersion);
            Debug.LogWarning("localAssetVersion: " + localAssetVersion);

            return remoteAssetVersion > localAssetVersion;
        }

        public virtual IEnumerator LoadAssetAsyncRequest(string assetBundleName, string assetName, Type type, Action<AssetBundleLoadAssetOperation> onRequestChanged)
        {
            if(UseRemoteAsset(assetBundleName))
            {
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
            else
            {
                yield return _localAssetBundleManager.LoadAssetAsyncRequest(assetBundleName, assetName, type, onRequestChanged);
            }
        }

        public virtual IEnumerator LoadLevelAsyncRequest(string assetBundleName, string levelName, AssetBundleLoadLevelOperation.LoadSceneBundleMode loadSceneMode, Action<AssetBundleLoadLevelOperation> onRequestChanged)
        {
            if(UseRemoteAsset(assetBundleName))
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
            else
            {
                yield return _localAssetBundleManager.LoadLevelAsyncRequest(assetBundleName, levelName, loadSceneMode, onRequestChanged);
            }
        }

        /// <summary>
        /// Starts a load operation for an asset from the given asset bundle.
        /// </summary>
        protected AssetBundleLoadAssetOperation LoadAssetAsync(string assetBundleName, string assetName, Type type)
        {
            DebugLog("Loading " + assetName + " from " + assetBundleName + " bundle");

            LoadAssetBundle(assetBundleName);
            var operation = new AssetBundleLoadAssetOperationFull(this, assetBundleName, assetName, type);

            _inProgressOperations.Add(operation);

            return operation;
        }

        /// <summary>
        /// Starts a load operation for a level from the given asset bundle.
        /// </summary>
        protected AssetBundleLoadLevelOperation LoadLevelAsync(string assetBundleName, string levelName, AssetBundleLoadLevelOperation.LoadSceneBundleMode loadSceneMode)
        {
            DebugLog("Loading " + levelName + " from " + assetBundleName + " bundle");

            LoadAssetBundle(assetBundleName);
            var operation = new AssetBundleLoadLevelOperation(this, assetBundleName, levelName, loadSceneMode);

            _inProgressOperations.Add(operation);

            return operation;
        }
    }
    // End of AssetBundleManager.


    public class AssetBundleParsedData
    {
        public readonly string Name;
        public readonly int Version;
        public HashSet<AssetBundleParsedData> Dependencies = new HashSet<AssetBundleParsedData>();

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

}
