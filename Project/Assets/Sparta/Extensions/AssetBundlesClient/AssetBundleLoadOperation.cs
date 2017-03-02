using System;
using System.Collections;
using SocialPoint.IO;
using UnityEngine;
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
        readonly string _Url;

        public AssetBundleLoadLocalOperation(string assetBundleName, string path)
            : base(assetBundleName)
        {
            _Url = path;
        }

        protected override void FinishDownload()
        {
            if(!FileUtils.ExistsFile(_Url))
            {
                Error = string.Format("{0} file does not exists locally.", AssetBundleName);
                return;
            }

            var bundle = AssetBundle.LoadFromFile(_Url);

            if(bundle == null)
            {
                Error = string.Format("{0} is not a valid asset bundle.", AssetBundleName);
                return;
            }

            AssetBundleLoaded = new LoadedAssetBundle(bundle);
        }

        public override string GetSourceURL()
        {
            return _Url;
        }

        protected override bool downloadIsDone
        {
            get
            {
                return true;
                ;
            }
        }
    }

    public class AssetBundleDownloadFromWebOperation : AssetBundleDownloadOperation
    {
        WWW _WWW;
        readonly string _Url;

        public AssetBundleDownloadFromWebOperation(string assetBundleName, WWW www)
            : base(assetBundleName)
        {
            if(www == null)
            {
                throw new ArgumentNullException("www");
            }
            _Url = www.url;
            _WWW = www;
        }

        protected override bool downloadIsDone { get { return (_WWW == null) || _WWW.isDone; } }

        protected override void FinishDownload()
        {
            Error = _WWW.error;
            if(!string.IsNullOrEmpty(Error))
            {
                return;
            }

            AssetBundle bundle = _WWW.assetBundle;
            if(bundle == null)
            {
                Error = string.Format("{0} is not a valid asset bundle.", AssetBundleName);
            }
            else
            {
                AssetBundleLoaded = new LoadedAssetBundle(bundle);
            }

            _WWW.Dispose();
            _WWW = null;
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
        const string LoadingErrorDescription = "LoadAssetAsync loading failed.";

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
                return _request.asset as T;
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
