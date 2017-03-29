using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using SocialPoint.Attributes;
using UnityEditor;
using UnityEngine;

namespace SocialPoint.TransparentBundles
{
    public class EditorClientController
    {
        static EditorClientController _instance;
        static Downloader _downloader;
        public Dictionary<string, List<Asset>> DependenciesCache, ReferencesCache;
        public Dictionary<string, bool> SharedDependenciesCache;
        
        Dictionary<string, Bundle> _bundleDictionary;
        public ServerInfo ServerInfo;
        bool _requestPending;

        //TEMPORARY
        public Dictionary<string, Bundle> NewBundles;

        EditorClientController()
        {
            //Mounts the smb folder
#if UNITY_EDITOR_OSX
            if(!Directory.Exists(Config.IconsPath))
            {
                var process = new ProcessStartInfo();
                process.UseShellExecute = false;
                process.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                process.FileName = "mkdir";
                process.Arguments = "-p " + Config.VolumePath;
                process.RedirectStandardError = true;
                var run = Process.Start(process);
                while(!run.StandardError.EndOfStream)
                {
                    UnityEngine.Debug.LogError(run.StandardError.ReadLine());
                }
                if(!run.WaitForExit(10000))
                {
                    if(EditorUtility.DisplayDialog("Transparent Bundles", "The folder '" + Config.VolumePath + "' was not able to be created. Please, contact the Transparent Bundles developers: " + Config.ContactUrl, "Close"))
                    {
                        BundlesWindow.Window.Close();
                    }
                    return;
                }

                process = new ProcessStartInfo();
                process.UseShellExecute = false;
                process.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                process.FileName = "mount_smbfs";
                process.Arguments = Config.SmbConnectionUrl + " " + Config.VolumePath;
                process.RedirectStandardError = true;
                run = Process.Start(process);
                while(!run.StandardError.EndOfStream)
                {
                    UnityEngine.Debug.LogError(run.StandardError.ReadLine());
                }
                if(!run.WaitForExit(10000))
                {
                    if(EditorUtility.DisplayDialog("Transparent Bundles", "Connection timeout. Please, make sure that you are connected to the SocialPoint network: \n wifi: 'SP_EMPLOYEE'", "Close"))
                    {
                        BundlesWindow.Window.Close();
                    }
                }

                run.Close();
            }
#endif
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
            if(!_requestPending)
            {
                _requestPending = true;
                TransparentBundleAPI.GetBundles(new GetBundlesArgs(x => ImportBundleData(x.ResponseRes.Response, SuccessCallback), x => UnityEngine.Debug.LogError(x.ResponseRes.Response)));
            }
        }

        void ImportBundleData(string bundleJsonString, Action SuccessCallback = null)
        {
            _requestPending = false;
            if(bundleJsonString.Length > 0)
            {
                byte[] jsonBytes = Encoding.ASCII.GetBytes(bundleJsonString);
                ServerInfo = ReadServerInfoFromJSON(jsonBytes);
                _bundleDictionary = ReadBundleListFromJSON(jsonBytes);

                UpdateProcessingBundleStatus();

                if(SuccessCallback != null)
                {
                    SuccessCallback();
                }
                if(BundlesWindow.Window != null)
                {
                    BundlesWindow.Window.Repaint();
                }
            }
        }

