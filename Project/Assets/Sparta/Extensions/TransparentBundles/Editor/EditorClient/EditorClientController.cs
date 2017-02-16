using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using SocialPoint.Attributes;
using System.Diagnostics;
using System.Threading;
using System.Text;

namespace SocialPoint.TransparentBundles
{
    public class EditorClientController
    {
        private static EditorClientController _instance;
        private static Downloader _downloader;
        public Dictionary<string, List<Asset>> DependenciesCache, ReferencesCache;
        public Dictionary<string, bool> SharedDependenciesCache;
        private Dictionary<string, Bundle> _bundleDictionary;
        public ServerInfo ServerInfo;

        //TEMPORARY
        public Dictionary<string, Bundle> NewBundles;

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
            LoadBundleDataFromServer();

            DependenciesCache = new Dictionary<string, List<Asset>>();
            ReferencesCache = new Dictionary<string, List<Asset>>();
            SharedDependenciesCache = new Dictionary<string, bool>();
            _downloader = Downloader.GetInstance();
            _bundleDictionary = new Dictionary<string, Bundle>();
            ServerInfo = new ServerInfo(ServerStatus.Ok, "", new Dictionary<int, BundleOperation>());
            NewBundles = new Dictionary<string, Bundle>();

        }

        public void LoadBundleDataFromServer(Action SuccessCallback = null)
        {
            TransparentBundleAPI.GetBundles(new GetBundlesArgs(x => ImportBundleData(x.ResponseRes.Response, SuccessCallback), x => UnityEngine.Debug.LogError(x.ResponseRes.Response)));
        }

        private void ImportBundleData(string bundleJsonString, Action SuccessCallback = null )
        {
            if (bundleJsonString.Length > 0)
            {
                byte[] jsonBytes = Encoding.ASCII.GetBytes(bundleJsonString);
                ServerInfo = ReadServerInfoFromJSON(jsonBytes);
                var tempDict = ReadBundleListFromJSON(jsonBytes);
                if(tempDict.Count > 0)
                {
                    _bundleDictionary = tempDict;
                }

                UpdateProcessingBundleStatus();

                if (SuccessCallback != null)
                {
                    SuccessCallback();
                }
                if (BundlesWindow.Window != null)
                {
                    BundlesWindow.Window.Repaint();
                }
            }
        }

        private void UpdateProcessingBundleStatus()
        {
            var serverQueue = ServerInfo.ProcessingQueue;
            var bundleEnumerator = _bundleDictionary.GetEnumerator();
            while (bundleEnumerator.MoveNext())
            {
                Bundle bundle = bundleEnumerator.Current.Value;
                if(bundle.OperationQueue.Count > 0)
                {
                    bundle.Status = BundleStatus.Queued;

                    var operationsEnumerator = bundle.OperationQueue.GetEnumerator();
                    operationsEnumerator.MoveNext();

                    var serverOperationsEnumerator = serverQueue.GetEnumerator();
                    serverOperationsEnumerator.MoveNext();

                    if(operationsEnumerator.Current.Key == serverOperationsEnumerator.Current.Key)
                    {
                        bundle.Status = BundleStatus.Processing;
                    }
                }
            }
        }

