using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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

    public class FixedUpdateableData
    {
        public readonly double Interval;
        public double CurrentTime;

        public FixedUpdateableData(double interval)
        {
            Interval = interval;
            CurrentTime = 0.0;
        }
    }

    public class IUpdateableComparer : IEqualityComparer<IUpdateable>
    {
        public bool Equals(IUpdateable x, IUpdateable y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(IUpdateable obj)
        {
            return obj.GetType().GetHashCode();
        }
    }

    public class UnityUpdateRunner : MonoBehaviour, ICoroutineRunner, IUpdateScheduler, IFixedUpdateScheduler
    {
        readonly HashSet<IUpdateable> _elements;
        readonly Dictionary<IUpdateable, FixedUpdateableData> _fixedElements;
        readonly List<Exception> _exceptions = new List<Exception>();

        public UnityUpdateRunner()
        {
            var comparer = new IUpdateableComparer();
            _elements = new HashSet<IUpdateable>(comparer);
            _fixedElements = new Dictionary<IUpdateable, FixedUpdateableData>(comparer);
        }

        public void Add(IUpdateable elm)
        {
            if(elm == null)
            {
                throw new ArgumentException("elm cannot be null");
            }
            _elements.Add(elm);
        }

        public void Remove(IUpdateable elm)
        {
            _elements.Remove(elm);
        }

        public void AddFixed(IUpdateable elm, double interval)
        {
            if(elm == null)
            {
                throw new ArgumentException("elm cannot be null");
            }
            var fixedUpdateable = new FixedUpdateableData(interval);
            _fixedElements.Add(elm, fixedUpdateable);
        }

        public void RemoveFixed(IUpdateable elm)
        {
            _fixedElements.Remove(elm);
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
            _exceptions.Clear();

            var itr = _elements.GetEnumerator();
            while(itr.MoveNext())
            {
                var elm = itr.Current;
                try
                {
                    elm.Update();
                }
                catch(Exception e)
                {
                    _exceptions.Add(e);
                }
            }
            itr.Dispose();

            var deltaTime = Time.deltaTime;
            var itr2 = _fixedElements.GetEnumerator();
            while(itr2.MoveNext())
            {
                itr2.Current.Value.CurrentTime += deltaTime;

                var data = itr2.Current.Value;
                var interval = data.Interval;
                var currentTime = data.CurrentTime;
                if(currentTime >= interval)
                {
                    var elm = itr2.Current.Key;
                    try
                    {
                        elm.Update();
                    }
                    catch(Exception e)
                    {
                        _exceptions.Add(e);
                    }
                    itr2.Current.Value.CurrentTime = currentTime - interval;
                }
            }
            itr2.Dispose();

            var exceptionsCount = _exceptions.Count;
            if(exceptionsCount > 0)
            {
                var sb = new StringBuilder();
                for(int i = 0; i < exceptionsCount; i++)
                {
                    var ex = _exceptions[i];
                    sb.Append(ex.Message);
                }
                throw new Exception(sb.ToString());
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