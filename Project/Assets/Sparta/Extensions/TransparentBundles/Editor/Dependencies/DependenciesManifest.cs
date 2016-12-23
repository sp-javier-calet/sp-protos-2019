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

        public static BundleDependenciesData GetDependencyData(string GUID)
        {
            return Manifest[GUID];
        }

        public static bool HasAsset(string GUID)
        {
            return Manifest.ContainsKey(GUID);
        }

        #region FullAssets

        public static void AddAssetsWithDependencies(string GUID, string bundleName)
        {
            var objPath = AssetDatabase.GUIDToAssetPath(GUID);
            List<string> dependendantAsset = GetListOfDependencies(objPath);
            List<string> dependenciesToRemove = new List<string>();

            if(HasAsset(GUID))
            {
                dependenciesToRemove.AddRange(Manifest[GUID].Dependencies);
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

                var dependenciesPropagation = Manifest[GUID].Dependencies;

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
                data = Manifest[GUID];
                listDependencies = data.Dependencies;
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

            data.AssetPath = AssetDatabase.GUIDToAssetPath(GUID);
            data.BundleName = bundleName;


            if(listDependencies.Count == 0 || listDependencies.RemoveAll(x => x == bundleName) != 0)
            {
                data.Dependencies = listDependencies;
            }

            if(HasAsset(GUID))
            {
                Manifest[GUID] = data;
            }
            else
            {
                Manifest.Add(GUID, data);
            }

        }

        public static void RemoveSingleAsset(string GUID)
        {
            if(HasAsset(GUID))
            {
                if(Manifest[GUID].Dependencies.Count == 0)
                {
                    Manifest.Remove(GUID);
                }
                else
                {
                    Manifest[GUID].BundleName = string.Empty;
                }
            }
        }

        public static List<string> GetBundleIDs(string GUID)
        {
            List<string> bundles = new List<string>();

            if(Manifest[GUID].IsExplicitlyBundled)
            {
                bundles.Add(Manifest[GUID].BundleName);
            }
            else
            {
                foreach(string dep in Manifest[GUID].Dependencies)
                {
                    bundles.AddRange(GetBundleIDs(dep));
                }
            }

            return bundles;
        }


        public static Dictionary<string, BundleDependenciesData> GetAllParentsBundled(string guid)
        {
            Dictionary<string, BundleDependenciesData> parents = new Dictionary<string, BundleDependenciesData>();

            if(Manifest[guid].IsExplicitlyBundled)
            {
                parents.Add(guid, Manifest[guid]);
            }
            else
            {
                foreach(string dep in Manifest[guid].Dependencies)
                {
                    parents.Add(dep, Manifest[dep]);
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
                if(!HasAsset(dependencyGUID) || !Manifest[dependencyGUID].IsExplicitlyBundled)
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
                data = Manifest[GUID];
                listDependencies = data.Dependencies;
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

            data.AssetPath = AssetDatabase.GUIDToAssetPath(GUID);
            data.Dependencies = listDependencies;


            if(HasAsset(GUID))
            {
                Manifest[GUID] = data;
            }
            else
            {
                Manifest.Add(GUID, data);
            }
        }


        public static void RemoveDependency(string GUID, string dependency, List<string> dependenciesToAdd = null)
        {
            if(HasAsset(GUID))
            {
                Manifest[GUID].Dependencies.Remove(dependency);

                if(dependenciesToAdd != null)
                {
                    Manifest[GUID].Dependencies.AddRange(dependenciesToAdd);
                }

                if(!Manifest[GUID].IsExplicitlyBundled && Manifest[GUID].Dependencies.Count == 0)
                {
                    Manifest.Remove(GUID);
                }
            }
        }
        #endregion

        #region Parenthood


        public static List<string> GetBundleChildren(string bundleName)
        {
            List<string> childBundles = new List<string>();

            foreach(KeyValuePair<string, BundleDependenciesData> pair in Manifest)
            {
                if(pair.Value.BundleName == bundleName)
                {
                    childBundles.AddRange(GetChildsRecursive(pair.Value));
                }
            }

            return childBundles;
        }

        static List<string> GetChildsRecursive(BundleDependenciesData dependencyData)
        {
            List<string> childs = new List<string>();

            foreach(string str in dependencyData.Dependencies)
            {
                if(!Manifest[str].IsExplicitlyBundled)
                {
                    childs.AddRange(GetChildsRecursive(Manifest[str]));
                }
                else
                {
                    childs.Add(Manifest[str].BundleName);
                }
            }

            return childs;
        }

        public static string GetBundleParent(string bundleName)
        {
            string parentBundle = "";

            foreach(KeyValuePair<string, BundleDependenciesData> pair in Manifest)
            {
                if(pair.Value.BundleName == bundleName)
                {
                    var dependencies = GetListOfDependencies(AssetDatabase.GUIDToAssetPath(pair.Key));

                    foreach(string str in dependencies)
                    {
                        string GUID = AssetDatabase.AssetPathToGUID(str);

                        if(HasAsset(GUID) && Manifest[GUID].IsExplicitlyBundled)
                        {
                            parentBundle = Manifest[GUID].BundleName;
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
