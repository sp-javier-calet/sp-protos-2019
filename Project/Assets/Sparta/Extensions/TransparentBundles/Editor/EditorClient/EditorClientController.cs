using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using SocialPoint.Attributes;
using System.Diagnostics;
using System.Threading;

namespace SocialPoint.TransparentBundles
{
    public class EditorClientController
    {
        private static EditorClientController _instance;
        private static Downloader _downloader;
        public Dictionary<string, List<Asset>> DependenciesCache, ReferencesCache;
        public Dictionary<string, bool> SharedDependenciesCache;
        private Dictionary <string, Bundle> _bundleDictionary;
        public ServerInfo ServerInfo;

        private EditorClientController()
        {
            //Mounts the smb folder
            #if UNITY_EDITOR_OSX
            if(!Directory.Exists(Config.IconsPath))
            {
                ProcessStartInfo process = new ProcessStartInfo();
                process.WindowStyle = ProcessWindowStyle.Hidden;
                process.FileName = "mkdir";
                process.Arguments = Config.VolumePath;
                Process proc = Process.Start(process);
                if(!proc.WaitForExit(10000))
                {
                    if(EditorUtility.DisplayDialog("Transparent Bundles", "The folder '" + Config.VolumePath + "' was not able to be created. Please, contact the Transparent Bundles developers: " + Config.ContactUrl, "Close"))
                    {
                        BundlesWindow.Window.Close();
                    }
                }
                else
                {
                    process = new ProcessStartInfo();
                    process.WindowStyle = ProcessWindowStyle.Hidden;
                    process.FileName = "mount_smbfs";
                    process.Arguments = Config.SmbConnectionUrl + " " + Config.VolumePath;
                    proc = Process.Start(process);
                    if(!proc.WaitForExit(10000))
                    {
                        if(EditorUtility.DisplayDialog("Transparent Bundles", "Connection timeout. Please, make sure that you are connected to the SocialPoint network: \n wifi: 'SP_EMPLOYEE'", "Close"))
                        {
                            BundlesWindow.Window.Close();
                        }
                    }
                }
            }
            #endif

            /*string response = "";
            TransparentBundleAPI.GetBundles(new GetBundlesArgs(x => response = x.ResponseRes.Response, x => UnityEngine.Debug.LogError(x.RequestCancelled)));

            if(response.Length > 0)
            {*/
                DependenciesCache = new Dictionary<string, List<Asset>>();
                ReferencesCache = new Dictionary<string, List<Asset>>();
                SharedDependenciesCache = new Dictionary<string, bool>();

                byte[] jsonBytes = File.ReadAllBytes(Application.dataPath + "/Sparta/Extensions/TransparentBundles/Editor/TEST_json/test_json.json"); //Encoding.ASCII.GetBytes(response);

                _bundleDictionary = ReadBundleListFromJSON(jsonBytes);
                _downloader = Downloader.GetInstance();
                ServerInfo = ReadServerInfoFromJSON(jsonBytes);
            //}

        }


