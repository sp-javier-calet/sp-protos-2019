using System;
using System.Collections;
using SocialPoint.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace SocialPoint.AssetBundlesClient
{
    public abstract class AssetBundleLoadOperation : IEnumerator
    {
        public object Current
        {
            get
            {
                return null;
            }
        }

        public bool MoveNext()
        {
            return !IsDone();
        }

        public void Reset()
        {
        }

        public abstract bool Update();

        public abstract bool IsDone();

        public string Error { get; protected set; }
    }

    public abstract class AssetBundleDownloadOperation : AssetBundleLoadOperation
    {
        bool _done;

        public string AssetBundleName { get; private set; }

        public LoadedAssetBundle AssetBundleLoaded { get; protected set; }

        protected abstract bool downloadIsDone { get; }

        protected abstract void FinishDownload();

        public override bool Update()
        {
            if(!_done && downloadIsDone)
            {
                FinishDownload();
                _done = true;
            }
            return !_done;
        }

        public override bool IsDone()
        {
            return _done;
        }

        public abstract string GetSourceURL();

        protected AssetBundleDownloadOperation(string assetBundleName)
        {
            AssetBundleName = assetBundleName;
        }
    }

    public class AssetBundleLoadLocalOperation : AssetBundleDownloadOperation
    {
        readonly string _fullPath;
        AssetBundleCreateRequest _request;

        public AssetBundleLoadLocalOperation(string assetBundleName, string fullPath)
            : base(assetBundleName)
        {
            _fullPath = fullPath;

            // This CAN'T be called for Adroid because it freezes the main thread in Samsung Galaxy S8 device with Unity 5.5.4p4 or less. Further investigation needed
            #if !UNITY_ANDROID
            if(!FileUtils.ExistsFile(_fullPath))
            {
                Error = string.Format("{0} file does not exists locally. FullPath: {1}", AssetBundleName, _fullPath);
                return;
            }
            #endif

            _request = AssetBundle.LoadFromFileAsync(_fullPath);
        }

        protected override bool downloadIsDone { get { return (_request == null) || _request.isDone; } }

        protected override void FinishDownload()
        {
            if(_request == null)
            {
                return;
            }

            var bundle = _request.assetBundle;
            if(bundle == null)
            {
                Error = string.Format("{0} is not a valid asset bundle. FullPath: {1}", AssetBundleName, _fullPath);
                return;
            }

            AssetBundleLoaded = new LoadedAssetBundle(bundle);
        }

        public override string GetSourceURL()
        {
            return _fullPath;
        }
    }

    public class AssetBundleDownloadFromUnityWebRequestOperation : AssetBundleDownloadOperation
    {
        readonly string _Url;
        UnityWebRequest _request;

        public AssetBundleDownloadFromUnityWebRequestOperation(string assetBundleName, int assetBundleVersion, string url)
            : base(assetBundleName)
        {
            _Url = url;

            _request = UnityWebRequest.GetAssetBundle(url, (uint)assetBundleVersion, 0);
            if(_request != null)
            {
                _request.Send();
            }
        }

        protected override bool downloadIsDone { get { return (_request == null) || _request.isDone; } }

        protected override void FinishDownload()
        {
            if(_request == null)
            {
                _request.Dispose();
                return;
            }

            Error = _request.error;
            if(!string.IsNullOrEmpty(Error))
            {
                _request.Dispose();
                _request = null;
                return;
            }

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(_request);
            if(bundle == null)
            {
                Error = string.Format("{0} is not a valid asset bundle.", AssetBundleName);
            }
            else
            {
                AssetBundleLoaded = new LoadedAssetBundle(bundle);
            }

            _request.Dispose();
            _request = null;
        }

        public override string GetSourceURL()
        {
            return _Url;
        }
    }

    public class AssetBundleLoadLevelOperation : AssetBundleLoadOperation
    {
        public enum LoadSceneBundleMode
        {
            OnlyDownload,
            Single,
            Additive
        }

        const string LoadingErrorDescription = "LoadSceneAsync loading failed.";

        protected string _assetBundleName;
        protected string _levelName;
        protected LoadSceneBundleMode _loadSceneMode;
        protected string _downloadingError;
        protected string _loadingError;
        protected AsyncOperation _request;

        public AssetBundleLoadLevelOperation(string assetbundleName, string levelName, LoadSceneBundleMode loadSceneMode)
        {
            _assetBundleName = assetbundleName;
            _levelName = levelName;
            _loadSceneMode = loadSceneMode;
        }

        public override bool Update()
        {
            if(_request != null)
            {
                return false;
            }

            LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle(_assetBundleName, out _downloadingError);
            if(!string.IsNullOrEmpty(_downloadingError))
            {
                Error = _downloadingError;
                return false;
            }
            if(bundle != null)
            {
                switch(_loadSceneMode)
                {
                case LoadSceneBundleMode.Additive:
                    _request = SceneManager.LoadSceneAsync(_levelName, LoadSceneMode.Additive);
                    break;
                case LoadSceneBundleMode.Single:
                    _request = SceneManager.LoadSceneAsync(_levelName, LoadSceneMode.Single);
                    break;
                }
                if(_request == null)
                {
                    _loadingError = LoadingErrorDescription;
                    Error = _loadingError;
                }
                return false;
            }
            return true;
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // _downloadingError might come from the dependency downloading.
            if(_request == null && Error != null)
            {
                return true;
            }

            return _request != null && _request.isDone;
        }
    }

    public abstract class AssetBundleLoadAssetOperation : AssetBundleLoadOperation
    {
        public abstract T GetAsset<T>() where T: UnityEngine.Object;
    }

    public class AssetBundleLoadAssetOperationFull : AssetBundleLoadAssetOperation
    {
        const string LoadingErrorDescription = "LoadAssetAsync failed.";
        const string GetAssetTypeErrorDescription = "Asset from request is not from the expected type.";
        const string GetAssetRequestrDescription = "Can not retrieve asset from AssetBundleRequest.";

        protected string _assetBundleName;
        protected string _assetName;
        protected string _downloadingError;
        protected string _loadingError;
        protected Type _type;
        protected AssetBundleRequest _request;

        public AssetBundleLoadAssetOperationFull(string bundleName, string assetName, Type type)
        {
            _assetBundleName = bundleName;
            _assetName = assetName;
            _type = type;
        }

        public override T GetAsset<T>()
        {
            if(_request != null && _request.isDone)
            {
                var asset = _request.asset as T;
                if(asset != null)
                {
                    return asset;
                }
                if(string.IsNullOrEmpty(Error) && _request.asset != null)
                {
                    Error = GetAssetTypeErrorDescription;
                }
            }
            if(string.IsNullOrEmpty(Error))
            {
                Error = GetAssetRequestrDescription;
            }
            return null;
        }

        // Returns true if more Update calls are required.
        public override bool Update()
        {
            if(_request != null)
            {
                return false;
            }

            LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle(_assetBundleName, out _downloadingError);
            if(!string.IsNullOrEmpty(_downloadingError))
            {
                Error = _downloadingError;
                return false;
            }
            if(bundle != null)
            {
                _request = bundle._assetBundle.LoadAssetAsync(_assetName, _type);
                if(_request == null)
                {
                    _loadingError = LoadingErrorDescription;
                    Error = _loadingError;
                }
                return false;
            }
            return true;
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // _downloadingError might come from the dependency downloading.
            if(_request == null && Error != null)
            {
                return true;
            }

            return _request != null && _request.isDone;
        }
    }
}
