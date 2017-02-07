using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SocialPoint.TransparentBundles
{
    public class Downloader
    {
        private static Downloader _instance;
        private static Dictionary<string, Texture2D> _downloadCache;
        private static bool _downloadingBundle = false;
        private static Dictionary<Bundle, IEnumerator> _bundleQueue;
        private static WWW _request;
        private List<AssetBundle> _bundlesCache;
        private const float _downloadTimeout = 10f;

        private Downloader()
        {
            _downloadCache = new Dictionary<string, Texture2D>();
            _downloadingBundle = false;
            _bundleQueue = new Dictionary<Bundle, IEnumerator>();
            _bundlesCache = new List<AssetBundle>();
        }

        public static Downloader GetInstance()
        {
            if(_instance == null)
            {
                _instance = new Downloader();
            }

            return _instance;
        }


        public Texture2D DownloadImage(string path)
        {
            if(_downloadCache.ContainsKey(path))
            {
                return _downloadCache[path];
            }

            Texture2D loadedTexture = new Texture2D(0, 0);
            loadedTexture.LoadImage(File.ReadAllBytes(path));
            _downloadCache.Add(path, loadedTexture);

            return loadedTexture;
        }

        public void DownloadBundle(Bundle bundle)
        {
            if (!_downloadingBundle)
            {
                _downloadingBundle = true;
                if (!_bundleQueue.ContainsKey(bundle))
                {
                    for (int i = 0; i < bundle.Parents.Count; i++)
                    {
                        Bundle parent = bundle.Parents[i];
                        if (!_bundleQueue.ContainsKey(parent))
                        {
                            if (parent.Url.Length > 0 && parent.Asset.Name.Length > 0)
                            {
                                _bundleQueue.Add(parent, DownloadOnlyBundle(parent.Url, parent.Asset.Name));
                            }
                            else
                            {
                                Debug.LogError("Transparent Bundles - Error - The bundle '" + parent.Name + "' doesn't have a proper URL or Name assigned. Please, contact the transparent bundles team: " + Config.ContactUrl);
                            }
                        }
                    }

                    if (bundle.Url.Length > 0 && bundle.Asset.Name.Length > 0)
                    {
                        _bundleQueue.Add(bundle, DownloadAndInstantiateBundle(bundle.Url, bundle.Asset.Name, bundle.Asset.Type));
                    }
                    else
                    {
                        Debug.LogError("Transparent Bundles - Error - The bundle '" + bundle.Name + "' doesn't have a proper URL or Name assigned. Please, contact the transparent bundles team: " + Config.ContactUrl);
                    }
                }
                DownloadWindow.OpenWindow();
            }
        }

        public int InstantiateDownloadedBundles()
        {
            for(int i = 0; i < _bundleQueue.Keys.Count; i++)
            {
                var enumerator = _bundleQueue.Keys.GetEnumerator();
                for (int j = 0; j < i+1 && enumerator.MoveNext(); j++) { }
                Bundle key = enumerator.Current;

                if (!_bundleQueue[key].MoveNext())
                {
                    _bundleQueue.Remove(key);
                }
                enumerator.Dispose();
            }

            return _bundleQueue.Keys.Count;
        }

        private IEnumerator DownloadAndInstantiateBundle(string url, string assetName, string assetType)
        {
            _request = new WWW(url);
            float timeout = Time.realtimeSinceStartup + _downloadTimeout;
            while (!_request.isDone && Time.realtimeSinceStartup < timeout)
            {
                DownloadWindow.Window.position = new Rect(DownloadWindow.Window.position.x, DownloadWindow.Window.position.y-30, DownloadWindow.Window.position.width, DownloadWindow.Window.position.height);
                yield return "";
            }
            if(Time.realtimeSinceStartup >= timeout)
            {
                _downloadingBundle = false;
                _bundleQueue = new Dictionary<Bundle, IEnumerator>();
                _bundlesCache = new List<AssetBundle>();
                Debug.LogError("Transparent Bundle - Error - Timeout. Not able to download the bundle in the following url '"+url+ "'. Please, contact the transparent bundles team: " + Config.ContactUrl);
            }
            else
            {
                if (_request.error != null)
                {
                    Debug.LogError(_request.error);
                }

                else if (_bundleQueue.Count == 1)
                {
                    AssetBundle bundle = _request.assetBundle;
                    switch (assetType)
                    {
                        case "UnityEngine.GameObject":

                            GameObject GO = (GameObject)bundle.LoadAsset(assetName) as GameObject;
                            GameObject instanceGO = GameObject.Instantiate(GO);
                            instanceGO.name = "[BUNDLE] " + instanceGO.name.Substring(0, instanceGO.name.LastIndexOf("(Clone)"));
                            Component[] renderers = instanceGO.GetComponentsInChildren(typeof(Renderer));
                            foreach (Component component in renderers)
                            {
                                Renderer renderer = (Renderer)component;
                                foreach (Material material in renderer.sharedMaterials)
                                {
                                    if (material.shader != null)
                                    {
                                        material.shader = Shader.Find(material.shader.name);
                                    }
                                }
                            }
                            break;

                        case "UnityEngine.Texture2D":

                            Texture2D texture = (Texture2D)bundle.LoadAsset(assetName) as Texture2D;
                            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            cube.name = "[BUNDLE] " + assetName;
                            Renderer rend = cube.GetComponent<Renderer>();
                            Material mat = new Material(Shader.Find("Standard"));
                            rend.material = mat;
                            mat.mainTexture = texture;
                            break;
                    }


                    if (_request.assetBundle != null)
                    {
                        _request.assetBundle.Unload(false);
                    }

                    for (int i = 0; i < _bundlesCache.Count; i++)
                    {
                        _bundlesCache[i].Unload(false);
                    }
                    _bundlesCache = new List<AssetBundle>();
                    SceneView.RepaintAll();
                    _downloadingBundle = false;
                }
            }
        }

        private IEnumerator DownloadOnlyBundle(string url, string assetName)
        {
            WWW request = new WWW(url);
            float timeout = Time.realtimeSinceStartup + _downloadTimeout;
            while (!request.isDone && Time.realtimeSinceStartup < timeout)
            {
                yield return "";
            }
            if (Time.realtimeSinceStartup >= timeout)
            {
                _downloadingBundle = false;
                _bundleQueue = new Dictionary<Bundle, IEnumerator>();
                _bundlesCache = new List<AssetBundle>();
                Debug.LogError("Transparent Bundle - Error - Timeout. Not able to download the bundle in the following url '" + url + "'. Please, contact the transparent bundles team: " + Config.ContactUrl);
            }
            else
            {
                if (request.error != null)
                {
                    Debug.LogError(request.error);
                }
                else
                {
                    request.assetBundle.LoadAsset(assetName);
                    _bundlesCache.Add(request.assetBundle);
                }
            }
        }
    }
}