        private Dictionary<string, Bundle> ReadBundleListFromJSON(byte[] jsonBytes)
        {
            Dictionary<string, Bundle> bundleDictionary = new Dictionary<string, Bundle>();

            JsonAttrParser parser = new JsonAttrParser();
            Attr jsonParsed = parser.Parse(jsonBytes);

            AttrList jsonList = jsonParsed.AsDic["bundles"].AsList;
            for(int i = 0; i < jsonList.Count; i++)
            {
                AttrDic jsonRow = jsonList[i].AsDic;
                string bundleName = jsonRow["name"].AsValue.ToString();

                

                Asset asset = new Asset(jsonRow["assetguid"].AsValue.ToString());

                var sizesDict = new Dictionary<BundlePlaform, int>();
                var jsonSizes = jsonRow["size"].AsDic;
                var key = jsonSizes.Keys.GetEnumerator();
                while (key.MoveNext())
                {
                    sizesDict.Add((BundlePlaform)Enum.Parse(typeof(BundlePlaform), key.Current.ToString()), jsonSizes[key.Current].AsValue.ToInt());
                }

                var urlDict = new Dictionary<BundlePlaform, string>();
                var jsonUrls = jsonRow["url"].AsDic;
                key = jsonUrls.Keys.GetEnumerator();
                while (key.MoveNext())
                {
                    urlDict.Add((BundlePlaform)Enum.Parse(typeof(BundlePlaform), key.Current.ToString()), jsonUrls[key.Current].AsValue.ToString());
                }

                var operationDict = new Dictionary<int, BundleOperation>();
                var jsonOperations = jsonRow["queue"].AsList;
                
                for(int j = 0; j < jsonOperations.Count; j++)
                {
                    int operationId = jsonOperations[j].AsValue.ToInt();
                    operationDict.Add(operationId, (ServerInfo.ProcessingQueue[operationId]));
                }

                Bundle bundle = new Bundle(bundleName,
                                    sizesDict,
                                    jsonRow["isautogenerated"].AsValue.ToBool(),
                                    jsonRow["islocal"].AsValue.ToBool(),
                                    asset,
                                    new List<Bundle>(),
                                    urlDict,
                                    (BundleStatus)Enum.Parse(typeof(BundleStatus), jsonRow["status"].AsValue.ToString()),
                                    operationDict,
                                    jsonRow["log"].AsValue.ToString()
                                );
                bundleDictionary.Add(asset.Name.ToLower(), bundle);

                //TEMPORARY
                if (NewBundles.ContainsKey(bundleName))
                {
                    NewBundles.Remove(bundleName);
                }
            }

            //Get Parent Bundles
            for(int i = 0; i < jsonList.Count; i++)
            {
                AttrDic jsonRow = jsonList[i].AsDic;
                string childBundleName = jsonRow["name"].AsValue.ToString();
                string childAssetName = childBundleName.Substring(0, childBundleName.LastIndexOf("_"));

                AttrList jsonParents = jsonRow["parents"].AsList;

                for(int j = 0; j < jsonParents.Count; j++)
                {
                    string parentBundleName = jsonParents[j].AsValue.ToString();
                    string parentAssetName = parentBundleName.Substring(0, parentBundleName.LastIndexOf("_"));

                    if(bundleDictionary.ContainsKey(parentAssetName))
                    {
                        bundleDictionary[childAssetName].Parents.Add(bundleDictionary[parentAssetName]);
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Transparent Bundles - Error - The parent bundle '" + parentBundleName + "' was not found in the bundle list. Please, contact the transparent bundles team: " + Config.ContactMail);
                    }
                }
            }

            //TEMPORARY
            var newBundleEnum = NewBundles.GetEnumerator();
            while(newBundleEnum.MoveNext())
            {
                Bundle newBundle = newBundleEnum.Current.Value;
                bundleDictionary.Add(newBundle.Asset.Name.ToLower(), newBundle);
            }

            return bundleDictionary;
        }

