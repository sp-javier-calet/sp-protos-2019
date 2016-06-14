using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SocialPoint.AssetSerializer.Helpers;
using SocialPoint.AssetVersioning;
using SocialPoint.Attributes;
using SocialPoint.Hardware;
using SocialPoint.IO;
using SocialPoint.Utils;
using UnityEngine;
using Uri = System.Uri;

/**
 * DownloadManager is a runtime class for asset steaming and WWW management.
 */
public class DownloadManager : MonoBehaviour
{
    const string kAssetFolder = "Assets/";
    const string _bundleSuffix = "assetBundle";
    const int _downloadRetryTime = 2;
    const int _downloadThreadsCount = 3;
    static bool _useCache = true;
    static bool _useCrc = false;

    string _localBundlesJson;


    string _downloadRootUrl;
    BuildPlatform _currentBuildPlatform;
    string _spamPort;

    IAssetVersioningDictionary _buildStatesLocalDict;
    IAssetVersioningDictionary _assetVersioningDictionary;
    IDeviceInfo _deviceInfo;

    // Request members
    Dictionary<string, WWWRequest> _processingRequest = new Dictionary<string, WWWRequest>();
    Dictionary<string, WWWRequest> _succeedRequest = new Dictionary<string, WWWRequest>();
    Dictionary<string, WWWRequest> _failedRequest = new Dictionary<string, WWWRequest>();
    List<WWWRequest> _waitingRequests = new List<WWWRequest>();
    List<WWWRequest> _requestedBeforeInit = new List<WWWRequest>();

    static event Action LowStorageConditionMet;

    static ulong _minStorageRequiredToDownload;
    // bytes

    protected virtual void OnLowStorageConditionMet()
    {
        var handler = LowStorageConditionMet;
        if(handler != null)
        {
            handler();
        }
    }

    bool _isLowStorageConditionMet;

    public bool IsLowStorageConditionMet
    {
        get
        {
            // if already true, just return true, skip callback
            if(_isLowStorageConditionMet)
            {
                return true;
            }

            ulong freeStorage = _deviceInfo.StorageInfo.FreeStorage; // bytes
            if(freeStorage < _minStorageRequiredToDownload)
            {
                _isLowStorageConditionMet = true;
                OnLowStorageConditionMet();
            }
            return _isLowStorageConditionMet;
        }

        set
        {
            _isLowStorageConditionMet = value;
        }
    }



    /**
     * Get instance of DownloadManager.
     * This prop will create a GameObject named Downlaod Manager in scene when first time called.
     */
    static DownloadManager _instance;

    public static DownloadManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new GameObject("Download Manager").AddComponent<DownloadManager>();