        private Dictionary<string, Bundle> ReadBundleListFromJSON(byte[] jsonBytes)
        {
            Dictionary<string, Bundle> bundleDictionary = new Dictionary<string, Bundle>();

            JsonAttrParser parser = new JsonAttrParser();
            Attr jsonParsed = parser.Parse(jsonBytes);
            AttrList jsonList = jsonParsed.AsList[1].AsList;
            for(int i = 0; i < jsonList.Count; i++)
            {
                AttrList jsonRow = jsonList[i].AsList;
                Asset asset = new Asset(jsonRow[4].AsValue.ToString());

                var sizesDict = new Dictionary<BundlePlaform, float>();
                AttrList jsonSizes = jsonRow[1].AsList;
                for (int j = 0; j < jsonSizes.Count; j++)
                {
                    var key = jsonSizes[j].AsDic.Keys.GetEnumerator();
                    key.MoveNext();
                    sizesDict.Add((BundlePlaform)Enum.Parse(typeof(BundlePlaform), key.Current.ToString()), jsonSizes[j].AsList[0].AsValue.ToFloat());
                }

                List<BundleOperation> operationQueue = new List<BundleOperation>();
                AttrList jsonOperations = jsonRow[8].AsList;
                for (int j = 0; j < jsonOperations.Count; j++)
                {
                    operationQueue.Add((BundleOperation)Enum.Parse(typeof(BundleOperation), jsonOperations[j].AsList[0].AsValue.ToString()));
                }

                Bundle bundle = new Bundle(jsonRow[0].AsValue.ToString(),
                                    sizesDict,
                                    jsonRow[2].AsValue.ToBool(),
                                    jsonRow[3].AsValue.ToBool(), 
                                    asset, 
                                    new List<Bundle>(),
                                    jsonRow[6].AsValue.ToString(),
                                    (BundleStatus)Enum.Parse(typeof(BundleStatus), jsonRow[7].AsValue.ToString()),
                                    operationQueue,
                                    jsonRow[9].AsValue.ToString()
                                );
                bundleDictionary.Add(asset.Name, bundle);
            }

            //Get Parent Bundles
            for (int i = 0; i < jsonList.Count; i++)
            {
                AttrList jsonRow = jsonList[i].AsList;
                string childBundleName = jsonRow[0].AsValue.ToString();
                string childAssetName = childBundleName.Substring(0, childBundleName.LastIndexOf("_"));

                AttrList jsonParents = jsonRow[5].AsList;

                for (int j = 0; j < jsonParents.Count; j++)
                {
                    string parentBundleName = jsonParents[j].AsList[0].AsValue.ToString();
                    string parentAssetName = parentBundleName.Substring(0, parentBundleName.LastIndexOf("_"));

                    if (bundleDictionary.ContainsKey(parentAssetName))
                    {
                        bundleDictionary[childAssetName].Parents.Add(bundleDictionary[parentAssetName]);
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Transparent Bundles - Error - The parent bundle '"+ parentBundleName + "' was not found in the bundle list. Please, contact the transparent bundles team: "+Config.ContactUrl);
                    }
                }
            }

            return bundleDictionary;
        }

        private ServerInfo ReadServerInfoFromJSON(byte[] jsonBytes)
        {
            JsonAttrParser parser = new JsonAttrParser();
            Attr jsonParsed = parser.Parse(jsonBytes);
            AttrList jsonList = jsonParsed.AsList[0].AsList;
            AttrList jsonRow = jsonList[0].AsList;

            return new ServerInfo((ServerStatus)Enum.Parse(typeof(ServerStatus), jsonRow[0].AsValue.ToString()), jsonRow[1].AsValue.ToString());
        }

        public static EditorClientController GetInstance()
        {
            if(_instance == null)
            {
                _instance = new EditorClientController();
            }

            return _instance;
        }

        private bool IsValidAsset(Asset asset)
        {
            bool valid = true;
            if(asset.Type.ToString() == "UnityEditor.DefaultAsset")
            {
                valid = false;
                EditorUtility.DisplayDialog("Asset issue", "You cannot create a bundle of a folder.\n\nVisit the following link for more info: \n" + Config.HelpUrl, "Close");
            }
                
            return valid;
        }

        private bool HasValidDependencies(Asset asset)
        {
            bool valid = true;
            string[] dependencesPaths = AssetDatabase.GetDependencies(AssetDatabase.GUIDToAssetPath(asset.Guid));
            for(int i = 0; i < dependencesPaths.Length && valid; i++)
            {
                string path = dependencesPaths[i];
                string simpleFileName = Path.GetFileNameWithoutExtension(path);
                string fileName = Path.GetFileName(path);
                
                string[] matchesGUIDs = AssetDatabase.FindAssets(simpleFileName);
                for(int j = 0; j < matchesGUIDs.Length && valid; j++)
                {
                    string matchPath = AssetDatabase.GUIDToAssetPath(matchesGUIDs[j]);
                    string matchFileName = Path.GetFileName(matchPath);
                    if(matchFileName == fileName && matchPath != path)
                    {
                        valid = false;
                        EditorUtility.DisplayDialog("Asset issue", "Found a duplicated asset:\n" + path + "\n" + matchPath + "\n\nYou cannot have duplicated assets in the project.\n\nVisit the following link for more info: \n" + Config.HelpUrl, "Close");
                    }
                }
            }
            return valid;
        }

