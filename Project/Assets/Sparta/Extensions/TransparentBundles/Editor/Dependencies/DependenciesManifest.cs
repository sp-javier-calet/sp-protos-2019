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
        private static string path = Path.Combine(Application.dataPath, "DependenciesManifest.json");

        public static Dictionary<string, BundleDependenciesData> manifest = new Dictionary<string, BundleDependenciesData>();

        static DependenciesManifest()
        {
            if(File.Exists(path))
            {
                Load();
            }
        }

        public static void Load()
        {
            manifest = JsonMapper.ToObject<Dictionary<string, BundleDependenciesData>>(File.ReadAllText(path));
        }

        public static void Save()
        {
            JsonWriter writer = new JsonWriter();
            writer.PrettyPrint = true;
            JsonMapper.ToJson(manifest, writer);
            var str = writer.ToString();
            File.WriteAllText(path, str);
        }

        public static BundleDependenciesData GetDependencyData(string GUID)
        {
            return manifest[GUID];
        }

        public static bool HasAsset(string GUID)
        {
            return manifest.ContainsKey(GUID);
        }

        #region FullAssets

        public static void AddAssetsWithDependencies(string GUID, string bundleName)
        {
            var objPath = AssetDatabase.GUIDToAssetPath(GUID);
            List<string> dependendantAsset = GetListOfDependencies(objPath);
            List<string> dependenciesToRemove = new List<string>();

            if(HasAsset(GUID))
            {
                dependenciesToRemove.AddRange(manifest[GUID].dependencies);
            }

            AssignBundle(AssetDatabase.AssetPathToGUID(objPath), bundleName);
            foreach(string assetPath in dependendantAsset)
            {
                AddAssetDependency(AssetDatabase.AssetPathToGUID(assetPath), AssetDatabase.AssetPathToGUID(objPath), dependenciesToRemove);
            }
        }


        public static void RemoveAssetWithDependencies(string GUID)
        {
            if(HasAsset(GUID))
            {
                var objPath = AssetDatabase.GUIDToAssetPath(GUID);
                List<string> dependenciesToInclude = GetListOfDependencies(objPath);

                var dependenciesPropagation = manifest[GUID].dependencies;

                RemoveSingleAsset(AssetDatabase.AssetPathToGUID(objPath));
                foreach(string assetPath in dependenciesToInclude)
                {
                    RemoveDependency(AssetDatabase.AssetPathToGUID(assetPath), GUID, dependenciesPropagation);
                }
            }
        }

        #endregion

        #region SingleAssets

        public static void AssignBundle(string GUID, string bundleName)
        {
            BundleDependenciesData data = null;
            List<string> listDependencies = null;

            if(HasAsset(GUID))
            {
                data = manifest[GUID];
                listDependencies = data.dependencies;
            }
            else
            {
                data = new BundleDependenciesData();
                listDependencies = new List<string>();
            }

            if(data.IsExplicitlyBundled)
            {
                RemoveAssetWithDependencies(GUID);
            }

            data.assetPath = AssetDatabase.GUIDToAssetPath(GUID);
            data.bundleName = bundleName;


            if(listDependencies.Count == 0 || listDependencies.RemoveAll(x => x == bundleName) != 0)
            {
                data.dependencies = listDependencies;
            }

            if(HasAsset(GUID))
            {
                manifest[GUID] = data;
            }
            else
            {
                manifest.Add(GUID, data);
            }

        }

        public static void RemoveSingleAsset(string GUID)
        {
            if(HasAsset(GUID))
            {
                if(manifest[GUID].dependencies.Count == 0)
                {
                    manifest.Remove(GUID);
                }
                else
                {
                    manifest[GUID].bundleName = string.Empty;
                }
            }
        }

        public static List<string> GetBundleIDs(string GUID)
        {
            List<string> bundles = new List<string>();

            if(manifest[GUID].IsExplicitlyBundled)
            {
                bundles.Add(manifest[GUID].bundleName);
            }
            else
            {
                foreach(string dep in manifest[GUID].dependencies)
                {
                    bundles.AddRange(GetBundleIDs(dep));
                }
            }

            return bundles;
        }


        public static Dictionary<string, BundleDependenciesData> GetAllParentsBundled(string guid)
        {
            Dictionary<string, BundleDependenciesData> parents = new Dictionary<string, BundleDependenciesData>();

            if(manifest[guid].IsExplicitlyBundled)
            {
                parents.Add(guid, manifest[guid]);
            }
            else
            {
                foreach(string dep in manifest[guid].dependencies)
                {
                    parents.Add(dep, manifest[dep]);
                }
            }

            return parents;
        }
        #endregion

        #region Dependencies
        public static List<string> GetListOfDependencies(string objPath)
        {
            List<string> directDependencies = new List<string>(AssetDatabase.GetDependencies(objPath, false));
            List<string> subdependencies = new List<string>();


            foreach(string str in directDependencies)
            {
                var dependencyGUID = AssetDatabase.AssetPathToGUID(str);
                if(!HasAsset(dependencyGUID) || !manifest[dependencyGUID].IsExplicitlyBundled)
                {
                    subdependencies = GetListOfDependencies(str);
                }
            }

            directDependencies.AddRange(subdependencies);
            return directDependencies;
        }


        public static void AddAssetDependency(string GUID, string dependency, List<string> dependenciesToRemove = null)
        {
            BundleDependenciesData data = null;
            List<string> listDependencies = null;

            if(HasAsset(GUID))
            {
                data = manifest[GUID];
                listDependencies = data.dependencies;
            }
            else
            {
                data = new BundleDependenciesData();
                listDependencies = new List<string>();
            }


            listDependencies.Add(dependency);

            if(dependenciesToRemove != null)
            {
                listDependencies.RemoveAll(x => dependenciesToRemove.Contains(x));
            }

            data.assetPath = AssetDatabase.GUIDToAssetPath(GUID);
            data.dependencies = listDependencies;


            if(HasAsset(GUID))
            {
                manifest[GUID] = data;
            }
            else
            {
                manifest.Add(GUID, data);
            }
        }


        public static void RemoveDependency(string GUID, string dependency, List<string> dependenciesToAdd = null)
        {
            if(HasAsset(GUID))
            {
                manifest[GUID].dependencies.Remove(dependency);

                if(dependenciesToAdd != null)
                {
                    manifest[GUID].dependencies.AddRange(dependenciesToAdd);
                }

                if(!manifest[GUID].IsExplicitlyBundled && manifest[GUID].dependencies.Count == 0)
                {
                    manifest.Remove(GUID);
                }
            }
        }
        #endregion

        #region Parenthood


        public static List<string> GetBundleChildren(string bundleName)
        {
            List<string> childBundles = new List<string>();

            foreach(KeyValuePair<string, BundleDependenciesData> pair in manifest)
            {
                if(pair.Value.bundleName == bundleName)
                {
                    childBundles.AddRange(GetChildsRecursive(pair.Value));
                }
            }

            return childBundles;
        }

        static List<string> GetChildsRecursive(BundleDependenciesData dependencyData)
        {
            List<string> childs = new List<string>();

            foreach(string str in dependencyData.dependencies)
            {
                if(!manifest[str].IsExplicitlyBundled)
                {
                    childs.AddRange(GetChildsRecursive(manifest[str]));
                }
                else
                {
                    childs.Add(manifest[str].bundleName);
                }
            }

            return childs;
        }

        public static string GetBundleParent(string bundleName)
        {
            string parentBundle = "";

            foreach(KeyValuePair<string, BundleDependenciesData> pair in manifest)
            {
                if(pair.Value.bundleName == bundleName)
                {
                    var dependencies = GetListOfDependencies(AssetDatabase.GUIDToAssetPath(pair.Key));

                    foreach(string str in dependencies)
                    {
                        string GUID = AssetDatabase.AssetPathToGUID(str);

                        if(HasAsset(GUID) && manifest[GUID].IsExplicitlyBundled)
                        {
                            parentBundle = manifest[GUID].bundleName;
                        }
                    }

                    if(!string.IsNullOrEmpty(parentBundle))
                    {
                        break;
                    }
                }
            }

            return parentBundle;
        }
        #endregion
    }
}
