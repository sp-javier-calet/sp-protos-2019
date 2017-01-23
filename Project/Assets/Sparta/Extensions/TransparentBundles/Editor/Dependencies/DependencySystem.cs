using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LitJson;

namespace SocialPoint.TransparentBundles
{
    [InitializeOnLoad]
    public static class DependencySystem
    {
        public enum Severity
        {
            MESSAGE = 0,
            WARNING,
            ERROR
        }

        public static event System.Action<BundleDependenciesData> OnBundleAdded;
        public static event System.Action<string, BundleDependenciesData> OnBundleDeletedFromProject;
        public static event System.Action<BundleDependenciesData> OnBundleRemoved;
        public static event System.Action<string, Severity> LogMessage;

        private static string _path = Path.Combine(Application.dataPath, "DependenciesManifest.json");

        private static Dictionary<string, BundleDependenciesData> Manifest = new Dictionary<string, BundleDependenciesData>();

        static DependencySystem()
        {
            if(File.Exists(_path))
            {
                Load();
            }

            LogMessage += LogMessageHandler;
        }

        #region JSON
        public static void Load()
        {
            Manifest = JsonMapper.ToObject<Dictionary<string, BundleDependenciesData>>(File.ReadAllText(_path));
        }

        public static void Save()
        {
            JsonWriter writer = new JsonWriter();
            writer.PrettyPrint = true;
            JsonMapper.ToJson(Manifest, writer);
            var str = writer.ToString();
            File.WriteAllText(_path, str);
        }
        #endregion

        #region APICalls        

        public static Dictionary<string, BundleDependenciesData> GetManifest()
        {
            return Manifest;
        }

        public static List<BundleDependenciesData> GetUserBundles()
        {
            List<BundleDependenciesData> userBundles = new List<BundleDependenciesData>();
            foreach(var pair in Manifest)
            {
                if(pair.Value.IsExplicitlyBundled)
                {
                    userBundles.Add((BundleDependenciesData)pair.Value.Clone());
                }
            }

            return userBundles;
        }


        /// <summary>
        /// Gets a copy of the dependencies data stored for this asset
        /// </summary>
        /// <param name="GUID">GUID of the asset to search</param>
        /// <returns>BundleDependenciesData if the asset is in the manifest and null if it isn't</returns>
        public static BundleDependenciesData GetDependencyDataCopy(string GUID)
        {
            return Manifest.ContainsKey(GUID) ? (BundleDependenciesData)Manifest[GUID].Clone() : null;
        }

        /// <summary>
        /// Checks if the asset is registered in the manifest
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns></returns>
        public static bool HasAsset(string GUID)
        {
            return Manifest.ContainsKey(GUID);
        }

        /// <summary>
        /// Registers or refresh an asset as a manual bundled
        /// </summary>
        /// <param name="GUID">GUID of the asset to include</param>
        public static void RegisterManualBundledAsset(string GUID)
        {
            AddOrUpdateAsset(GUID, true);
            Save();
        }


        /// <summary>
        /// Updates the manifest with the guids passed by parameters. if there are guids 
        /// in the manifest that are not in the collection parameter, they'll be removed
        /// as user defined bundles.
        /// </summary>
        /// <param name="userBundles"></param>
        public static void UpdateManifest(List<string> userBundles)
        {
            var oldBundles = Manifest.Values.Where(x => x.IsExplicitlyBundled && !userBundles.Contains(x.GUID)).ToList();

            // Adds or Refreshes all the bundles.
            foreach(string guid in userBundles)
            {
                AddOrUpdateAsset(guid, true);
            }

            foreach(var bundleData in oldBundles)
            {
                if(LogMessage != null)
                {
                    LogMessage("Old asset will no longer be a user defined Bundle: " + bundleData.GUID + " Old Path: " + bundleData.AssetPath, Severity.WARNING);
                }
                // remove its condition of bundled asset and remove it if it is not a dependency
                RemoveAsset(bundleData.GUID);
            }

            Save();
        }


        /// <summary>
        /// Refreshes all the manifest and updates all the dependencies with the current project
        /// </summary>
        public static void RefreshAll()
        {
            var explicitBundles = Manifest.Where(x => x.Value.IsExplicitlyBundled);

            foreach(var pair in explicitBundles)
            {
                AddOrUpdateAsset(pair.Key);
            }

            Save();
        }


        /// <summary>
        /// Removes an asset from the manually bundled assets. 
        /// The asset may still be in the registry if another asset has it as a dependency and may still
        /// be a bundle depending on the AutoBundle policy
        /// </summary>
        /// <param name="GUID">GUID of the asset to remove</param>
        public static void RemoveBundle(string guid)
        {
            RemoveAsset(guid);
            Save();
        }

        #endregion

