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
    public static class DependenciesManifest
    {
        private static string _path = Path.Combine(Application.dataPath, "DependenciesManifest.json");

        public static Dictionary<string, BundleDependenciesData> Manifest = new Dictionary<string, BundleDependenciesData>();

        static DependenciesManifest()
        {
            if(File.Exists(_path))
            {
                Load();
            }
        }

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

        #region FullAssets

        /// <summary>
        /// Registers or refresh an asset as a manual bundled
        /// </summary>
        /// <param name="GUID">GUID of the asset to include</param>
        public static void RegisterManualBundledAsset(string GUID)
        {
            AddOrUpdateAsset(GUID, true);
        }


        /// <summary>
        /// Updates the manifest with the guids passed by parameters. if there are guids 
        /// in the manifest that are not in the collection parameter, they'll be removed
        /// as user defined bundles.
        /// </summary>
        /// <param name="userBundles"></param>
        public static void UpdateManifest(List<string> userBundles)
        {
            var oldBundles = Manifest.Where(x => x.Value.IsExplicitlyBundled && !userBundles.Contains(x.Key));

            // Adds or Refreshes all the bundles.
            foreach(string guid in userBundles)
            {
                RegisterManualBundledAsset(guid);
            }

            foreach(var pair in oldBundles)
            {
                var path = AssetDatabase.GUIDToAssetPath(pair.Key);
                Debug.LogError("Old asset will no longer be a user defined Bundle: " + pair.Key + " Old Path: " + pair.Value.AssetPath);
                // remove its condition of bundled asset and remove it if it is not a dependency
                RemoveAsset(pair.Key);
            }
        }


        /// <summary>
        /// Refreshes all the manifest and updates all the dependencies with the current project
        /// </summary>
        public static void RefreshAll()
        {
            var explicitBundles = Manifest.Where(x => x.Value.IsExplicitlyBundled);

            foreach(var pair in explicitBundles)
            {
                var path = AssetDatabase.GUIDToAssetPath(pair.Key);

                if(string.IsNullOrEmpty(path))
                {
                    Debug.LogError("Deleted file GUID: " + pair.Key + " Old Path: " + pair.Value.AssetPath);
                    // remove its condition of bundled asset and remove it if it is not a dependency
                    RemoveAsset(pair.Key);
                }
                else
                {
                    AddOrUpdateAsset(pair.Key);
                }
            }
        }
        #endregion

        #region SingleAssets
        /// <summary>
        /// Adds or updates an asset and all its dependencies to the manifest
        /// </summary>
        /// <param name="GUID">GUID of the asset to add or update</param>
        /// <param name="isManual">If the asset is explicitly bundled by the user or not</param>
        /// <param name="parent">GUID of the asset to which GUID param is a dependency</param>
        private static void AddOrUpdateAsset(string GUID, bool isManual = false, string parent = "")
        {
            BundleDependenciesData data = null;
            var objPath = AssetDatabase.GUIDToAssetPath(GUID);
            List<string> directDependencies = new List<string>(AssetDatabase.GetDependencies(objPath, false)).ConvertAll(x => AssetDatabase.AssetPathToGUID(x));
            List<string> oldDependencies = new List<string>();

            if(HasAsset(GUID))
            {
                data = Manifest[GUID];
                oldDependencies.AddRange(data.Dependencies.FindAll(x => !directDependencies.Contains(x)));
            }
            else
            {
                Manifest.Add(GUID, new BundleDependenciesData());
                data = Manifest[GUID];
            }

            // If it was explicit previously but we are updating dependencies we don't want to cancel manual bundle status
            data.IsExplicitlyBundled = data.IsExplicitlyBundled || isManual;
            data.AssetPath = objPath;
            data.Dependencies = directDependencies;

            // Removes all old dependencies references to this asset
            foreach(string oldDependency in oldDependencies)
            {
                RemoveDependant(oldDependency, GUID);
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
                var importer = AssetImporter.GetAtPath(objPath);
                importer.assetBundleName = data.BundleName;
            }

            foreach(string dependency in directDependencies)
            {
                AddOrUpdateAsset(dependency, false, GUID);
            }
        }

        /// <summary>
        /// Removes an asset from the manually bundled assets. 
        /// The asset may still be in the registry if another asset has it as a dependency and may still
        /// be a bundle depending on the AutoBundle policy
        /// </summary>
        /// <param name="GUID">GUID of the asset to remove</param>
        public static void RemoveAsset(string GUID)
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

                    var importer = AssetImporter.GetAtPath(data.AssetPath);
                    importer.assetBundleName = "";

                    Manifest.Remove(GUID);
                }
                else
                {
                    HandleAutoBundling(data);
                }
            }
            else
            {
                Debug.LogError("The asset was not found in the manifest.");
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
        #endregion

        #region Bundles
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
            // If is manual or have more than one dependant bundle it.
            if(data.Dependants.Count > 1)
            {
                data.BundleName = GetAutoBundleName(data.AssetPath);
            }
            else
            {
                data.BundleName = "";
            }

            var importer = AssetImporter.GetAtPath(data.AssetPath);
            importer.assetBundleName = data.BundleName;
        }
        #endregion

    }
}
