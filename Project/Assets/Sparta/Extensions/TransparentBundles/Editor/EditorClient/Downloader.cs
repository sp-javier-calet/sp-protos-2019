using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace SocialPoint.TransparentBundles
{
    public class Downloader
    {

        private static Downloader _instance;
        private static Dictionary<string, Texture2D> _downloadCache;


        private Downloader()
        {
            _downloadCache = new Dictionary<string, Texture2D>();
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
    }
}