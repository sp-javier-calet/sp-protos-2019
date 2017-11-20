using System;
using System.Collections;
using SocialPoint.Base;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SocialPoint.Utils
{
    public struct AssetBundleDef
    {
        public string Url;
        public int Version;
        public string Name;
        public bool All;
    }

    public sealed class UnityUpdateRunner : MonoBehaviour, ICoroutineRunner, IUpdateScheduler
    {
        const float kMaxUnscaledDelta = 0.5f;

        readonly UpdateScheduler _scheduler = new UpdateScheduler();

        public event Action<Exception> OnExceptionInUpdate;

        void Awake()
        {
            _scheduler.OnExceptionInUpdate += ExceptionsInUpdateCallback;
        }

        public void Add(IUpdateable elm, UpdateableTimeMode updateTimeMode = UpdateableTimeMode.GameTimeUnscaled, float interval = -1)
        {
            _scheduler.Add(elm, updateTimeMode, interval);
        }

        public void Add(IDeltaUpdateable elm, UpdateableTimeMode updateTimeMode = UpdateableTimeMode.GameTimeUnscaled, float interval = -1)
        {
            _scheduler.Add(elm, updateTimeMode, interval);
        }

        public void Remove(IUpdateable elm)
        {
            _scheduler.Remove(elm);
        }

        public void Remove(IDeltaUpdateable elm)
        {
            _scheduler.Remove(elm);
        }

        public bool Contains(IUpdateable elm)
        {
            return _scheduler.Contains(elm);
        }

        public bool Contains(IDeltaUpdateable elm)
        {
            return _scheduler.Contains(elm);
        }

        IEnumerator ICoroutineRunner.StartCoroutine(IEnumerator enumerator)
        {
            if(enumerator != null)
            {
                StartCoroutine(enumerator);
            }
            return enumerator;
        }

        void ICoroutineRunner.StopCoroutine(IEnumerator enumerator)
        {
            if(enumerator != null)
            {
                StopCoroutine(enumerator);
            }
        }

        void Update()
        {
            /* NOTE: Unity's Time.unscaledDeltaTime is counting time while in background (BUG?), 
             * but we want an unscaled game-time-only for UnscaledDeltaTime, 
             * so we ignore big deltas in Time.unscaledDeltaTime.
             * */
            float unscaledDeltaTime = (Time.unscaledDeltaTime > kMaxUnscaledDelta) ? kMaxUnscaledDelta : Time.unscaledDeltaTime;
            _scheduler.Update(Time.deltaTime, unscaledDeltaTime);
        }

        void ExceptionsInUpdateCallback(Exception e)
        {
            if(OnExceptionInUpdate != null)
            {
                OnExceptionInUpdate(e);
            }
        }
    }

    public static class UnityCoroutineRunnerExtensions
    {
        public static IEnumerator DownloadTexture(this ICoroutineRunner runner, string url, Action<Texture2D, Error> cbk)
        {
            var itr = DownloadTextureCoroutine(url, cbk);
            runner.StartCoroutine(itr);
            return itr;
        }

        static IEnumerator DownloadTextureCoroutine(string url, Action<Texture2D, Error> cbk)
        {
            var www = new WWW(url);
            yield return www;
            if(cbk != null)
            {
                if(!string.IsNullOrEmpty(www.error))
                {
                    cbk(null, new Error(www.error));
                }
                else if(www.texture == null)
                {
                    cbk(null, new Error("Could not load texture."));
                }
                else
                {
                    cbk(www.texture, null);
                }
            }
            www.Dispose();
        }

        public static IEnumerator DownloadBundle<T>(this ICoroutineRunner runner, AssetBundleDef def, Action<T[], Error> cbk) where T : UnityEngine.Object
        {
            var itr = DownloadBundleCoroutine(def, cbk);
            runner.StartCoroutine(itr);
            return itr;
        }

        static IEnumerator DownloadBundleCoroutine<T>(AssetBundleDef def, Action<T[], Error> cbk) where T : UnityEngine.Object
        {
            while(!Caching.ready)
            {
                yield return null;
            }

            using(var www = WWW.LoadFromCacheOrDownload(def.Url, def.Version))
            {
                yield return www;
                if(!string.IsNullOrEmpty(www.error))
                {
                    if(cbk != null)
                    {
                        cbk(null, new Error(www.error));
                    }
                    yield break;
                }
                if(cbk == null)
                {
                    yield break;
                }
                var bundle = www.assetBundle;
                www.Dispose();
                AssetBundleRequest req;
                if(string.IsNullOrEmpty(def.Name))
                {
                    req = bundle.LoadAllAssetsAsync();
                }
                else if(def.All)
                {
                    req = bundle.LoadAssetWithSubAssetsAsync<T>(def.Name);
                }
                else
                {
                    req = bundle.LoadAssetAsync<T>(def.Name);
                }
                yield return req;
                bundle.Unload(false);
                if(def.All)
                {
                    var elms = new T[req.allAssets.Length];
                    int i = 0;
                    for(int j = 0, reqallAssetsLength = req.allAssets.Length; j < reqallAssetsLength; j++)
                    {
                        var asset = req.allAssets[j];
                        elms[i++] = asset as T;
                    }
                    cbk(elms, null);
                }
                else
                {
                    cbk(new []{ req.asset as T }, null);
                }
            }
        }

        public static AsyncOperation LoadSceneAsync(this ICoroutineRunner runner, string name, Action<AsyncOperation> finished)
        {
            return LoadSceneAsync(runner, name, LoadSceneMode.Single, finished);
        }

        public static AsyncOperation LoadSceneAsync(this ICoroutineRunner runner, int index, Action<AsyncOperation> finished)
        {
            return LoadSceneAsync(runner, index, LoadSceneMode.Single, finished);
        }

        public static AsyncOperation LoadSceneAsync(this ICoroutineRunner runner, string name, LoadSceneMode mode, Action<AsyncOperation> finished)
        {
            return LoadSceneAsync(runner, name, false, mode, finished);
        }

        public static AsyncOperation LoadSceneAsync(this ICoroutineRunner runner, int index, LoadSceneMode mode, Action<AsyncOperation> finished)
        {
            return LoadSceneAsync(runner, index, false, mode, finished);
        }

        public static AsyncOperation LoadSceneAsyncProgress(this ICoroutineRunner runner, int index, Action<AsyncOperation> finished)
        {
            return LoadSceneAsync(runner, index, LoadSceneMode.Single, finished);
        }

        public static AsyncOperation LoadSceneAsyncProgress(this ICoroutineRunner runner, string name, Action<AsyncOperation> finished)
        {
            return LoadSceneAsync(runner, name, LoadSceneMode.Single, finished);
        }

        public static AsyncOperation LoadSceneAsyncProgress(this ICoroutineRunner runner, int index, LoadSceneMode mode, Action<AsyncOperation> finished)
        {
            return LoadSceneAsync(runner, index, true, mode, finished);
        }

        public static AsyncOperation LoadSceneAsyncProgress(this ICoroutineRunner runner, string name, LoadSceneMode mode, Action<AsyncOperation> finished)
        {
            return LoadSceneAsync(runner, name, true, mode, finished);
        }

        static AsyncOperation LoadSceneAsync(this ICoroutineRunner runner, string name, bool progress, LoadSceneMode mode, Action<AsyncOperation> finished)
        {
            var op = SceneManager.LoadSceneAsync(name, mode);
            runner.StartCoroutine(CheckAsyncOp(op, progress, finished));
            return op;
        }

        static AsyncOperation LoadSceneAsync(this ICoroutineRunner runner, int index, bool progress, LoadSceneMode mode, Action<AsyncOperation> finished)
        {
            var op = SceneManager.LoadSceneAsync(index, mode);
            runner.StartCoroutine(CheckAsyncOp(op, progress, finished));
            return op;
        }

        static IEnumerator CheckAsyncOp(AsyncOperation op, bool progress, Action<AsyncOperation> finished)
        {
            while(!op.isDone)
            {
                if(progress && finished != null)
                {
                    finished(op);
                }
                yield return null;
            }
            if(finished != null)
            {
                finished(op);
            }
        }
    }
}