        #region PrivateMethods
        /// <summary>
        /// Adds or updates an asset and all its dependencies to the manifest
        /// </summary>
        /// <param name="guid">GUID of the asset to add or update</param>
        /// <param name="isManual">If the asset is explicitly bundled by the user or not</param>
        /// <param name="parent">GUID of the asset to which GUID param is a dependency</param>
        private static void AddOrUpdateAsset(string guid, bool isManual = false, string parent = "")
        {
            BundleDependenciesData data = null;
            var objPath = AssetDatabase.GUIDToAssetPath(guid);

            if(string.IsNullOrEmpty(objPath))
            {
                if(HasAsset(guid))
                {
                    data = Manifest[guid];

                    if(LogMessage != null)
                    {
                        LogMessage("Deleted file GUID: " + guid + " Old Path: " + data.AssetPath, Severity.WARNING);
                    }
                }
                else
                {
                    if(LogMessage != null)
                    {
                        LogMessage("The following file GUID: " + guid + " couldn't be added because doesn't exist", Severity.ERROR);
                    }
                }

                // Remove the asset
                RemoveAsset(guid);

                if(OnBundleDeletedFromProject != null)
                {
                    OnBundleDeletedFromProject(guid, data);
                }

                return;
            }

            List<string> directDependencies = new List<string>(AssetDatabase.GetDependencies(objPath, false)).ConvertAll(x => AssetDatabase.AssetPathToGUID(x));
            List<string> oldDependencies = new List<string>();

            if(HasAsset(guid))
            {
                data = Manifest[guid];
                oldDependencies.AddRange(data.Dependencies.FindAll(x => !directDependencies.Contains(x)));
            }
            else
            {
                data = new BundleDependenciesData(guid);
                Manifest.Add(guid, data);
            }

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

            if(!data.IsExplicitlyBundled)
            {
                HandleAutoBundling(data);
            }
            else
            {
                data.BundleName = GetBundleName(objPath);
                AssetImporter.GetAtPath(objPath).assetBundleName = data.BundleName;


                if(OnBundleAdded != null)
                {
                    OnBundleAdded(data);
                }
            }

            foreach(string dependency in directDependencies)
            {
                AddOrUpdateAsset(dependency, false, guid);
            }
        }

        /// <summary>
        /// Removes an asset from bundled assets and cleans it if it is not neede anymore. 
        /// The asset may still be in the registry if another asset has it as a dependency and may still
        /// be a bundle depending on the AutoBundle policy
        /// </summary>
        /// <param name="GUID">GUID of the asset to remove</param>
        private static void RemoveAsset(string GUID)
        {
            if(HasAsset(GUID))
            {
                BundleDependenciesData data = Manifest[GUID];

                var path = AssetDatabase.GUIDToAssetPath(GUID);

                data.IsExplicitlyBundled = false;

                // If no assets depends on this one or this asset is no longer in the project we need to remove it completely
                if(data.Dependants.Count == 0 || string.IsNullOrEmpty(path))
                {
                    foreach(string dependency in data.Dependencies)
                    {
                        RemoveDependant(dependency, GUID);
                    }

                    AssetImporter.GetAtPath(data.AssetPath).assetBundleName = string.Empty;

                    if(OnBundleRemoved != null)
                    {
                        OnBundleRemoved(data);
                    }

                    Manifest.Remove(GUID);
                }
                else
                {
                    HandleAutoBundling(data);
                }
            }
            else
            {
                if(LogMessage != null)
                {
                    LogMessage("The asset was not found in the manifest for removal.", Severity.ERROR);
                }
            }
        }


        /// <summary>
        /// Removes the link of a child dependency to its parent so that the child asset doesn't depend on the parent anymore
        /// </summary>
        /// <param name="GUID">GUID of the asset</param>
        /// <param name="dependantToRemove">GUID of the asset that used to have the child as a dependency</param>
        private static void RemoveDependant(string GUID, string dependantToRemove)
        {
            if(HasAsset(GUID))
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
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Dependency not found to update");
            }
        }

        public static void ValidateAllBundles()
        {
            var bundledAssets = GetBundledAsset();

            AssetDatabase.RemoveUnusedAssetBundleNames();
            foreach(var bundle in AssetDatabase.GetAllAssetBundleNames())
            {
                if(bundledAssets.Find(x => x.BundleName == bundle) == null)
                {
                    LogMessage("Old bundle found: " + bundle + ". Removing...", Severity.WARNING);
                    foreach(var asset in AssetDatabase.GetAssetPathsFromAssetBundle(bundle))
                    {
                        AssetImporter.GetAtPath(asset).assetBundleName = string.Empty;
                    }
                }
            }
        }


        private static List<BundleDependenciesData> GetBundledAsset()
        {
            return Manifest.Values.Where(x => !string.IsNullOrEmpty(x.BundleName)).ToList();
        }

        #endregion

        #region BundlesPolicies
        /// <summary>
        /// Gets the name of a user defined bundle
        /// </summary>
        /// <param name="path">Path of the asset</param>
        /// <returns>Bundle name</returns>
        private static string GetBundleName(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path).Replace(" ", "_") + "_" + Path.GetExtension(path).Replace(".", "");

            return name.ToLowerInvariant();
        }

        /// <summary>
        /// Gets the name of a automatic bundle
        /// </summary>
        /// <param name="path">Path of the asset</param>
        /// <returns>Bundle name</returns>
        private static string GetAutoBundleName(string path)
        {
            return GetBundleName(path);
        }

        /// <summary>
        /// Defines the bundle policy of the assets that are not manually bundled
        /// </summary>
        /// <param name="data">Registry info of the asset</param>
        private static void HandleAutoBundling(BundleDependenciesData data)
        {
            var wasBundled = !string.IsNullOrEmpty(data.BundleName);

            // If is manual or have more than one dependant bundle it.
            if(data.Dependants.Count > 1)
            {
                if(!wasBundled && OnBundleAdded != null)
                {
                    OnBundleAdded(data);
                }
                data.BundleName = GetAutoBundleName(data.AssetPath);
            }
            else
            {
                if(wasBundled && OnBundleRemoved != null)
                {
                    OnBundleRemoved(data);
                }
                data.BundleName = "";
            }

            AssetImporter.GetAtPath(data.AssetPath).assetBundleName = data.BundleName;
        }
        #endregion

        #region Log
        private static void LogMessageHandler(string message, Severity severity)
        {
            switch(severity)
            {
                case Severity.ERROR:
                    Debug.LogError(message);
                    break;

                case Severity.MESSAGE:
                    Debug.Log(message);
                    break;

                case Severity.WARNING:
                    Debug.LogWarning(message);
                    break;
            }
        }

        #endregion

    }
}