        public void CreateOrUpdateBundle(Asset asset)
        {
            bool valid = true;

            if(IsValidAsset(asset))
            {
                if(!_bundleDictionary.ContainsKey(asset.Name))
                {
                    valid = HasValidDependencies(asset);
                }
                else
                {
                    Asset serverAsset = _bundleDictionary[asset.Name].Asset;
                    if(serverAsset.Guid != asset.Guid)
                    {
                        EditorUtility.DisplayDialog("Asset issue", "You are trying to create a bundle from an asset with a repeated name in the server. The system does not allow multiple bundles with the same name, so please, rename the asset and try again. \n\nVisit the following link for more info: \n" + Config.HelpUrl, "Close");
                        valid = false;
                    }
                }
            }
            else
            {
                valid = false;
            }
                

            if (valid)
            {
                TransparentBundleAPI.CreateBundle(new CreateBundlesArgs(new List<string> { asset.Guid }, x => UnityEngine.Debug.Log(x.ResponseRes.Response), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
            }

        }

        public void CreateOrUpdateBundles(List<Asset> assets)
        {
            bool valid = true;

            for (int i = 0; i < assets.Count; i++)
            {
                Asset asset = assets[i];

                if (IsValidAsset(asset))
                {
                    if (!_bundleDictionary.ContainsKey(asset.Name))
                    {
                        valid &= HasValidDependencies(asset);
                    }
                    else
                    {
                        Asset serverAsset = _bundleDictionary[asset.Name].Asset;
                        if (serverAsset.Guid != asset.Guid)
                        {
                            EditorUtility.DisplayDialog("Asset issue", "You are trying to create a bundle from an asset with a repeated name in the server. The system does not allow multiple bundles with the same name, so please, rename the asset and try again. \n\nVisit the following link for more info: \n" + Config.HelpUrl, "Close");
                            valid = false;
                        }
                    }
                }
                else
                {
                    valid = false;
                }
            }

            if (valid)
            {
                List<string> guids = new List<string>();
                for (int i = 0; i < assets.Count; i++)
                {
                    guids.Add(assets[i].Guid);
                }
                TransparentBundleAPI.CreateBundle(new CreateBundlesArgs(guids, x => UnityEngine.Debug.Log(x.ResponseRes.Response), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
            }
        }



        public void RemoveBundle(Asset asset)
        {
            if(_bundleDictionary.ContainsKey(asset.Name))
            {
                TransparentBundleAPI.RemoveBundle(new RemoveBundlesArgs(new List<string> { asset.Guid }, x => UnityEngine.Debug.Log(x.ResponseRes.Response), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
            }
        }

        public void RemoveBundles(List<Asset> assets)
        {
            bool valid = true;

            for (int i = 0; i < assets.Count; i++)
            {
                Asset asset = assets[i];
                valid &= _bundleDictionary.ContainsKey(asset.Name);
            }

            if (valid)
            {
                List<string> guids = new List<string>();
                for (int i = 0; i < assets.Count; i++)
                {
                    guids.Add(assets[i].Guid);
                }
                TransparentBundleAPI.RemoveBundle(new RemoveBundlesArgs(guids, x => UnityEngine.Debug.Log(x.ResponseRes.Response), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
            }
        }

        public void BundleIntoBuild(Asset asset)
        {
            if(_bundleDictionary.ContainsKey(asset.Name) && !_bundleDictionary[asset.Name].IsLocal)
            {
                TransparentBundleAPI.MakeLocalBundle(new MakeLocalBundlesArgs(new List<string> { asset.Guid }, x => UnityEngine.Debug.Log(x.ResponseRes.Response), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
            }
        }

        public void BundlesIntoBuild(List<Asset> assets)
        {
            bool valid = true;

            for (int i = 0; i < assets.Count; i++)
            {
                Asset asset = assets[i];
                valid &= _bundleDictionary.ContainsKey(asset.Name);
            }

            if (valid)
            {
                List<string> guids = new List<string>();
                for (int i = 0; i < assets.Count; i++)
                {
                    guids.Add(assets[i].Guid);
                }
                TransparentBundleAPI.MakeLocalBundle(new MakeLocalBundlesArgs(guids, x => UnityEngine.Debug.Log(x.ResponseRes.Response), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
            }
        }

        public void BundleOutsideBuild(Asset asset)
        {
            if(_bundleDictionary.ContainsKey(asset.Name) && _bundleDictionary[asset.Name].IsLocal)
            {
                TransparentBundleAPI.RemoveLocalBundle(new RemoveLocalBundlesArgs(new List<string> { asset.Guid }, x => UnityEngine.Debug.Log(x.ResponseRes.Response), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
            }
        }

        public void BundlesOutsideBuild(List<Asset> assets)
        {
            bool valid = true;

            for (int i = 0; i < assets.Count; i++)
            {
                Asset asset = assets[i];
                valid &= _bundleDictionary.ContainsKey(asset.Name);
            }

            if (valid)
            {
                List<string> guids = new List<string>();
                for (int i = 0; i < assets.Count; i++)
                {
                    guids.Add(assets[i].Guid);
                }
                TransparentBundleAPI.RemoveLocalBundle(new RemoveLocalBundlesArgs(guids, x => UnityEngine.Debug.Log(x.ResponseRes.Response), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
            }
        }

        public List<Bundle> GetBundles(string searchText)
        {
            List<Bundle> bundleList = new List<Bundle>();
            List<string> keys = new List<string>(_bundleDictionary.Keys);
            for(int i = 0; i < keys.Count; i++)
            {
                if(_bundleDictionary[keys[i]].Autogerated == false && keys[i].IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    bundleList.Add(_bundleDictionary[keys[i]]);
                }
            }
            return bundleList;
        }

        public Bundle GetBundleFromAsset(Asset asset)
        {
            if(_bundleDictionary.ContainsKey(asset.Name))
            {
                return _bundleDictionary[asset.Name];
            }
            else
            {
                return null;
            }

        }

        public Bundle GetBundleFromAsset(string assetName)
        {
            if(_bundleDictionary.ContainsKey(assetName))
            {
                return _bundleDictionary[assetName];
            }
            else
            {
                return null;
            }

        }

        public float GetLocalBundlesTotalSize(BundlePlaform platform)
        {
            float totalSize = 0f;
            List<string> keys = new List<string>(_bundleDictionary.Keys);
            for(int i = 0; i < keys.Count; i++)
            {
                Bundle bundle = _bundleDictionary[keys[i]];
                if(bundle.IsLocal)
                {
                    totalSize += bundle.Size[platform];
                }
            }
            return totalSize;
        }

        public float GetServerBundlesTotalSize(BundlePlaform platform)
        {
            float totalSize = 0f;
            List<string> keys = new List<string>(_bundleDictionary.Keys);
            for(int i = 0; i < keys.Count; i++)
            {
                Bundle bundle = _bundleDictionary[keys[i]];
                if(!bundle.IsLocal)
                {
                    totalSize += bundle.Size[platform];
                }
            }
            return totalSize;
        }

        public void DownloadBundle(Bundle bundle)
        {
            _downloader.DownloadBundle(bundle);
        }


        public Texture2D DownloadImage(string path)
        {
            return _downloader.DownloadImage(path);
        }

        public Asset[] GetAssetsFromSelection()
        {
            return GetAssetsFromObjects(Selection.objects);
        }

        public Asset GetAssetFromObject(UnityEngine.Object assetObject)
        {
            return GetAssetsFromObjects(new UnityEngine.Object[] { assetObject })[0];
        }

        public Asset[] GetAssetsFromObjects(UnityEngine.Object[] objects)
        {
            UnityEngine.Object[] selectedObjects = objects;

            List<Asset> assets = new List<Asset>();
            for(int i = 0; i < selectedObjects.Length; i++)
            {
                UnityEngine.Object selectedObject = selectedObjects[i];
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

        public bool IsBundle(UnityEngine.Object assetObject)
        {
            Bundle bundle = GetBundleFromAsset(assetObject.name);
            return (bundle != null && bundle.Asset.Type == assetObject.GetType().ToString());
        }

        public void SortBundles(BundleSortingMode mode, List<Bundle> bundleList, BundlePlaform platform)
        {
            switch(mode)
            {
            case BundleSortingMode.TypeDesc:
                bundleList.Sort(delegate (Bundle b1, Bundle b2) {
                    return ((b2.Asset.Type.ToString() + b1.Asset.Name).CompareTo(b1.Asset.Type.ToString() + b2.Asset.Name));
                });
                break;

            case BundleSortingMode.TypeAsc:
                bundleList.Sort(delegate (Bundle b1, Bundle b2) {
                    return ((b1.Asset.Type.ToString() + b1.Asset.Name).CompareTo(b2.Asset.Type.ToString() + b2.Asset.Name));
                });
                break;

            case BundleSortingMode.NameDesc:
                bundleList.Sort(delegate (Bundle b1, Bundle b2) {
                    return b2.Name.CompareTo(b1.Name);
                });
                break;

            case BundleSortingMode.NameAsc:
                bundleList.Sort(delegate (Bundle b1, Bundle b2) {
                    return b1.Name.CompareTo(b2.Name);
                });
                break;

            case BundleSortingMode.SizeDesc:
                bundleList.Sort(delegate (Bundle b1, Bundle b2) {
                    return (b2.Size[platform].ToString() + b1.Asset.Name).CompareTo(b1.Size[platform].ToString() + b2.Asset.Name);
                });
                break;

            case BundleSortingMode.SizeAsc:
                bundleList.Sort(delegate (Bundle b1, Bundle b2) {
                    return (b1.Size[platform].ToString() + b1.Asset.Name).CompareTo(b2.Size[platform].ToString() + b2.Asset.Name);
                });
                break;
            }
        }

        public void SortAssets(AssetSortingMode mode, List<Asset> assetList)
        {
            switch(mode)
            {
            case AssetSortingMode.TypeDesc:
                assetList.Sort(delegate (Asset b1, Asset b2) {
                    return ((b2.Type.ToString() + b1.Name).CompareTo(b1.Type.ToString() + b2.Name));
                });
                break;

            case AssetSortingMode.TypeAsc:
                assetList.Sort(delegate (Asset b1, Asset b2) {
                    return ((b1.Type.ToString() + b1.Name).CompareTo(b2.Type.ToString() + b2.Name));
                });
                break;

            case AssetSortingMode.NameDesc:
                assetList.Sort(delegate (Asset b1, Asset b2) {
                    return b2.Name.CompareTo(b1.Name);
                });
                break;

            case AssetSortingMode.NameAsc:
                assetList.Sort(delegate (Asset b1, Asset b2) {
                    return b1.Name.CompareTo(b2.Name);
                });
                break;

            }
        }

        public void FlushCache()
        {
            DependenciesCache = new Dictionary<string, List<Asset>>();
            ReferencesCache = new Dictionary<string, List<Asset>>();
            SharedDependenciesCache = new Dictionary<string, bool>();
        }
    }

    public enum BundleSortingMode
    {
        TypeAsc,
        TypeDesc,
        NameAsc,
        NameDesc,
        SizeAsc,
        SizeDesc

    }

    public enum AssetSortingMode
    {
        TypeAsc,
        TypeDesc,
        NameAsc,
        NameDesc
    }

    
}
