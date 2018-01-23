using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace SocialPoint.TransparentBundles
{
    public class Downloader
    {

        static Downloader _instance;
        static Dictionary<string, Texture2D> _downloadCache;
        static bool _downloadingBundle;
        readonly List<UnityWebRequest> _requests = new List<UnityWebRequest>();
        const float _downloadTimeout = 10f;

        Downloader()
        {
            _downloadCache = new Dictionary<string, Texture2D>();
            _downloadingBundle = false;
            EditorApplication.update += Update;
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

            var loadedTexture = new Texture2D(0, 0);
            loadedTexture.LoadImage(File.ReadAllBytes(path));
            _downloadCache.Add(path, loadedTexture);

            return loadedTexture;
        }

        public void FlushImagesCache()
        {
            _downloadCache = new Dictionary<string, Texture2D>();
        }

        void Update()
        {
            if(_requests.Count == 0)
            {
                return;
            }

            if(_requests.Any(x => !x.isDone))
            {
                float progress = 0;
                _requests.ForEach(x => progress += x.downloadProgress);
                EditorUtility.DisplayProgressBar("Download", "Downloading Bundles", progress / _requests.Count);
                return;
            }
            EditorUtility.ClearProgressBar();

            for(int i = 0; i < _requests.Count; i++)
            {
                if(i < _requests.Count - 1)
                {
                    ((DownloadHandlerAssetBundle)_requests[i].downloadHandler).assetBundle.LoadAllAssets();
                }
                else
                {
                    InstantiateBundle(((DownloadHandlerAssetBundle)_requests[i].downloadHandler).assetBundle);
                }
            }
        }

        void InstantiateBundle(AssetBundle bundle)
        {
            var objects = bundle.LoadAllAssets();
            var assetType = objects[0].GetType().ToString();

            switch(assetType)
            {
                case "UnityEngine.GameObject":

                    GameObject instanceGO = Object.Instantiate((GameObject)objects[0]);
                    instanceGO.name = "[BUNDLE] " + objects[0].name;
                    Component[] renderers = instanceGO.GetComponentsInChildren(typeof(Renderer));
                    foreach(Component component in renderers)
                    {
                        var renderer = (Renderer)component;
                        foreach(Material material in renderer.sharedMaterials)
                        {
                            if(material != null && material.shader != null)
                            {
                                material.shader = Shader.Find(material.shader.name);
                            }
                        }
                    }
                    break;

                case "UnityEngine.Texture2D":

                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "[BUNDLE] " + objects[0].name;
                    Renderer rend = cube.GetComponent<Renderer>();
                    var mat = new Material(Shader.Find("Standard"));
                    rend.material = mat;
                    mat.mainTexture = (Texture2D)objects[0];
                    break;
            }

            _requests.ForEach(x => ((DownloadHandlerAssetBundle)x.downloadHandler).assetBundle.Unload(false));
            _downloadingBundle = false;
            _requests.Clear();
        }

        public void DownloadBundle(Bundle bundle, BundlePlaform platform)
        {
            if(!_downloadingBundle)
            {
                _downloadingBundle = true;

                DownloadBundleRecursive(bundle, platform);
            }
        }

        public void DownloadBundleRecursive(Bundle bundle, BundlePlaform platform)
        {
            for(int i = 0; i < bundle.Parents.Count; i++)
            {
                Bundle parent = bundle.Parents[i];
                if(parent.Url[platform].Length > 0)
                {
                    DownloadBundleRecursive(parent, platform);
                }
                else
                {
                    ErrorDisplay.DisplayError(ErrorType.bundleNotDownloadable, true, false, false, parent.Name);
                }
            }

            if(_requests.All(x => x.url != bundle.Url[platform]))
            {
                if(bundle.Url[platform].Length > 0)
                {
                    var request = UnityWebRequest.GetAssetBundle(bundle.Url[platform]);
                    request.SendWebRequest();
                    _requests.Add(request);
                }
                else
                {
                    ErrorDisplay.DisplayError(ErrorType.bundleNotDownloadable, true, false, false, bundle.Name);
                }
            }
        }
    }
}