                DontDestroyOnLoad(_instance.gameObject);
                _instance.LoadBundleJsonFile();
            }

            return _instance;
        }
    }

    public string SpamPort
    {
        get
        {
            if(_spamPort == null)
            {
                _spamPort = PlatformToSpamPort(_currentBuildPlatform);
            }
            return _spamPort;
        }
    }

    public static GameObject GetGOWithLinkedBehaviours(WWW www, string path)
    {
        GameObject go = null;
        if(www != null)
        {
            if(www.assetBundle != null)
            {
                go = www.assetBundle.LoadAsset(path, typeof(GameObject)) as GameObject;
                if(go != null)
                {
                    LinkBehaviours(go, path, www.assetBundle);
                }
                else
                {
                    Debug.LogError("go is NULL!! Path: " + path);
                }
            }
            else
            {
                Debug.LogError("www.assetBundle is NULL!! Path: " + path);
            }
        }
        else
        {
            Debug.LogError("WWW is NULL!! Path: " + path);
        }
        return go;
    }

    public static void LinkBehaviours(GameObject go, string path, AssetBundle bundle)
    {
        string assetPath = path;
        if(StringUtils.StartsWith(assetPath, kAssetFolder))
        {
            assetPath = assetPath.Substring(assetPath.IndexOf("/") + 1);
        }
        assetPath = assetPath + "_JSON_Data";

        var textAsset = bundle.LoadAsset(assetPath, typeof(TextAsset)) as TextAsset;

        if(textAsset != null)
        {
            Debug.Log(" -> Loading JSON data asset: " + assetPath);
            ComponentHelper.RemoveAllBehaviours(go, true);
            ComponentHelper.DeserializeObject(go, textAsset.text);
        } 
        //If the serialization data could not be found, this asset didn't need it in the first place
    }

    /// <summary>
    /// Utility function to convert from platform to corresponding port route used by the SPAM Bundle Client. Will always be lowercase
    /// </summary>
    static string PlatformToSpamPort(BuildPlatform platform)
    {
        string icase_port;
        switch(platform)
        {
        case BuildPlatform.IOS:
            {
                icase_port = "IOS";
                break;
            }
        case BuildPlatform.Android:
            {
                icase_port = "Android" + "_" + BMUrls.GetBestTextureFormatSupport();
                break;
            }
        default:
            icase_port = PlatformToSpamPort(BuildPlatform.IOS);
            #if UNITY_ANDROID
            icase_port = PlatformToSpamPort(BuildPlatform.Android);
            #endif
            Debug.LogWarning(String.Format("Unsupported targetted platform for asset bundles '{0}'. Defaulting to '{1}'.", platform, icase_port));
            break;
        }
        return icase_port.ToLowerInvariant();
    }

    /**
     * Get WWW instance of the url.
     * @return Return null if the WWW request haven't succeed.
     */ 
    public WWW GetWWW(string assetBundleName, Action<string> callback = null)
    {
        if(_succeedRequest.ContainsKey(assetBundleName))
        {
            var request = _succeedRequest[assetBundleName];

            if(callback != null)
                request.callback = callback;
            
            return request.www;
        }

        return null;
    }

    public void Initialize(string baseUrl, IAssetVersioningDictionary assetVersioningDictionary, IDeviceInfo deviceInfo, ulong minStorageRequiredToDownload = 0, Action LowStorageConditionMetAction = null)
    {
        SetCurrentPlatform();

        initRootUrl(baseUrl);
        _assetVersioningDictionary = assetVersioningDictionary;
        _deviceInfo = deviceInfo;
        _minStorageRequiredToDownload = minStorageRequiredToDownload;
        LowStorageConditionMet = LowStorageConditionMetAction;
    }

    public IEnumerator WaitDownload(string assetBundleName, bool useAssetVersioning = true)
    {
        if(useAssetVersioning)
            yield return StartCoroutine(WaitDownload(assetBundleName, -1));
        else
            yield return StartCoroutine(WaitDownloadWithoutVersioning(assetBundleName, -1));
    }

    /**
     * Coroutine for download waiting. 
     * You should use it like this,
     * yield return StartCoroutine(DownloadManager.Instance.WaitDownload("bundle1.assetbundle"));
     * If this url didn't been requested, the coroutine will start a new request.
     */ 
    public IEnumerator WaitDownload(string assetBundleName, int priority, Action<string> callback = null)
    {
        var request = new WWWRequest();
        request.bundleName = assetBundleName;
        request.url = formatUrl(assetBundleName + "." + _bundleSuffix);
        request.urlNoPath = assetBundleName;
        request.callback = callback;
        request.priority = priority;
        download(request, true);
        
        while(isDownloadingWWW(assetBundleName))
        {
            yield return null;
        }
    }

    /**
     * Coroutine for download waiting. 
     * You should use it like this,
     * yield return StartCoroutine(DownloadManager.Instance.WaitDownload("bundle1.assetbundle"));
     * If this url didn't been requested, the coroutine will start a new request.
     */ 
    public IEnumerator WaitDownloadWithoutVersioning(string assetBundleName, int priority, Action<string> callback = null)
    {
        var request = new WWWRequest();
        request.bundleName = assetBundleName;
        request.url = formatUrl(assetBundleName + "." + _bundleSuffix);
        request.urlNoPath = assetBundleName;
        request.callback = callback;
        request.priority = priority;
        download(request, false);

        while(isDownloadingWWW(assetBundleName))
        {
            yield return null;
        }
    }

    public BuildPlatform GetRuntimePlatform()
    {
        if(Application.platform == RuntimePlatform.WindowsPlayer ||
           Application.platform == RuntimePlatform.OSXPlayer)
        {
            return BuildPlatform.Standalones;
        }
        if(Application.platform == RuntimePlatform.OSXWebPlayer ||
           Application.platform == RuntimePlatform.WindowsWebPlayer)
        {
            return BuildPlatform.WebPlayer;
        }
        if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return BuildPlatform.IOS;
        }
        return Application.platform == RuntimePlatform.Android ? BuildPlatform.Android : BuildPlatform.Standalones;
    }

    public void StartDownload(string bundleName, Action<string> callback = null)
    {
        StartDownload(bundleName, -1, callback);
    }

    /**
     * Start a new download request.
     * @param url The url for download. Can be a absolute or relative url.
     * @param priority Priority for this request.
     */ 
    public void StartDownload(string bundleName, int priority, Action<string> callback = null)
    {
        var request = new WWWRequest();
        request.url = formatUrl(bundleName + "." + _bundleSuffix);
        request.bundleName = bundleName;
        request.urlNoPath = bundleName;
        request.priority = priority;
        request.callback = callback;

        download(request, true);
    }

    /**
     * Stop a request.
     */ 
    public void StopDownload(string assetBundleName)
    {
        _waitingRequests.RemoveAll(x => x.bundleName == assetBundleName);
        
        if(_processingRequest.ContainsKey(assetBundleName))
        {
            _processingRequest[assetBundleName].www.Dispose();
            _processingRequest.Remove(assetBundleName);
        }
    }

    public void DisposeAll()
    {
        var itr = _succeedRequest.Concat(_failedRequest).GetEnumerator();
        while(itr.MoveNext())
        {
            var kvp = itr.Current;
            try
            {
                var bundle = kvp.Value.www.assetBundle;
                if(bundle != null)
                {
                    bundle.Unload(true);
                }
            }
            catch
            {
                Debug.LogError("Failed to unload bundle: " + kvp.Key);
            }
        }
        itr.Dispose();

        if(_assetVersioningDictionary != null)
        {
            var itr2 = _assetVersioningDictionary.GetEnumerator();
            while(itr2.MoveNext())
            {
                var pair = itr2.Current;
                DisposeWWW(pair.Key + "." + _bundleSuffix);
            }
            itr2.Dispose();
        }
    }

    HashSet<string> GetDependenciesWaitingAndProcessingRequest()
    {
        var dependencies = new HashSet<string>();
        for(int i = 0, _waitingRequestsCount = _waitingRequests.Count; i < _waitingRequestsCount; i++)
        {
            WWWRequest request = _waitingRequests[i];
            List<string> reqDependencies = getDependList(request.bundleName);
            for(int j = 0; j < reqDependencies.Count; ++j)
            {
                if(!dependencies.Contains(reqDependencies[j]))
                {
                    dependencies.Add(reqDependencies[j]);
                }
            }
        }

        var itr = _processingRequest.GetEnumerator();
        while(itr.MoveNext())
        {
            var kvp = itr.Current;
            List<string> reqDependencies = getDependList(kvp.Key);
            for(int i = 0; i < reqDependencies.Count; ++i)
            {
                if(!dependencies.Contains(reqDependencies[i]))
                {
                    dependencies.Add(reqDependencies[i]);
                }
            }
        }
        itr.Dispose();

        return dependencies;
    }

    /**
     * Dispose a finished WWW request.
     */ 
    public void DisposeWWW(string assetBundleName)
    {
        StopDownload(assetBundleName);
        
        if(_succeedRequest.ContainsKey(assetBundleName))
        {
            _succeedRequest[assetBundleName].www.Dispose();
            _succeedRequest.Remove(assetBundleName);
        }
        
        if(_failedRequest.ContainsKey(assetBundleName))
        {
            _failedRequest[assetBundleName].www.Dispose();
            _failedRequest.Remove(assetBundleName);
        }
    }

    bool BundleIsParent(string bundleName)
    {
        var itr = _assetVersioningDictionary.GetEnumerator();
        while(itr.MoveNext())
        {
            var kvp = itr.Current;
            if(kvp.Value.Parent == bundleName)
            {
                itr.Dispose();
                return true;
            }
        }
        itr.Dispose();

        return false;
    }

    /**
     * This function will stop all request in processing.
     */ 
    public void StopAll()
    {
        _requestedBeforeInit.Clear();
        _waitingRequests.Clear();

        var itr = _processingRequest.Values.GetEnumerator();
        while(itr.MoveNext())
        {
            var request = itr.Current;
            request.www.Dispose();
        }
        itr.Dispose();
        
        _processingRequest.Clear();
    }

    public bool ContainedInSucceedRequests(string bundleName)
    {
        return _succeedRequest.ContainsKey(bundleName);
    }

    void LoadBundleJsonFile()
    {
        _buildStatesLocalDict = new AssetVersioningDictionary();
        _localBundlesJson = Path.Combine(PathsManager.StreamingAssetsPath, "json/localBundlesJsonSpam.json");

        string bundleLocalJson = FileUtils.ReadAllText(_localBundlesJson);
        try
        {
            var parserJson = new JsonAttrParser();
            AttrDic json = parserJson.Parse(System.Text.Encoding.ASCII.GetBytes(bundleLocalJson)).AsDic;

            AttrList attrBundles = json.AsDic.Get("bundles").AsList;
            for(int i = 0; i < attrBundles.Count; i++)
            {
                Attr obj = attrBundles[i];

                string bundleName = obj.AsDic.Get("bundleName").ToString();

                var data = new AssetVersioningData();
                data.Version = obj.AsDic.GetValue("bundleVersion").ToInt();
                data.CRC = Convert.ToUInt32(obj.AsDic.Get("bundleCRC").ToString());
                data.Client = obj.AsDic.Get("bundleClient").ToString();

                _buildStatesLocalDict.Add(bundleName, data);
            }
            
        }
        catch(Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    void SetCurrentPlatform()
    {
        if(Application.platform == RuntimePlatform.WindowsEditor ||
           Application.platform == RuntimePlatform.OSXEditor)
        {
            // This allows targeting platform bundles from the Editor
            #if UNITY_IOS
            _currentBuildPlatform = BuildPlatform.IOS;
            #elif UNITY_ANDROID
            _currentBuildPlatform = BuildPlatform.Android;
            #endif
        }
        else
        {
            _currentBuildPlatform = GetRuntimePlatform();
        }
    }

    void Update()
    {
        if(!Caching.ready)
        {
            return;
        }

        // Check if any WWW is finished or errored
        var newFinisheds = new List<string>();
        var newFaileds = new List<string>();
        var itr = _processingRequest.Values.GetEnumerator();
        while(itr.MoveNext())
        {
            var request = itr.Current;
            if(request.www.error != null)
            {
                if(request.triedTimes - 1 < _downloadRetryTime)
                {
                    // Retry download
                    request.CreatWWW();
                }
                else
                {
                    newFaileds.Add(request.bundleName);
                    string error = "Download " + request.url + " failed for " + request.triedTimes + " times.\nError: " + request.www.error;
                    if(request.callback != null)
                    {
                        request.callback.Invoke(error);
                    }
                    else
                    {
#if ADMIN_PANEL
                        Debug.LogError(error);
#endif
                    }
                }
            }
            else if(request.www.isDone)
            {
                newFinisheds.Add(request.bundleName);
                if(request.callback != null)
                {
                    request.callback.Invoke(request.urlNoPath);
                }
            }
        }
        itr.Dispose();
        
        // Move complete bundles out of downloading list
        for(int i = 0, newFinishedsCount = newFinisheds.Count; i < newFinishedsCount; i++)
        {
            string finishedBundles = newFinisheds[i];
            _succeedRequest.Add(finishedBundles, _processingRequest[finishedBundles]);
            _processingRequest.Remove(finishedBundles);
        }
        
        // Move failed bundles out of downloading list
        for(int i = 0, newFailedsCount = newFaileds.Count; i < newFailedsCount; i++)
        {
            string finishedBundles = newFaileds[i];
            if(!_failedRequest.ContainsKey(finishedBundles))
            {
                _failedRequest.Add(finishedBundles, _processingRequest[finishedBundles]);
            }
            _processingRequest.Remove(finishedBundles);
        }
        
        // Start download new bundles
        int waitingIndex = 0;
        while(_processingRequest.Count < _downloadThreadsCount &&
              waitingIndex < _waitingRequests.Count)
        {
            WWWRequest curRequest = _waitingRequests[waitingIndex++];
            
            bool canStartDownload = (curRequest.assetVersioningData == null || isBundleDependenciesReady(curRequest.bundleName)) && !IsLowStorageConditionMet;
            if(canStartDownload)
            {
                _waitingRequests.Remove(curRequest);
                curRequest.CreatWWW();
                _processingRequest.Add(curRequest.bundleName, curRequest);
            }
        }
    }

    bool isBundleDependenciesReady(string bundleName)
    {
        List<string> dependencies = getDependList(bundleName);
        for(int i = 0, dependenciesCount = dependencies.Count; i < dependenciesCount; i++)
        {
            string dependBundle = dependencies[i];
            if(!_succeedRequest.ContainsKey(dependBundle))
            {
                return false;
            }
        }
        
        return true;
    }
    
    // This private method should be called after init
    void download(WWWRequest request, bool useAssetVersioning)
    {
        WWWRequest existingRequest = getDownloadingWWW(request.bundleName);
        if(existingRequest != null)
        {
            var oldCallback = existingRequest.callback;
            existingRequest.callback = msg => { 
                if(request.callback != null)
                    request.callback(msg);
                if(oldCallback != null)
                    oldCallback(msg);
            };
            return;
        }

        if(_succeedRequest.ContainsKey(request.bundleName))
        {
            if(request.callback != null)
            {
                request.callback(request.bundleName);
            }

            return;
        }

        string bundleName = request.bundleName;

        if(useAssetVersioning && !_assetVersioningDictionary.ContainsKey(bundleName))
        {
            string error = "Error: Cannot download bundle [" + bundleName + "]. It's not in the bundle config.";
            if(request.callback != null)
            {
                request.callback.Invoke(error);
            }
            else
            {
                Debug.LogError(error);
            }
            return;
        }

        if(useAssetVersioning)
        {
            List<string> dependlist = getDependList(bundleName);
            for(int i = 0, dependlistCount = dependlist.Count; i < dependlistCount; i++)
            {
                string dependantBundleName = dependlist[i];
                if(!_processingRequest.ContainsKey(dependantBundleName) && !_succeedRequest.ContainsKey(dependantBundleName) && !isInWaitingList(dependantBundleName))
                {
                    var dependRequest = new WWWRequest();
                    dependRequest.bundleName = dependantBundleName;
                    if(useAssetVersioning)
                        dependRequest.assetVersioningData = _assetVersioningDictionary[dependantBundleName];
                    dependRequest.url = formatUrl(dependantBundleName + "." + _bundleSuffix);
                    dependRequest.urlNoPath = bundleName;
                    dependRequest.priority = request.priority;
                    addRequestToWaitingList(dependRequest);
                }
            }
        }

        request.bundleName = bundleName;
        if(useAssetVersioning)
            request.assetVersioningData = _assetVersioningDictionary[bundleName];
        addRequestToWaitingList(request);
    }

    bool isInWaitingList(string bundleName)
    {
        for(int i = 0, _waitingRequestsCount = _waitingRequests.Count; i < _waitingRequestsCount; i++)
        {
            WWWRequest request = _waitingRequests[i];
            if(request.bundleName == bundleName)
            {
                return true;
            }
        }
        
        return false;
    }

    void addRequestToWaitingList(WWWRequest request)
    {
        if(_succeedRequest.ContainsKey(request.bundleName) || isInWaitingList(request.bundleName))
        {
            return;
        }
        
        int insertPos = _waitingRequests.FindIndex(x => x.priority <= request.priority);
        insertPos = insertPos == -1 ? _waitingRequests.Count : insertPos;
        _waitingRequests.Insert(insertPos, request);
    }

    
    bool isDownloadingWWW(string assetBundleName)
    {
        return getDownloadingWWW(assetBundleName) != null;
    }

    WWWRequest getDownloadingWWW(string assetBundleName)
    {
        for(int i = 0, _waitingRequestsCount = _waitingRequests.Count; i < _waitingRequestsCount; i++)
        {
            WWWRequest request = _waitingRequests[i];
            if(request.bundleName == assetBundleName)
            {
                return request;
            }
        }

        return _processingRequest.ContainsKey(assetBundleName) ? _processingRequest[assetBundleName] : null;

    }

    List<string> getDependList(string bundle)
    {
        var res = new List<string>();
        
        if(!_assetVersioningDictionary.ContainsKey(bundle))
        {
            Debug.LogError("Cannot find parent bundle [" + bundle + "], Please check your bundle config.");
            return res;
        }

        var assetVersioningData = _assetVersioningDictionary[bundle];
        while(!string.IsNullOrEmpty(assetVersioningData.Parent))
        {
            bundle = assetVersioningData.Parent;
            assetVersioningData = _assetVersioningDictionary[bundle];

            if(_assetVersioningDictionary.ContainsKey(bundle))
            {
                res.Add(bundle);
            }
            else
            {
                Debug.LogError("Cannot find parent bundle [" + bundle + "], Please check your bundle config.");
                break;
            }
        }
        
        res.Reverse();
        return res;
    }

    void initRootUrl(string baseUrl)
    {
        _downloadRootUrl = baseUrl + SpamPort;
    }

    string formatUrl(string urlstr)
    {
        Uri url;
        if(!isAbsoluteUrl(urlstr))
        {
            string bundleName = Path.GetFileNameWithoutExtension(urlstr);
            int version = _assetVersioningDictionary[bundleName].Version;
            string client = _assetVersioningDictionary[bundleName].Client;
            if(IsLocalBundleVersion(bundleName, version, client))
            {
                url = new Uri(Application.streamingAssetsPath + "/" + SpamPort + "/" + urlstr);
            }
            else
            {
                string projectVersionUrl = SpamData.ClientToPath(client);
                string uriString = _downloadRootUrl + "/" + projectVersionUrl + "/" + version + "/" + urlstr;
                url = new Uri(uriString);
            }
        }
        else
        {
            url = new Uri(urlstr);
        }

        return url.AbsoluteUri;
    }

    public bool IsLocalBundleVersion(string bundleName, int version, string client)
    {
        bool bundleFound = _buildStatesLocalDict.ContainsKey(bundleName) && _assetVersioningDictionary.ContainsKey(bundleName);
        if(bundleFound)
        {
            var assetVersioningData = _buildStatesLocalDict[bundleName];

            if(version == assetVersioningData.Version && client == assetVersioningData.Client)
            {
                return true;
            }
        }
        return false;

    }

    static bool isAbsoluteUrl(string url)
    {
        Uri result;
        return Uri.TryCreate(url, UriKind.Absolute, out result);
    }

    static bool isBundleUrl(string url)
    {
        return string.Compare(Path.GetExtension(url), "." + _bundleSuffix, StringComparison.OrdinalIgnoreCase) == 0;
    }

    public bool IsCachedOrLocal(string bundleName)
    {
        int version;

        List<string> res = getDependList(bundleName);
        res.Add(bundleName);
        for(int i = 0, resCount = res.Count; i < resCount; i++)
        {
            string dependency = res[i];
            version = _assetVersioningDictionary[dependency].Version;
            if(!IsLocalBundleVersion(bundleName, version, _assetVersioningDictionary[bundleName].Client) && !Caching.IsVersionCached(bundleName + "." + _bundleSuffix, version))
            {
                return false;
            }
        }
        return true;
    }

    class WWWRequest
    {
        public string url = "";
        public string bundleName = "";
        public string urlNoPath = "";
        public int triedTimes;
        public int priority;
        //public BundleData bundleData = null;
        //public BundleBuildState bundleBuildState = null;
        public AssetVersioningData assetVersioningData;
        public WWW www;
        public Action<string> callback;

        public void CreatWWW()
        {   
            triedTimes++;
            
            if(_useCache && assetVersioningData != null)
            {
#if !(UNITY_4_2 || UNITY_4_1 || UNITY_4_0)
                if(_useCrc)
                {
                    www = WWW.LoadFromCacheOrDownload(url, assetVersioningData.Version, assetVersioningData.CRC);
                }
                else 
#endif
                {
                    www = WWW.LoadFromCacheOrDownload(url, assetVersioningData.Version);
                }
            }
            else
            {
                www = new WWW(url);
            }
        }
    }

    public class SpamData
    {
        [NonSerialized]
        static SpamData
            _instance;

        public static SpamData Instance
        {
            get
            {
                if(_instance == null)
                {
                    if(Application.isEditor)
                    {
                        PathsManager.Init();
                    }
                    var filePath = Path.Combine(PathsManager.StreamingAssetsPath, "json/spamData.json");
                    var content = FileUtils.ReadAllText(filePath);
                    
                    _instance = LitJson.JsonMapper.ToObject<SpamData>(content);
                }
                return _instance;
            }
        }

        public string client;
        public string project;

        public static string ClientToPath(string client)
        {
            return "v" + client.Replace(".", "_").Replace(",", "_");
        }
    }
}