        void UpdateProcessingBundleStatus()
        {
            var serverQueue = ServerInfo.ProcessingQueue;
            var bundleEnumerator = _bundleDictionary.GetEnumerator();
            while(bundleEnumerator.MoveNext())
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

                    operationsEnumerator.Dispose();
                    serverOperationsEnumerator.Dispose();
                }
            }
            bundleEnumerator.Dispose();
        }

        Dictionary<string, Bundle> ReadBundleListFromJSON(byte[] jsonBytes)
        {
            var bundleDictionary = new Dictionary<string, Bundle>();

            var parser = new JsonAttrParser();
            Attr jsonParsed = parser.Parse(jsonBytes);

            AttrList jsonList = jsonParsed.AsDic["bundles"].AsList;
            for(int i = 0; i < jsonList.Count; i++)
            {
                AttrDic jsonRow = jsonList[i].AsDic;
                string bundleName = jsonRow["name"].AsValue.ToString();

                var asset = new Asset(jsonRow["assetguid"].AsValue.ToString());

                var sizesDict = new Dictionary<BundlePlaform, int>();
                var jsonSizes = jsonRow["size"].AsDic;
                var key = jsonSizes.Keys.GetEnumerator();
                while(key.MoveNext())
                {
                    sizesDict.Add((BundlePlaform)Enum.Parse(typeof(BundlePlaform), key.Current), jsonSizes[key.Current].AsValue.ToInt());
                }
                key.Dispose();

                var urlDict = new Dictionary<BundlePlaform, string>();
                var jsonUrls = jsonRow["url"].AsDic;
                key = jsonUrls.Keys.GetEnumerator();
                while(key.MoveNext())
                {
                    urlDict.Add((BundlePlaform)Enum.Parse(typeof(BundlePlaform), key.Current), jsonUrls[key.Current].AsValue.ToString());
                }
                key.Dispose();

                var operationDict = new Dictionary<int, BundleOperation>();
                var jsonOperations = jsonRow["queue"].AsList;

                for(int j = 0; j < jsonOperations.Count; j++)
                {
                    int operationId = jsonOperations[j].AsValue.ToInt();
                    operationDict.Add(operationId, (ServerInfo.ProcessingQueue[operationId]));
                }

                var bundle = new Bundle(bundleName,
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

                if(asset.Name.Length == 0)
                {
                    ErrorDisplay.DisplayError(ErrorType.assetNotFoundInBundle, false, true, true, bundleName, asset.Guid);
                }
                else
                {
                    bundleDictionary.Add(GetFixedAssetName(asset.FullName), bundle);
                }

                //TEMPORARY
                if(NewBundles.ContainsKey(bundleName) && !bundleDictionary[GetFixedAssetName(asset.FullName)].IsAutogenerated)
                {
                    NewBundles.Remove(bundleName);
                }
            }

            //Get Parent Bundles
            for(int i = 0; i < jsonList.Count; i++)
            {
                AttrDic jsonRow = jsonList[i].AsDic;
                string childBundleName = jsonRow["name"].AsValue.ToString();
                string childAssetName = GetFixedAssetName(childBundleName.Substring(0, childBundleName.LastIndexOf("_"))+"."+ childBundleName.Substring(childBundleName.LastIndexOf("_")+1));
                if(bundleDictionary.ContainsKey(childAssetName))
                {
                    AttrList jsonParents = jsonRow["parents"].AsList;

                    for(int j = 0; j < jsonParents.Count; j++)
                    {
                        string parentBundleName = jsonParents[j].AsValue.ToString();
                        string parentAssetName = GetFixedAssetName(parentBundleName.Substring(0, parentBundleName.LastIndexOf("_")) + "." + parentBundleName.Substring(parentBundleName.LastIndexOf("_")+1));

                        if(bundleDictionary.ContainsKey(parentAssetName))
                        {
                            bundleDictionary[childAssetName].Parents.Add(bundleDictionary[parentAssetName]);
                        }
                        else
                        {
                            ErrorDisplay.DisplayError(ErrorType.parentBundleNotFound, false, true, false, parentBundleName);
                        }
                    }
                }
            }

            //TEMPORARY
            var newBundleEnum = NewBundles.GetEnumerator();
            while(newBundleEnum.MoveNext())
            {
                Bundle newBundle = newBundleEnum.Current.Value;
                string newBundleAssetName = GetFixedAssetName(newBundle.Asset.FullName);

                if(!bundleDictionary.ContainsKey(newBundleAssetName))
                {
                    bundleDictionary.Add(newBundleAssetName, newBundle);
                }
                else if(bundleDictionary[newBundleAssetName].IsAutogenerated)
                {
                    bundleDictionary[newBundleAssetName] = newBundle;
                }
            }
            newBundleEnum.Dispose();

            return bundleDictionary;
        }

        static ServerInfo ReadServerInfoFromJSON(byte[] jsonBytes)
        {
            var parser = new JsonAttrParser();
            Attr jsonParsed = parser.Parse(jsonBytes);
            AttrDic jsonList = jsonParsed.AsDic["server"].AsDic;

            var processingQueue = new Dictionary<int, BundleOperation>();
            var jsonQueue = jsonList["queue"].AsList;

            for(int i = 0; i < jsonQueue.Count; i++)
            {
                var key = jsonQueue[i].AsDic.Keys.GetEnumerator();
                key.MoveNext();
                processingQueue.Add(int.Parse(key.Current), (BundleOperation)Enum.Parse(typeof(BundleOperation), jsonQueue[i].AsDic[key.Current].AsValue.ToString()));
                key.Dispose();
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

        static bool IsValidAsset(Asset asset)
        {
            bool valid = true;
            if(asset.Type == "UnityEditor.DefaultAsset")
            {
                valid = false;
                EditorUtility.DisplayDialog("Asset issue", "You cannot create a bundle of a folder.\n\nVisit the following link for more info: \n" + Config.HelpUrl, "Close");
            }
            else
            {
                if(asset.GetAssetObject() == null)
                {
                    ErrorDisplay.DisplayError(ErrorType.assetNotFound, true, false, false, asset.Name);
                    return false;
                }
                string[] assetsPath = AssetDatabase.GetDependencies(AssetDatabase.GUIDToAssetPath(asset.Guid));
                var pendingAssetMessages = GitChecker.CheckFilePending(assetsPath);
                if(pendingAssetMessages.Count > 0)
                {
                    valid = false;
                    string penginAssetsText = "";
                    for(int i = 0; i < pendingAssetMessages.Count; i++)
                    {
                        penginAssetsText += pendingAssetMessages[i]+"\n";
                    }
                    ErrorDisplay.DisplayError(ErrorType.assetNotFound, true, false, false, penginAssetsText);
                }
            }
            

            return valid;
        }

        static bool HasValidDependencies(Asset asset)
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

        
        //TEMPORARY
        void AddNewBundle(Asset asset)
        {
            if((!_bundleDictionary.ContainsKey(GetFixedAssetName(asset.FullName)) || _bundleDictionary[GetFixedAssetName(asset.FullName)].IsAutogenerated) && !NewBundles.ContainsKey(asset.FullName.ToLower().Replace(".", "_").Replace(" ", "_")))
            {
                var sizeDict = new Dictionary<BundlePlaform, int>();
                sizeDict.Add(BundlePlaform.android_etc, 0);
                sizeDict.Add(BundlePlaform.ios, 0);
                var urlDict = new Dictionary<BundlePlaform, string>();
                urlDict.Add(BundlePlaform.android_etc, "");
                urlDict.Add(BundlePlaform.ios, "");
                var operationsDict = new Dictionary<int, BundleOperation>();
                operationsDict.Add(-1, BundleOperation.create_asset_bundles);
                var newBundle = new Bundle(asset.FullName.ToLower().Replace(".", "_"), sizeDict, false, false, asset, new List<Bundle>(), urlDict, BundleStatus.Queued, operationsDict, "");
                NewBundles.Add(newBundle.Name, newBundle);
            }
        }

        public bool CreateOrUpdateBundles(List<Asset> assets)
        {
            bool valid = true;

            for(int i = 0; i < assets.Count; i++)
            {
                Asset asset = assets[i];

                if(IsValidAsset(asset))
                {
                    string fixedAssetName = GetFixedAssetName(asset.FullName);

                    if(!_bundleDictionary.ContainsKey(fixedAssetName))
                    {
                        valid &= HasValidDependencies(asset);
                    }
                    else
                    {
                        Asset serverAsset = _bundleDictionary[fixedAssetName].Asset;
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
                var guids = new List<string>();
                for(int i = 0; i < assets.Count; i++)
                {
                    guids.Add(assets[i].Guid);

                    //TEMPORARY
                    AddNewBundle(assets[i]);
                }
                TransparentBundleAPI.CreateBundle(new CreateBundlesArgs(guids, x => LoadBundleDataFromServer(), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
            }
            return valid;
        }

        public bool PerfomBundleOperation(List<Asset> assets, BundleOperation operation)
        {
            bool valid = true;

            for(int i = 0; i < assets.Count && valid; i++)
            {
                Asset asset = assets[i];
                valid &= IsValidAsset(asset) && _bundleDictionary.ContainsKey(GetFixedAssetName(asset.FullName));
            }

            if(valid)
            {
                var guids = new List<string>();
                for(int i = 0; i < assets.Count; i++)
                {
                    guids.Add(assets[i].Guid);
                }
                switch(operation)
                {
                case BundleOperation.create_local_asset_bundles:
                    TransparentBundleAPI.MakeLocalBundle(new MakeLocalBundlesArgs(guids, x => LoadBundleDataFromServer(), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
                    break;
                case BundleOperation.remove_asset_bundles:
                    TransparentBundleAPI.RemoveBundle(new RemoveBundlesArgs(guids, x => LoadBundleDataFromServer(), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
                    break;
                case BundleOperation.remove_local_asset_bundles:
                    TransparentBundleAPI.RemoveLocalBundle(new RemoveLocalBundlesArgs(guids, x => LoadBundleDataFromServer(), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
                    break;
                }
            }
            return valid;
        }

        public void CancelBundleOperation(int operationId)
        {
            TransparentBundleAPI.CancelRequest(new CancelRequestArgs(operationId, x => LoadBundleDataFromServer(), x => UnityEngine.Debug.LogError(x.RequestCancelled)));
        }

        public string GetFixedAssetName(string assetName)
        {
            return assetName.ToLower().Replace(" ", "_");
        }

        public List<Bundle> GetBundles(string searchText)
        {
            var bundleList = new List<Bundle>();
            var keys = new List<string>(_bundleDictionary.Keys);
            for(int i = 0; i < keys.Count; i++)
            {
                if(!_bundleDictionary[keys[i]].IsAutogenerated && keys[i].IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    bundleList.Add(_bundleDictionary[keys[i]]);
                }
            }
            return bundleList;
        }

        public Bundle GetBundleFromAsset(Asset asset)
        {
            return _bundleDictionary.ContainsKey(GetFixedAssetName(asset.FullName)) ? _bundleDictionary[GetFixedAssetName(asset.FullName)] : null;
        }

        public Bundle GetBundleFromAsset(string assetName)
        {
            return _bundleDictionary.ContainsKey(GetFixedAssetName(assetName)) ? _bundleDictionary[GetFixedAssetName(assetName)] : null;
        }

        public int GetLocalBundlesTotalSize(BundlePlaform platform)
        {
            return GetBundlesTotalSize(platform, true);
        }

        public int GetServerBundlesTotalSize(BundlePlaform platform)
        {
            return GetBundlesTotalSize(platform, false);
        }

        int GetBundlesTotalSize(BundlePlaform platform, bool isLocal)
        {
            int totalSize = 0;
            var keys = new List<string>(_bundleDictionary.Keys);
            for(int i = 0; i < keys.Count; i++)
            {
                Bundle bundle = _bundleDictionary[keys[i]];
                if(bundle.IsLocal == isLocal)
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
            return GetAssetsFromObjects(new[] { assetObject })[0];
        }

        public Asset[] GetAssetsFromObjects(UnityEngine.Object[] objects)
        {
            UnityEngine.Object[] selectedObjects = objects;

            var assets = new List<Asset>();
            for(int i = 0; i < selectedObjects.Length; i++)
            {
                UnityEngine.Object selectedObject = selectedObjects[i];
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetOrScenePath(selectedObject));
                var asset = new Asset(guid, selectedObject.name);
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
            Bundle bundle = GetBundleFromAsset(Path.GetFileName(AssetDatabase.GetAssetPath(assetObject)));
            return (bundle != null && bundle.Asset.Type == assetObject.GetType().ToString());
        }

        public void SortBundles(BundleSortingMode mode, List<Bundle> bundleList, BundlePlaform platform)
        {
            switch(mode)
            {
            case BundleSortingMode.TypeDesc:
                bundleList.Sort((b1, b2) => ((b2.Asset.Type + b2.Asset.Name).CompareTo(b1.Asset.Type + b1.Asset.Name)));
                break;

            case BundleSortingMode.TypeAsc:
                bundleList.Sort((b1, b2) => ((b1.Asset.Type + b1.Asset.Name).CompareTo(b2.Asset.Type + b2.Asset.Name)));
                break;

            case BundleSortingMode.NameDesc:
                bundleList.Sort((b1, b2) => b2.Name.CompareTo(b1.Name));
                break;

            case BundleSortingMode.NameAsc:
                bundleList.Sort((b1, b2) => b1.Name.CompareTo(b2.Name));
                break;

            case BundleSortingMode.SizeDesc:
                bundleList.Sort((b1, b2) => (b2.Size[platform] + b2.Asset.Name).CompareTo(b1.Size[platform] + b1.Asset.Name));
                break;

            case BundleSortingMode.SizeAsc:
                bundleList.Sort((b1, b2) => (b1.Size[platform] + b1.Asset.Name).CompareTo(b2.Size[platform] + b2.Asset.Name));
                break;
            }
        }

        public void SortAssets(AssetSortingMode mode, List<Asset> assetList)
        {
            switch(mode)
            {
            case AssetSortingMode.TypeDesc:
                assetList.Sort((b1, b2) => ((b2.Type + b2.Name).CompareTo(b1.Type + b1.Name)));
                break;

            case AssetSortingMode.TypeAsc:
                assetList.Sort((b1, b2) => ((b1.Type + b1.Name).CompareTo(b2.Type + b2.Name)));
                break;

            case AssetSortingMode.NameDesc:
                assetList.Sort((b1, b2) => b2.Name.CompareTo(b1.Name));
                break;

            case AssetSortingMode.NameAsc:
                assetList.Sort((b1, b2) => b1.Name.CompareTo(b2.Name));
                break;

            }
        }

        public void FlushCache()
        {
            DependenciesCache = new Dictionary<string, List<Asset>>();
            ReferencesCache = new Dictionary<string, List<Asset>>();
            SharedDependenciesCache = new Dictionary<string, bool>();
            ErrorDisplay.FlushErrorCache();
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
