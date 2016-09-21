using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibraryDownloader
    {

        private static GrayboxLibraryDownloader instance;
        private static Dictionary<string, Texture2D> downloadCache;


        private GrayboxLibraryDownloader()
        {
            downloadCache = new Dictionary<string, Texture2D>();
        }

        public static GrayboxLibraryDownloader GetInstance()
        {
            if (instance == null)
                instance = new GrayboxLibraryDownloader();

            return instance;
        }


        public Texture2D DownloadImage(string path)
        {
            if (downloadCache.ContainsKey(path))
                return downloadCache[path];

            Texture2D loadedTexture = new Texture2D(0, 0);
            loadedTexture.LoadImage(File.ReadAllBytes(path));
            downloadCache.Add(path, loadedTexture);

            return loadedTexture;
        }


        public void ImportPackage(string path)
        {
            AssetDatabase.ImportPackage(path, false);
        }
    }
}