        private ServerInfo ReadServerInfoFromJSON(byte[] jsonBytes)
        {
            JsonAttrParser parser = new JsonAttrParser();
            Attr jsonParsed = parser.Parse(jsonBytes);
            AttrDic jsonList = jsonParsed.AsDic["server"].AsDic;

            var processingQueue = new Dictionary<int, BundleOperation>();
            var jsonQueue = jsonList["queue"].AsList;

            for(int i = 0; i < jsonQueue.Count; i++)
            {
                var key = jsonQueue[i].AsDic.Keys.GetEnumerator();
                key.MoveNext();
                processingQueue.Add(int.Parse(key.Current), (BundleOperation)Enum.Parse(typeof(BundleOperation), jsonQueue[i].AsDic[key.Current].AsValue.ToString()));
            }

            return new ServerInfo((ServerStatus)Enum.Parse(typeof(ServerStatus), jsonList["status"].AsValue.ToString()), jsonList["log"].AsValue.ToString(), processingQueue);
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
                if(!_bundleDictionary.ContainsKey(asset.Name.ToLower()))
                {
                    valid = HasValidDependencies(asset);
                }
                else
                {
                    Asset serverAsset = _bundleDictionary[asset.Name.ToLower()].Asset;
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


            if(valid)
            {
                //TEMPORARY
                AddNewBundle(asset);

                TransparentBundleAPI.CreateBundle(new CreateBundlesArgs(new List<string> { asset.Guid }, x => LoadBundleDataFromServer(), x => UnityEngine.Debug.LogError(x.ResponseRes.StatusCode)));
            }

        }

        //TEMPORARY
        private void AddNewBundle(Asset asset)
        {
            if (!_bundleDictionary.ContainsKey(asset.Name.ToLower()) && !NewBundles.ContainsKey(asset.FullName.ToLower().Replace(".", "_")))
            {
                var sizeDict = new Dictionary<BundlePlaform, int>();
                sizeDict.Add(BundlePlaform.android_etc, 0);
                sizeDict.Add(BundlePlaform.ios, 0);
                var urlDict = new Dictionary<BundlePlaform, string>();
                urlDict.Add(BundlePlaform.android_etc, "");
                urlDict.Add(BundlePlaform.ios, "");
                var operationsDict = new Dictionary<int, BundleOperation>();
                operationsDict.Add(-1, BundleOperation.create_asset_bundles);
                Bundle newBundle = new Bundle(asset.FullName.ToLower().Replace(".", "_"), sizeDict, false, false, asset, new List<Bundle>(), urlDict, BundleStatus.Queued, operationsDict, "");
                NewBundles.Add(newBundle.Name, newBundle);
            }
        }

        public void CreateOrUpdateBundles(List<Asset> assets)
        {
            bool valid = true;

            for(int i = 0; i < assets.Count; i++)
            {
                Asset asset = assets[i];

                if(IsValidAsset(asset))
                {
                    if(!_bundleDictionary.ContainsKey(asset.Name.ToLower()))
                    {
                        valid &= HasValidDependencies(asset);
                    }
                    else
                    {
                        Asset serverAsset = _bundleDictionary[asset.Name.ToLower()].Asset;
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
            }

            if(valid)
            {
                List<string> guids = new List<string>();
                for(int i = 0; i < assets.Count; i++)
                {
                    guids.Add(assets[i].Guid);
                    
                    //TEMPORARY
                    AddNewBundle(assets[i]);
                }
                TransparentBundleAPI.CreateBundle(new CreateBundlesArgs(guids, x => LoadBundleDataFromServer(), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
            }
        }



        public void RemoveBundle(Asset asset)
        {
            if(_bundleDictionary.ContainsKey(asset.Name.ToLower()))
            {
                TransparentBundleAPI.RemoveBundle(new RemoveBundlesArgs(new List<string> { asset.Guid }, x => LoadBundleDataFromServer(), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
            }
        }

        public void RemoveBundles(List<Asset> assets)
        {
            bool valid = true;

            for(int i = 0; i < assets.Count; i++)
            {
                Asset asset = assets[i];
                valid &= _bundleDictionary.ContainsKey(asset.Name.ToLower());
            }

            if(valid)
            {
                List<string> guids = new List<string>();
                for(int i = 0; i < assets.Count; i++)
                {
                    guids.Add(assets[i].Guid);
                }
                TransparentBundleAPI.RemoveBundle(new RemoveBundlesArgs(guids, x => LoadBundleDataFromServer(), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
            }
        }

        public void BundleIntoBuild(Asset asset)
        {
            if(_bundleDictionary.ContainsKey(asset.Name.ToLower()) && !_bundleDictionary[asset.Name.ToLower()].IsLocal)
            {
                TransparentBundleAPI.MakeLocalBundle(new MakeLocalBundlesArgs(new List<string> { asset.Guid }, x => LoadBundleDataFromServer(), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
            }
        }

        public void BundlesIntoBuild(List<Asset> assets)
        {
            bool valid = true;

            for(int i = 0; i < assets.Count; i++)
            {
                Asset asset = assets[i];
                valid &= _bundleDictionary.ContainsKey(asset.Name.ToLower());
            }

            if(valid)
            {
                List<string> guids = new List<string>();
                for(int i = 0; i < assets.Count; i++)
                {
                    guids.Add(assets[i].Guid);
                }
                TransparentBundleAPI.MakeLocalBundle(new MakeLocalBundlesArgs(guids, x => LoadBundleDataFromServer(), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
            }
        }

        public void BundleOutsideBuild(Asset asset)
        {
            if(_bundleDictionary.ContainsKey(asset.Name.ToLower()) && _bundleDictionary[asset.Name.ToLower()].IsLocal)
            {
                TransparentBundleAPI.RemoveLocalBundle(new RemoveLocalBundlesArgs(new List<string> { asset.Guid }, x => LoadBundleDataFromServer(), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
            }
        }

        public void BundlesOutsideBuild(List<Asset> assets)
        {
            bool valid = true;

            for(int i = 0; i < assets.Count; i++)
            {
                Asset asset = assets[i];
                valid &= _bundleDictionary.ContainsKey(asset.Name.ToLower());
            }

            if(valid)
            {
                List<string> guids = new List<string>();
                for(int i = 0; i < assets.Count; i++)
                {
                    guids.Add(assets[i].Guid);
                }
                TransparentBundleAPI.RemoveLocalBundle(new RemoveLocalBundlesArgs(guids, x => LoadBundleDataFromServer(), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
            }
        }

        public List<Bundle> GetBundles(string searchText)
        {
            List<Bundle> bundleList = new List<Bundle>();
            List<string> keys = new List<string>(_bundleDictionary.Keys);
            for(int i = 0; i < keys.Count; i++)
            {
                if(_bundleDictionary[keys[i]].IsAutogerated == false && keys[i].IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    bundleList.Add(_bundleDictionary[keys[i]]);
                }
            }
            return bundleList;
        }

        public Bundle GetBundleFromAsset(Asset asset)
        {
            if(_bundleDictionary.ContainsKey(asset.Name.ToLower()))
            {
                return _bundleDictionary[asset.Name.ToLower()];
            }
            else
            {
                return null;
            }

        }

        public Bundle GetBundleFromAsset(string assetName)
        {
            if(_bundleDictionary.ContainsKey(assetName.ToLower()))
            {
                return _bundleDictionary[assetName.ToLower()];
            }
            else
            {
                return null;
            }

        }

        public int GetLocalBundlesTotalSize(BundlePlaform platform)
        {
            int totalSize = 0;
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

        public int GetServerBundlesTotalSize(BundlePlaform platform)
        {
            int totalSize = 0;
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

        public void DownloadBundle(Bundle bundle, BundlePlaform platform)
        {
            _downloader.DownloadBundle(bundle, platform);
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
                    bundleList.Sort(delegate (Bundle b1, Bundle b2)
                    {
                        return ((b2.Asset.Type.ToString() + b1.Asset.Name).CompareTo(b1.Asset.Type.ToString() + b2.Asset.Name));
                    });
                    break;

                case BundleSortingMode.TypeAsc:
                    bundleList.Sort(delegate (Bundle b1, Bundle b2)
                    {
                        return ((b1.Asset.Type.ToString() + b1.Asset.Name).CompareTo(b2.Asset.Type.ToString() + b2.Asset.Name));
                    });
                    break;

                case BundleSortingMode.NameDesc:
                    bundleList.Sort(delegate (Bundle b1, Bundle b2)
                    {
                        return b2.Name.CompareTo(b1.Name);
                    });
                    break;

                case BundleSortingMode.NameAsc:
                    bundleList.Sort(delegate (Bundle b1, Bundle b2)
                    {
                        return b1.Name.CompareTo(b2.Name);
                    });
                    break;

                case BundleSortingMode.SizeDesc:
                    bundleList.Sort(delegate (Bundle b1, Bundle b2)
                    {
                        return (b2.Size[platform].ToString() + b1.Asset.Name).CompareTo(b1.Size[platform].ToString() + b2.Asset.Name);
                    });
                    break;

                case BundleSortingMode.SizeAsc:
                    bundleList.Sort(delegate (Bundle b1, Bundle b2)
                    {
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
                    assetList.Sort(delegate (Asset b1, Asset b2)
                    {
                        return ((b2.Type.ToString() + b1.Name).CompareTo(b1.Type.ToString() + b2.Name));
                    });
                    break;

                case AssetSortingMode.TypeAsc:
                    assetList.Sort(delegate (Asset b1, Asset b2)
                    {
                        return ((b1.Type.ToString() + b1.Name).CompareTo(b2.Type.ToString() + b2.Name));
                    });
                    break;

                case AssetSortingMode.NameDesc:
                    assetList.Sort(delegate (Asset b1, Asset b2)
                    {
                        return b2.Name.CompareTo(b1.Name);
                    });
                    break;

                case AssetSortingMode.NameAsc:
                    assetList.Sort(delegate (Asset b1, Asset b2)
                    {
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

        public void FlushImagesCache()
        {
            _downloader.FlushImagesCache();
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
