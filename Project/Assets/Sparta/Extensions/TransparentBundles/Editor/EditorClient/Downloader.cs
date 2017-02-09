using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SocialPoint.TransparentBundles
{
    //Dummy window to fix the WWWs never ending in Editor.
    public class DownloadWindow : EditorWindow { }
    public class Downloader
    {

        private static Downloader _instance;
        private static Dictionary<string, Texture2D> _downloadCache;
        private static bool _downloadingBundle = false;
        private List<WWW> _requests = new List<WWW>();
        private WWW _mainRequest;
        private EditorWindow _sender;
        private List<AssetBundle> _bundlesCache;
        private const float _downloadTimeout = 10f;

        private Downloader()
        {
            _downloadCache = new Dictionary<string, Texture2D>();
            _downloadingBundle = false;
            _bundlesCache = new List<AssetBundle>();
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

            Texture2D loadedTexture = new Texture2D(0, 0);
            loadedTexture.LoadImage(File.ReadAllBytes(path));
            _downloadCache.Add(path, loadedTexture);

            return loadedTexture;
        }

        void Update()
        {
            if(_requests.Count == 0)
            {
                return;
            }

            if(_requests.Any(x => !x.isDone))
            {
                if(_requests.TrueForAll(x => x.progress == 1))
                {
                    EditorUtility.ClearProgressBar();
                    var win = EditorWindow.GetWindow<DownloadWindow>();
                    win.minSize = new Vector2(1, 1);
                    win.position = new Rect(0, 0, 1, 1);
                    win.Close();
                }
                else
                {
                    float progress = 0;
                    _requests.ForEach(x => progress += x.progress);
                    EditorUtility.DisplayProgressBar("Download", "Downloading Bundles", progress / _requests.Count);
                }
                return;
            }
            EditorUtility.ClearProgressBar();

            for(int i = 0; i < _requests.Count; i++)
            {
                if(i < _requests.Count - 1)
                {
                    _requests[i].assetBundle.LoadAllAssets();
                }
                else
                {
                    InstantiateBundle(_requests[i].assetBundle);  
                }
            }
        }

        private void InstantiateBundle(AssetBundle bundle)
        {
            //TODO: handle different types of bundles.
            var objects = bundle.LoadAllAssets();
            GameObject instanceGO = GameObject.Instantiate((GameObject)objects[0]);
            instanceGO.name = "[BUNDLE] " + objects[0].name;
            Component[] renderers = instanceGO.GetComponentsInChildren(typeof(Renderer));
            foreach(Component component in renderers)
            {
                Renderer renderer = (Renderer)component;
                foreach(Material material in renderer.sharedMaterials)
                {
                    if(material.shader != null)
                    {
                        material.shader = Shader.Find(material.shader.name);
                    }
                }
            }

            _requests.ForEach(x => x.assetBundle.Unload(false));
            _downloadingBundle = false;
            _requests.Clear();
        }

        public void DownloadBundle(Bundle bundle)
        {
            if (!_downloadingBundle)
            {
                _downloadingBundle = true;
                if (!_requests.Any(x=>x.url == bundle.Url))
                {
                    for (int i = 0; i < bundle.Parents.Count; i++)
                    {
                        Bundle parent = bundle.Parents[i];
                        if (!_requests.Any(x => x.url == parent.Url))
                        {
                            if (parent.Url.Length > 0 && parent.Asset.Name.Length > 0)
                            {
                                _requests.Add(new WWW(parent.Url));
                            }
                            else
                            {
                                Debug.LogError("Transparent Bundles - Error - The bundle '" + parent.Name + "' doesn't have a proper URL or Name assigned. Please, contact the transparent bundles team: " + Config.ContactUrl);
                            }
                        }
                    }

                    if (bundle.Url.Length > 0 && bundle.Asset.Name.Length > 0)
                    {
                        _mainRequest = new WWW(bundle.Url);
                        _requests.Add(_mainRequest);
                    }
                    else
                    {
                        Debug.LogError("Transparent Bundles - Error - The bundle '" + bundle.Name + "' doesn't have a proper URL or Name assigned. Please, contact the transparent bundles team: " + Config.ContactUrl);
                    }
                }

            }
        }        
    }
}
