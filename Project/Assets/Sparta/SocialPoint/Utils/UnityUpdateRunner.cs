
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.Base;
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

    public class UnityUpdateRunner : MonoBehaviour, ICoroutineRunner, IUpdateScheduler
    {
        HashSet<IUpdateable> _elements = new HashSet<IUpdateable>();

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

        void ICoroutineRunner.StartCoroutine(IEnumerator enumerator)
        {
            if(enumerator != null)
            {
                StartCoroutine(enumerator);
            }
        }

        void ICoroutineRunner.StopCoroutine(IEnumerator enumerator)
        {
            if(enumerator != null)
            {
                StopCoroutine(enumerator);
            }
        }

        public void Update()
        {
            foreach(var elm in _elements)
            {
                elm.Update();
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
                AssetBundleRequest req = null;
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
                    foreach(var asset in req.allAssets)
                    {
                        elms[i++] = asset as T;
                    }
                    cbk(elms, null);
                }
                else
                {
                    cbk(new T[]{ req.asset as T }, null);
                }
            }
        }

        public static void LoadSceneAsync(this ICoroutineRunner runner, string sceneName, Action<AsyncOperation> finished, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            runner.StartCoroutine(DoLoadSceneAsync(sceneName, finished, loadSceneMode));
        }

        static IEnumerator DoLoadSceneAsync(string sceneName, Action<AsyncOperation> finished, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            while(!op.isDone)
            {
                if(finished != null)
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