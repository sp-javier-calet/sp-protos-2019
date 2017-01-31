using System;
using System.Collections.Generic;
using System.Text;
using SocialPoint.Base;
using SocialPoint.Utils;
using UnityEngine;
using SocialPoint.Attributes;

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
        static List<string> _downloadingBundles = new List<string>();
        static readonly List<AssetBundleLoadOperation> _inProgressOperations = new List<AssetBundleLoadOperation>();
        static AssetBundlesParsedData _assetBundlesParsedData = new AssetBundlesParsedData();

        static string _baseDownloadingURL;

        [System.Diagnostics.Conditional("DEBUG_BUNDLES")]
        static void DebugLog(string msg)
        {
            Log.i(string.Format("[AssetBundleManager] {0}", msg));
        }

        public const string DefaultServer = "http://s3.amazonaws.com/int-sp-static-content/static";
        public const string DefaultGame = "basegame";

        public string Server = DefaultServer;
        public string Game = DefaultGame;

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
        }

        public void Init(AttrList bundlesAttrList)
        {
            // http://s3.amazonaws.com/int-sp-static-content/static/basegame/android_etc/1/test_scene_unity
            _baseDownloadingURL = string.Format("{0}/{1}/{2}/", Server, Game, Utility.GetPlatformName());
            DebugLog("BaseDownloadingURL: " + _baseDownloadingURL);

            _assetBundlesParsedData = LoadBundleData(bundlesAttrList);
        }


        static AssetBundlesParsedData LoadBundleData(AttrList bundlesAttrList)
        {
            var assetBundlesParsedData = new AssetBundlesParsedData();
            var dependencies = new Dictionary<string, List<string>>();

            for(int i = 0, bundlesAttrCount = bundlesAttrList.Count; i < bundlesAttrCount; i++)
            {
                var item = bundlesAttrList[i].AssertDic;
                var itemName = item.Get("name").AsValue.ToString();
                var itemVersion = item.Get("version").AsValue.ToInt();

                var dependenciesArray = item.Get("dependencies").AsValue.ToString().Split(',');
                var itemDependencies = (dependenciesArray.Length == 0 || dependenciesArray[0].Equals("null")) ? new List <string>() : new List <string>(dependenciesArray);
                dependencies.Add(itemName, itemDependencies);

                var assetBundleData = new AssetBundleParsedData(itemName, itemVersion);
                assetBundlesParsedData.Add(itemName, assetBundleData);
            }

            // fill bundle data dependencies.
            var depIter = dependencies.GetEnumerator();
            while(depIter.MoveNext())
            {
                var item = depIter.Current;

                AssetBundleParsedData bundleData;
                if(assetBundlesParsedData.TryGetValue(item.Key, out bundleData))
                {
                    var itemDependencies = item.Value;
                    AssetBundleParsedData bundleDataDep;
                    for(int i = 0, itemDependenciesCount = itemDependencies.Count; i < itemDependenciesCount; i++)
                    {
                        var dep = itemDependencies[i];
                        if(assetBundlesParsedData.TryGetValue(dep, out bundleDataDep))
                        {
                            bundleData.Dependencies.Add(bundleDataDep);
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

            return assetBundlesParsedData;
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

            // No dependencies are recorded, only the bundle itself is required.
            AssetBundleParsedData assetBundleData;
            if(_assetBundlesParsedData.TryGetValue(assetBundleName, out assetBundleData))
            {
                var dependencies = assetBundleData.Dependencies;
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
            }
            return bundle;
        }

        /// <summary>
        /// Returns true if certain asset bundle has been downloaded without checking
        /// whether the dependencies have been loaded.
        /// </summary>
        public static bool IsAssetBundleDownloaded(string assetBundleName)
        {
            return _loadedAssetBundles.ContainsKey(assetBundleName);
        }

        // Starts the download of the asset bundle identified by the given name, and asset bundles
        // that this asset bundle depends on.
        protected static void LoadAssetBundle(string assetBundleName)
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
        protected static bool LoadAssetBundleInternal(string assetBundleName)
        {
            // Already loaded.
            LoadedAssetBundle bundle;
            _loadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if(bundle != null)
            {
                bundle._referencedCount++;
                return true;
            }

            // @TODO: Do we need to consider the referenced count of WWWs?
            // In the demo, we never have duplicate WWWs as we wait LoadAssetAsync()/LoadLevelAsync() to be finished before calling another LoadAssetAsync()/LoadLevelAsync().
            // But in the real case, users can call LoadAssetAsync()/LoadLevelAsync() several times then wait them to be finished which might have duplicate WWWs.
            if(_downloadingBundles.Contains(assetBundleName))
            {
                return true;
            }

            AssetBundleParsedData assetBundleData;
            if(!_assetBundlesParsedData.TryGetValue(assetBundleName, out assetBundleData))
            {
                return false;
            }

            WWW download;

            const string slash = "/";
            string url = _baseDownloadingURL + assetBundleData.Version + slash + assetBundleName;

            download = WWW.LoadFromCacheOrDownload(url, assetBundleData.Version);

            _inProgressOperations.Add(new AssetBundleDownloadFromWebOperation(assetBundleName, download));
            _downloadingBundles.Add(assetBundleName);

            return false;
        }

        // Where we get all the dependencies and load them all.
        protected static void LoadDependencies(string assetBundleName)
        {
            AssetBundleParsedData assetBundleData;
            if(!_assetBundlesParsedData.TryGetValue(assetBundleName, out assetBundleData))
            {
                return;
            }

            var dependencies = assetBundleData.Dependencies;
            if(dependencies.Count == 0)
            {
                return;
            }

            // Record and load all dependencies.
            var dependenciesArray = new AssetBundleParsedData[dependencies.Count];
            dependencies.CopyTo(dependenciesArray);

            var itr = dependencies.GetEnumerator();
            while(itr.MoveNext())
            {
                var item = itr.Current;
                LoadAssetBundleInternal(item.Name);
            }
        }

        /// <summary>
        /// Unloads assetbundle and its dependencies.
        /// </summary>
        public static void UnloadAssetBundle(string assetBundleName)
        {
            UnloadAssetBundleInternal(assetBundleName);
        }

        protected static void UnloadAssetBundleInternal(string assetBundleName)
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

        #region IDisposable implementation

        public void Dispose()
        {
            _scheduler.Remove(this);
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
                _loadedAssetBundles.Add(download.AssetBundleName, download.AssetBundle);
            }
            else
            {
                string msg = string.Format("Failed downloading bundle {0} from {1}: {2}",
                                 download.AssetBundleName, download.GetSourceURL(), download.Error);
                _downloadingErrors.Add(download.AssetBundleName, msg);
            }

            _downloadingBundles.Remove(download.AssetBundleName);
        }

        /// <summary>
        /// Starts a load operation for an asset from the given asset bundle.
        /// </summary>
        public static AssetBundleLoadAssetOperation LoadAssetAsync(string assetBundleName, string assetName, Type type)
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
        public static AssetBundleLoadOperation LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive)
        {
            DebugLog("Loading " + levelName + " from " + assetBundleName + " bundle");

            LoadAssetBundle(assetBundleName);
            var operation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, isAdditive);

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
