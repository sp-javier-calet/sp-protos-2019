using System.Collections;
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
    }

    public abstract class AssetBundleDownloadOperation : AssetBundleLoadOperation
    {
        bool _done;

        public string AssetBundleName { get; private set; }

        public LoadedAssetBundle AssetBundle { get; protected set; }

        public string Error { get; protected set; }

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

    public class AssetBundleDownloadFromWebOperation : AssetBundleDownloadOperation
    {
        WWW _WWW;
        readonly string _Url;

        public AssetBundleDownloadFromWebOperation(string assetBundleName, WWW www)
            : base(assetBundleName)
        {
            if(www == null)
            {
                throw new System.ArgumentNullException("www");
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
                AssetBundle = new LoadedAssetBundle(_WWW.assetBundle);
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
        protected string _assetBundleName;
        protected string _levelName;
        protected bool _isAdditive;
        protected string _downloadingError;
        protected AsyncOperation _request;

        public AssetBundleLoadLevelOperation(string assetbundleName, string levelName, bool isAdditive)
        {
            _assetBundleName = assetbundleName;
            _levelName = levelName;
            _isAdditive = isAdditive;
        }

        public override bool Update()
        {
            if(_request != null)
            {
                return false;
            }

            LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle(_assetBundleName, out _downloadingError);
            if(bundle != null)
            {
                _request = SceneManager.LoadSceneAsync(_levelName, _isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
                return false;
            }
            return true;
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // _downloadingError might come from the dependency downloading.
            if(_request == null && _downloadingError != null)
            {
                Debug.LogError(_downloadingError);
                return true;
            }

            return _request != null && _request.isDone;
        }
    }

    public abstract class AssetBundleLoadAssetOperation : AssetBundleLoadOperation
    {
        public abstract T GetAsset<T>() where T: Object;
    }

    public class AssetBundleLoadAssetOperationFull : AssetBundleLoadAssetOperation
    {
        protected string _assetBundleName;
        protected string _assetName;
        protected string _downloadingError;
        protected System.Type _type;
        protected AssetBundleRequest _request;

        public AssetBundleLoadAssetOperationFull(string bundleName, string assetName, System.Type type)
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
            if(bundle != null)
            {
                ///@TODO: When asset bundle download fails this throws an exception...
                _request = bundle._assetBundle.LoadAssetAsync(_assetName, _type);
                return false;
            }
            return true;
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // _downloadingError might come from the dependency downloading.
            if(_request == null && _downloadingError != null)
            {
                Debug.LogError(_downloadingError);
                return true;
            }
            return _request != null && _request.isDone;
        }
    }
}
