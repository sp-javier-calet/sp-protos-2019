
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.Base;

namespace SocialPoint.Utils
{
    public struct AssetBundleDef
    {
        public string Url;
        public int Version;
        public string Name;
        public bool All;
    }

    public interface IUnityDownloader
    {
        void DownloadTexture(string url, Action<Texture2D, Error> cbk);
        void DownloadBundle<T>(AssetBundleDef def, Action<T[], Error> cbk) where T : UnityEngine.Object;
    }

    public class UnityUpdateRunner : MonoBehaviour, ICoroutineRunner, IUpdateScheduler, IUnityDownloader
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
            StartCoroutine(enumerator);
        }

        void ICoroutineRunner.StopCoroutine(IEnumerator enumerator)
        {
            StopCoroutine(enumerator);
        }

        public void Update()
        {
            foreach(var elm in _elements)
            {
                if(elm != null)
                {
                    elm.Update();
                }
            }
        }

        public void DownloadTexture(string url, Action<Texture2D, Error> cbk)
        {
            StartCoroutine(DownloadTextureCoroutine(url, cbk));
        }

        IEnumerator DownloadTextureCoroutine(string url, Action<Texture2D, Error> cbk)
        {
            var www = new WWW(url);
            yield return www;
            if(cbk != null)
            {
                cbk(www.texture, new Error(www.error));
            }
        }

        public void DownloadBundle<T>(AssetBundleDef def, Action<T[], Error> cbk) where T : UnityEngine.Object
        {
            StartCoroutine(DownloadBundleCoroutine(def, cbk));
        }

        IEnumerator DownloadBundleCoroutine<T>(AssetBundleDef def, Action<T[], Error> cbk) where T : UnityEngine.Object
        {
            while(!Caching.ready)
            {
                yield return null;
            }

            using(var www = WWW.LoadFromCacheOrDownload(def.Url, def.Version))
            {
                yield return www;
                if(www.error != null)
                {
                    if(cbk != null)
                    {
                        cbk(null, new Error(www.error));
                    }
                    yield break;
                }
                var bundle = www.assetBundle;
                if(cbk == null)
                {
                    yield break;
                }
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
    }
}