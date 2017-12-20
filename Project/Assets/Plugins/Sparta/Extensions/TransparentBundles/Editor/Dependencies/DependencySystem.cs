using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SocialPoint.TransparentBundles
{
    [InitializeOnLoad]
    public static class DependencySystem
    {
        public class BundleInfo
        {
            public string Guid;
            public string Path = string.Empty;
            public bool IsLocal;

            public BundleInfo()
            {
                Path = string.Empty;
            }

            public BundleInfo(string guid, bool isLocal = false)
            {
                Guid = guid;
                Path = AssetDatabase.GUIDToAssetPath(guid);
                IsLocal = isLocal;
            }
        }

        public static event Action<BundleDependenciesData> OnBundleAdded;
        public static event Action<string, BundleDependenciesData> OnBundleDeletedFromProject;
        public static event Action<BundleDependenciesData> OnBundleRemoved;
        public static event Action<BundleDependenciesData> OnAssetRemoved;
        public static event Action<BundleDependenciesData> OnBundleLocalChanged;
        public static event Action<string, LogType> OnLogMessage;

        static string _path = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "DependenciesManifest.json");

        public static BundlesManifest Manifest = new BundlesManifest();

        static DependencySystem()
        {
            if(File.Exists(_path))
            {
                Load();
            }

            OnLogMessage += LogMessageHandler;
            OnBundleAdded += x => OnLogMessage("Added new bundle " + x.AssetPath + " GUID: " + x.GUID, LogType.Log);
            OnBundleRemoved += x => OnLogMessage("Removed bundle " + x.AssetPath + " GUID: " + x.GUID, LogType.Log);
            OnAssetRemoved += x => OnLogMessage("This asset is no longer part of the manifest because it lost all its dependants: " + x.AssetPath + " GUID: " + x.GUID, LogType.Log);
            OnBundleLocalChanged += x => OnLogMessage("Bundle " + x.AssetPath + " changed local status to " + x.IsLocal, LogType.Log);
        }

        #region JSON

        public static void Load(string path = "")
        {
            if(string.IsNullOrEmpty(path))
            {
                path = _path;
            }

            Manifest = BundlesManifest.Load(path);
        }

        public static void Save(string path = "")
        {
            if(string.IsNullOrEmpty(path))
            {
                path = _path;
            }

            Manifest.Save(path);
        }

        #endregion

        #region APICalls

        /// <summary>
        /// Registers or refresh an asset as a manual bundled
        /// </summary>
        /// <param name = "info"></param>
        public static void RegisterManualBundledAsset(BundleInfo info)
        {
            AddOrUpdateAsset(info.Guid, info.IsLocal, true);

            Save();
        }

        /// <summary>
        /// Overload taking only guids. Defaults all bundles to remote (not included in build)
        /// </summary>
        /// <param name="userBundles">List of the guids of the assets to be user downloadable</param>
        public static void UpdateManifest(List<string> userBundles)
        {
            UpdateManifest(userBundles.ConvertAll(x => new BundleInfo(x)));
        }

        /// <summary>
        /// Updates the manifest with the BundleInfo passed by parameters. This will clear the manifest 
        /// and then register all the provided bundles as userbundles and calculate and include their
        /// dependencies
        /// </summary>
        /// <param name="userBundles">List of the guids of the assets to be user downloadable</param>
        public static void UpdateManifest(List<BundleInfo> userBundles)
        {
            var oldBundles = Manifest.GetValues().Where(x => x.IsExplicitlyBundled && !userBundles.Exists(y => y.Guid == x.GUID)).ToList();

            foreach(var bundleData in oldBundles)
            {
                if(OnLogMessage != null)
                {
                    OnLogMessage("Old assetbundle will no longer be a user defined Bundle since is not included in the input bundles: " + bundleData.GUID + " Old Path: " + bundleData.AssetPath, LogType.Warning);
                }
                // remove its condition of bundled asset and remove it if it is not a dependency
                RemoveAsset(bundleData.GUID);
            }

            // Adds or Refreshes all the bundles.
            foreach(BundleInfo info in userBundles)
            {
                AddOrUpdateAsset(info.Guid, info.IsLocal, true);
            }

            Save();
        }


        /// <summary>
        /// Refreshes all the manifest and updates all the dependencies with the current project
        /// </summary>
        public static void RefreshAll()
        {
            var explicitBundles = Manifest.GetDictionary().Where(x => x.Value.IsExplicitlyBundled);

            foreach(var pair in explicitBundles)
            {
                AddOrUpdateAsset(pair.Key);
            }

            Save();
        }


        /// <summary>
        /// Removes bundles from the manually bundled assets. 
        /// The asset may still be in the registry if another asset has it as a dependency and may still
        /// be a bundle depending on the AutoBundle policy
        /// </summary>
        /// <param name = "guids"></param>
        public static void RemoveBundles(params string[] guids)
        {
            foreach(string guid in guids)
            {
                RemoveAsset(guid);
            }

            Save();
        }

        /// <summary>
        /// Creates the data structure needed to build bundles with the Unity call
        /// </summary>
        /// <param name="bundleManifest">List of bundles calculated by Dependency System (Will take local data if null is passed)</param>
        /// <returns>Array of AssetBundleBuild structure to pass to BuildAssetBundles</returns>
        public static AssetBundleBuild[] CreateBuildMap(Dictionary<string, BundleDependenciesData> bundleManifest = null)
        {
            if(bundleManifest == null)
            {
                bundleManifest = Manifest.GetDictionary();
            }

            List<AssetBundleBuild> bundles = new List<AssetBundleBuild>();
            foreach(var bundleData in bundleManifest.Values)
            {
                if(!string.IsNullOrEmpty(bundleData.BundleName))
                {
                    var assetBundleBuild = new AssetBundleBuild();
                    assetBundleBuild.assetBundleName = bundleData.BundleName;
                    assetBundleBuild.assetNames = new string[] { bundleData.AssetPath };
                    bundles.Add(assetBundleBuild);
                }
            }

            return bundles.ToArray();
        }
        #endregion

        #region PrivateMethods

        /// <summary>
        /// Adds or updates an asset and all its dependencies to the manifest
        /// </summary>
        /// <param name="guid">GUID of the asset to add or update</param>
        /// <param name = "isLocal"></param>
        /// <param name="isManual">If the asset is explicitly bundled by the user or not</param>
        /// <param name="parent">GUID of the asset to which GUID param is a dependency</param>
        static void AddOrUpdateAsset(string guid, bool isLocal = false, bool isManual = false, string parent = "")
        {
            BundleDependenciesData data = null;
            bool isNew = false;
            var objPath = AssetDatabase.GUIDToAssetPath(guid);

            if(string.IsNullOrEmpty(objPath))
            {
                if(Manifest.HasAsset(guid))
                {
                    data = Manifest[guid];

                    if(OnLogMessage != null)
                    {
                        OnLogMessage("Deleted file from the project, removing it from the manifest... GUID: " + guid + " Old Path: " + data.AssetPath, LogType.Warning);
                    }

                    // Remove the asset
                    RemoveAsset(guid);
                }
                else
                {
                    if(OnLogMessage != null)
                    {
                        OnLogMessage("The following file GUID: " + guid + " couldn't be added because doesn't exist in the project", LogType.Error);
                    }
                }

                if(OnBundleDeletedFromProject != null)
                {
                    OnBundleDeletedFromProject(guid, data);
                }

                return;
            }

            List<string> directDependencies = new List<string>(AssetDatabase.GetDependencies(objPath, false)).ConvertAll(x => AssetDatabase.AssetPathToGUID(x));
            var oldDependencies = new List<string>();

            if(Manifest.HasAsset(guid))
            {
                data = Manifest[guid];
                oldDependencies.AddRange(data.Dependencies.FindAll(x => !directDependencies.Contains(x)));
            }
            else
            {
                data = new BundleDependenciesData(guid);
                Manifest.Add(guid, data);
                isNew = true;
            }

            // Update the path in case it has changed
            data.AssetPath = AssetDatabase.GUIDToAssetPath(guid);
            // If it was explicit previously but we are updating dependencies we don't want to cancel manual bundle status
            data.IsExplicitlyBundled = data.IsExplicitlyBundled || isManual;
            data.Dependencies = directDependencies;

            // Removes all old dependencies references to this asset
            foreach(string oldDependency in oldDependencies)
            {
                RemoveDependant(oldDependency, guid);
            }

            // Adds the dependant in case there is not already included
            if(!string.IsNullOrEmpty(parent) && !data.Dependants.Contains(parent))
            {
                data.Dependants.Add(parent);
            }

            bool localChanged = false;
            if(!data.IsExplicitlyBundled)
            {
                HandleAutoBundling(data);

                // This can be optimized by having a cache list of already checked locals in order not to check handleislocal
                // when the isLocal is confirmed for this run (asset has two dependants one local and one remote and the remote is processed
                // after, so it has to check the rest of the dependants to see if the autobundle isLocal)
                if(data.IsLocal)
                {
                    if(!isLocal)
                    {
                        HandleIsLocal(data);
                    }
                }
                else
                {
                    localChanged = data.IsLocal != isLocal;
                    data.IsLocal = isLocal;
                }
            }
            else
            {
                if(isLocal)
                {
                    localChanged = !data.IsLocal;
                    data.IsLocal = true;
                }
                else if(data.IsLocal && isManual)
                {
                    localChanged = data.IsLocal;
                    data.IsLocal = false;
                }

                data.BundleName = GetBundleName(objPath);

                if(isNew && OnBundleAdded != null)
                {
                    OnBundleAdded(data);
                }
            }

            if(localChanged && OnBundleLocalChanged != null)
            {
                OnBundleLocalChanged(data);
            }

            foreach(string dependency in directDependencies)
            {
                AddOrUpdateAsset(dependency, data.IsLocal, false, guid);
            }
        }

        /// <summary>
        /// Removes an asset from bundled assets and cleans it if it is not needed anymore. 
        /// The asset may still be in the registry if another asset has it as a dependency and may still
        /// be a bundle depending on the AutoBundle policy
        /// </summary>
        /// <param name="GUID">GUID of the asset to remove</param>
        static void RemoveAsset(string GUID)
        {
            if(Manifest.HasAsset(GUID))
            {
                BundleDependenciesData data = Manifest[GUID];

                var path = AssetDatabase.GUIDToAssetPath(GUID);

                var wasExplicit = data.IsExplicitlyBundled;

                data.IsExplicitlyBundled = false;

                if(wasExplicit && OnBundleRemoved != null)
                {
                    OnBundleRemoved(data);
                }

                // If no assets depends on this one or this asset is no longer in the project we need to remove it completely
                if(data.Dependants.Count == 0 || string.IsNullOrEmpty(path))
                {
                    if(OnAssetRemoved != null)
                    {
                        OnAssetRemoved(data);
                    }

                    foreach(string dependency in data.Dependencies)
                    {
                        RemoveDependant(dependency, GUID);
                    }

                    Manifest.Remove(GUID);
                }
                else
                {
                    HandleAutoBundling(data);
                    HandleIsLocal(data);
                }
            }
            else
            {
                if(OnLogMessage != null)
                {
                    OnLogMessage("The asset " + GUID + " was not found in the manifest for removal.", LogType.Error);
                }
            }
        }

        /// <summary>
        /// Removes the link of a child dependency to its parent so that the child asset doesn't depend on the parent anymore
        /// </summary>
        /// <param name="GUID">GUID of the asset</param>
        /// <param name="dependantToRemove">GUID of the asset that used to have the child as a dependency</param>
        static void RemoveDependant(string GUID, string dependantToRemove)
        {
            if(Manifest.HasAsset(GUID))
            {
                var data = Manifest[GUID];
                if(data.Dependants.Contains(dependantToRemove))
                {
                    data.Dependants.Remove(dependantToRemove);

                    if(!data.IsExplicitlyBundled)
                    {
                        if(data.Dependants.Count == 0)
                        {
                            // This asset is not used anymore in any bundle and is not manually bundled
                            RemoveAsset(GUID);
                        }
                        else
                        {
                            HandleAutoBundling(data);
                            HandleIsLocal(data);
                        }
                    }
                }
            }
            else
            {
                if(OnLogMessage != null)
                {
                    OnLogMessage("Dependency not found to remove relationship " + GUID + ". Removed parent was " + dependantToRemove, LogType.Warning);
                }
            }
        }

        static List<BundleDependenciesData> GetBundledAsset()
        {
            return Manifest.GetValues().Where(x => !string.IsNullOrEmpty(x.BundleName)).ToList();
        }

        #endregion

        #region BundlesPolicies

        /// <summary>
        /// Gets the name of a user defined bundle
        /// </summary>
        /// <param name="path">Path of the asset</param>
        /// <returns>Bundle name</returns>
        static string GetBundleName(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path).Replace(" ", "_") + "_" + Path.GetExtension(path).Replace(".", "");

            return name.ToLowerInvariant();
        }

        /// <summary>
        /// Gets the name of a automatic bundle
        /// </summary>
        /// <param name="path">Path of the asset</param>
        /// <returns>Bundle name</returns>
        static string GetAutoBundleName(string path)
        {
            return GetBundleName(path);
        }

        /// <summary>
        /// Defines the bundle policy of the assets that are not manually bundled and updates the bundle status.
        /// </summary>
        /// <param name="data">Bundle data of the asset</param>
        static void HandleAutoBundling(BundleDependenciesData data)
        {
            var wasBundled = !string.IsNullOrEmpty(data.BundleName);

            // If is manual or have more than one dependant bundle it.
            if(data.Dependants.Count > 1 && string.Compare(Path.GetExtension(data.AssetPath), ".cs", StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                data.BundleName = GetAutoBundleName(data.AssetPath);

                if(!wasBundled && OnBundleAdded != null)
                {
                    OnBundleAdded(data);
                }
            }
            else
            {
                data.BundleName = string.Empty;

                if(wasBundled && OnBundleRemoved != null)
                {
                    OnBundleRemoved(data);
                }
            }
        }

        /// <summary>
        /// Determines if a bundle should be local or not depending on its parent and update the status
        /// </summary>
        /// <param name="data">Bundle data</param>
        static void HandleIsLocal(BundleDependenciesData data)
        {
            var wasLocal = data.IsLocal;
            bool isLocal = false;

            foreach(string guid in data.Dependants)
            {
                if(Manifest[guid].IsLocal)
                {
                    isLocal = true;
                    break;
                }
            }

            data.IsLocal = isLocal;

            if(wasLocal != data.IsLocal && OnBundleLocalChanged != null)
            {
                OnBundleLocalChanged(data);
            }
        }

        #endregion

        #region Log

        static void LogMessageHandler(string message, LogType severity)
        {
#if UNITY_2017_1_OR_NEWER
            Debug.unityLogger.Log(severity, message);
#else
            Debug.logger.Log(severity, message);
#endif
        }

        #endregion
    }
}
