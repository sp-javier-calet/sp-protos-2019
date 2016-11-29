using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SocialPoint.TransparentBundles
{
    public class EditorClientController
    {
        private static EditorClientController _instance;
        private static Downloader _downloader;

        /*FOR TESTING ONLY*/ private Dictionary <string, Bundle> _bundleDictionary;

        /*FOR TESTING ONLY*/
        private EditorClientController()
        {
            _bundleDictionary = new Dictionary<string, Bundle>();
            _downloader = Downloader.GetInstance();
        }

        public static EditorClientController GetInstance()
        {
            if (_instance == null)
                _instance = new EditorClientController();

            return _instance;
        }


        public void CreateOrUpdateBundle(Asset asset)
        {
            //TODO comunication with server

            /*FOR TESTING ONLY*/
            if (!_bundleDictionary.ContainsKey(asset.Name))
            {
                Bundle bundle = new Bundle(1, asset.Name.ToLower(), 1, 2f, false, asset);
                _bundleDictionary.Add(asset.Name, bundle);
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
            if (_bundleDictionary.ContainsKey(asset.Name))
                _bundleDictionary[asset.Name].IsLocal = true;
        }

        public void BundleOutsideBuild(Asset asset)
        {
            //TODO comunication with server

            /*FOR TESTING ONLY*/
            if (_bundleDictionary.ContainsKey(asset.Name))
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