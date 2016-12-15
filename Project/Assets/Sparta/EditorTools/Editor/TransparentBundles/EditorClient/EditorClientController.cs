using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace SocialPoint.TransparentBundles
{
    public class EditorClientController
    {
        private static EditorClientController _instance;
        private static Downloader _downloader;
        public Dictionary<string, List<Asset>> DependenciesCache, ReferencesCache;
        public Dictionary<string, bool> SharedDependenciesCache;

        /*FOR TESTING ONLY*/
        private Dictionary <string, Bundle> _bundleDictionary;

        /*FOR TESTING ONLY*/
        private EditorClientController()
        {
            DependenciesCache = new Dictionary<string, List<Asset>>();
            ReferencesCache = new Dictionary<string, List<Asset>>();
            SharedDependenciesCache = new Dictionary<string, bool>();
            _bundleDictionary = new Dictionary<string, Bundle>();
            _downloader = Downloader.GetInstance();
        }

        public static EditorClientController GetInstance()
        {
            if (_instance == null)
                _instance = new EditorClientController();

            return _instance;
        }

        private bool IsValidAsset(Asset asset)
        {
            bool valid = true;
            if (asset.Type.ToString() == "UnityEditor.DefaultAsset")
            {
                valid = false;
                EditorUtility.DisplayDialog("Asset issue","You cannot create a bundle of a folder.\n\nVisit the following link for more info: \n"+Config.HelpUrl, "Close");
            }
                
            return valid;
        }

        private bool HasValidDependencies(Asset asset)
        {
            bool valid = true;
            string[] dependencesPaths = AssetDatabase.GetDependencies(AssetDatabase.GUIDToAssetPath(asset.Guid));
            for (int i = 0; i < dependencesPaths.Length && valid; i++)
            {
                string path = dependencesPaths[i];
                string simpleFileName = Path.GetFileNameWithoutExtension(path);
                string fileName = Path.GetFileName(path);
                
                string[] matchesGUIDs = AssetDatabase.FindAssets(simpleFileName);
                for(int j=0; j < matchesGUIDs.Length && valid; j++)
                {
                    string matchPath = AssetDatabase.GUIDToAssetPath(matchesGUIDs[j]);
                    string matchFileName = Path.GetFileName(matchPath);
                    if (matchFileName == fileName && matchPath != path)
                    {
                        valid = false;
                        EditorUtility.DisplayDialog("Asset issue", "Found a duplicated asset:\n" + path + "\n"+ matchPath + "\n\nYou cannot have duplicated assets in the project.\n\nVisit the following link for more info: \n" + Config.HelpUrl, "Close");
                    }
                }
            }
            return valid;
        }

        public void CreateOrUpdateBundle(Asset asset)
        {
            if (IsValidAsset(asset))
            {
                //TODO comunication with server

                /*FOR TESTING ONLY*/

                if (!_bundleDictionary.ContainsKey(asset.Name))
                {
                    if (!HasValidDependencies(asset))
                        return;
                    Bundle bundle = new Bundle(1, asset.Name.ToLower(), 1, 2f, false, asset);
                    _bundleDictionary.Add(asset.Name, bundle);
                }
                else
                {
                    Asset serverAsset = _bundleDictionary[asset.Name].Asset;
                    if(serverAsset.Guid != asset.Guid)
                    {
                        EditorUtility.DisplayDialog("Asset issue", "You are trying to create a bundle from an asset with a repeated name in the server. The system does not allow multiple bundles with the same name, so please, rename the asset and try again. \n\nVisit the following link for more info: \n" + Config.HelpUrl, "Close");
                    }
                    else
                    {
                        //UPDATE
                    }
                }
            }
        }

        public void RemoveBundle(Asset asset)
        {
            //TODO comunication with server

            /*FOR TESTING ONLY*/
            if (_bundleDictionary.ContainsKey(asset.Name))
                _bundleDictionary.Remove(asset.Name);
        }

        public void BundleIntoBuild(Asset asset)
        {
            //TODO comunication with server

            /*FOR TESTING ONLY*/
            if (_bundleDictionary.ContainsKey(asset.Name) && !_bundleDictionary[asset.Name].IsLocal)
                _bundleDictionary[asset.Name].IsLocal = true;
        }

        public void BundleOutsideBuild(Asset asset)
        {
            //TODO comunication with server

            /*FOR TESTING ONLY*/
            if (_bundleDictionary.ContainsKey(asset.Name) && _bundleDictionary[asset.Name].IsLocal)
                _bundleDictionary[asset.Name].IsLocal = false;
        }

        public List<Bundle> GetBundles(string searchText)
        {
            //TODO comunication with server

            /*FOR TESTING ONLY*/
            List<Bundle> bundleList = new List<Bundle>();
            List<string> keys = new List<string>(_bundleDictionary.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].Contains(searchText))
                    bundleList.Add(_bundleDictionary[keys[i]]);
            }
            return bundleList;
        }

        public Bundle GetBundleFromAsset(Asset asset)
        {
            //TODO comunication with server

            /*FOR TESTING ONLY*/
            if (_bundleDictionary.ContainsKey(asset.Name))
                return _bundleDictionary[asset.Name];
            else
                return null;

        }

        public Bundle GetBundleFromAsset(string assetName)
        {
            //TODO comunication with server

            /*FOR TESTING ONLY*/
            if (_bundleDictionary.ContainsKey(assetName))
                return _bundleDictionary[assetName];
            else
                return null;

        }

        public float GetLocalBundlesTotalSize()
        {
            //TODO comunication with server

            /*FOR TESTING ONLY*/
            float totalSize = 0f;
            List<string> keys = new List<string>(_bundleDictionary.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                Bundle bundle = _bundleDictionary[keys[i]];
                if (bundle.IsLocal)
                    totalSize += bundle.Size;
            }
            return totalSize;
        }

        public float GetServerBundlesTotalSize()
        {
            //TODO comunication with server

            /*FOR TESTING ONLY*/
            float totalSize = 0f;
            List<string> keys = new List<string>(_bundleDictionary.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                Bundle bundle = _bundleDictionary[keys[i]];
                if (!bundle.IsLocal)
                    totalSize += bundle.Size;
            }
            return totalSize;
        }

        public void InstanciateBundle(Bundle bundle)
        {
            //TODO
            /*FOR TESTING ONLY*/
            UnityEngine.Debug.Log("DOWNLOADED " + bundle);
        }

        public Texture2D DownloadImage(string path)
        {
            return _downloader.DownloadImage(path);
        }

        public Asset[] GetAssetsFromSelection()
        {
            return GetAssetsFromObjects(Selection.objects);
        }

        public Asset GetAssetFromObject(Object assetObject)
        {
            return GetAssetsFromObjects(new Object[] { assetObject })[0];
        }

        public Asset[] GetAssetsFromObjects(Object[] objects)
        {
            Object[] selectedObjects = objects;

            List<Asset> assets = new List<Asset>();
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                Object selectedObject = selectedObjects[i];
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetOrScenePath(selectedObject));
                Asset asset = new Asset(guid, selectedObject.name);
                assets.Add(asset);
            }

            return assets.ToArray();
        }

        public bool IsBundle(string assetName)
        {
            return GetBundleFromAsset(assetName) != null;
        }

        public bool IsBundle(Object assetObject)
        {
            Bundle bundle = GetBundleFromAsset(assetObject.name);
            return (bundle != null && bundle.Asset.Type == assetObject.GetType().ToString());
        }

        

        public void SortBundles(BundleSortingMode mode, List<Bundle> bundleList)
        {
            switch (mode)
            {
                case BundleSortingMode.TypeDesc:
                    bundleList.Sort(delegate (Bundle b1, Bundle b2) { return ((b2.Asset.Type.ToString() + b1.Asset.Name).CompareTo(b1.Asset.Type.ToString() + b2.Asset.Name)); });
                    break;

                case BundleSortingMode.TypeAsc:
                    bundleList.Sort(delegate (Bundle b1, Bundle b2) { return ((b1.Asset.Type.ToString() + b1.Asset.Name).CompareTo(b2.Asset.Type.ToString() + b2.Asset.Name)); });
                    break;

                case BundleSortingMode.NameDesc:
                    bundleList.Sort(delegate (Bundle b1, Bundle b2) { return b2.Name.CompareTo(b1.Name); });
                    break;

                case BundleSortingMode.NameAsc:
                    bundleList.Sort(delegate (Bundle b1, Bundle b2) { return b1.Name.CompareTo(b2.Name); });
                    break;

                case BundleSortingMode.SizeDesc:
                    bundleList.Sort(delegate (Bundle b1, Bundle b2) { return (b2.Size.ToString() + b1.Asset.Name).CompareTo(b1.Size.ToString() + b2.Asset.Name); });
                    break;

                case BundleSortingMode.SizeAsc:
                    bundleList.Sort(delegate (Bundle b1, Bundle b2) { return (b1.Size.ToString() + b1.Asset.Name).CompareTo(b2.Size.ToString() + b2.Asset.Name); });
                    break;
            }
        }

        public void SortAssets(AssetSortingMode mode, List<Asset> assetList)
        {
            switch (mode)
            {
                case AssetSortingMode.TypeDesc:
                    assetList.Sort(delegate (Asset b1, Asset b2) { return ((b2.Type.ToString() + b1.Name).CompareTo(b1.Type.ToString() + b2.Name)); });
                    break;

                case AssetSortingMode.TypeAsc:
                    assetList.Sort(delegate (Asset b1, Asset b2) { return ((b1.Type.ToString() + b1.Name).CompareTo(b2.Type.ToString() + b2.Name)); });
                    break;

                case AssetSortingMode.NameDesc:
                    assetList.Sort(delegate (Asset b1, Asset b2) { return b2.Name.CompareTo(b1.Name); });
                    break;

                case AssetSortingMode.NameAsc:
                    assetList.Sort(delegate (Asset b1, Asset b2) { return b1.Name.CompareTo(b2.Name); });
                    break;

            }
        }
    }

    public enum BundleSortingMode { TypeAsc, TypeDesc, NameAsc, NameDesc, SizeAsc, SizeDesc }
    public enum AssetSortingMode { TypeAsc, TypeDesc, NameAsc, NameDesc }
}