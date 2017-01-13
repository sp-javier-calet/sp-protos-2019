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

        public static BundleDependenciesData GetDependencyDataCopy(string GUID)
        {
            return (BundleDependenciesData)Manifest[GUID].Clone();
        }

        public static bool HasAsset(string GUID)
        {
            return Manifest.ContainsKey(GUID);
        }

        #region FullAssets

        public static void AddAssetsWithDependencies(string GUID)
        {
            AddOrUpdateAsset(GUID, true);
        }

        public static void RefreshAll()
        {
            var explicitBundles = Manifest.Where(x => x.Value.IsExplicitlyBundled);

            foreach(var pair in explicitBundles)
            {
                var path = AssetDatabase.GUIDToAssetPath(pair.Key);

                if(string.IsNullOrEmpty(path))
                {
                    Debug.LogError("Deleted file GUID: " + pair.Key + " Old Path: " + pair.Value.AssetPath);
                    // We should delete it from the json
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
            }

            var importer = AssetImporter.GetAtPath(objPath);
            importer.assetBundleName = data.BundleName;

            foreach(string dependency in directDependencies)
            {
                AddOrUpdateAsset(dependency, false, GUID);
            }
        }

        public static void RemoveAsset(string GUID)
        {
            BundleDependenciesData data = Manifest[GUID];

            data.IsExplicitlyBundled = false;

            if(data.Dependants.Count == 0)
            {
                foreach(string dependency in data.Dependencies)
                {
                    RemoveDependant(dependency, GUID);
                }

                Manifest.Remove(GUID);
            }
            else
            {
                HandleAutoBundling(data);
            }
        }

        private static void RemoveDependant(string GUID, string dependantToRemove)
        {
            if(HasAsset(GUID))
            {
                var data = Manifest[GUID];
                if(data.Dependants.Contains(dependantToRemove))
                {
                    data.Dependants.Remove(dependantToRemove);

                    if(data.Dependants.Count == 0 && !data.IsExplicitlyBundled)
                    {
                        // this is an old unused dependency
                        RemoveAsset(GUID);
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
        private static string GetBundleName(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path) + "_" + Path.GetExtension(path);

            return name.ToLowerInvariant();
        }

        private static string GetAutoBundleName(string path)
        {
            return GetBundleName(path);
        }


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
        }
        #endregion

    }
}
