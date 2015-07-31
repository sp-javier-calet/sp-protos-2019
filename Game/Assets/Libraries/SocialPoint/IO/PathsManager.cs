using UnityEngine;
using System;
using System.Collections;
using System.IO;

namespace SocialPoint.IO
{
    
    public class PathsManager
    {
        private static string _dataPath;
        private static string _persistentDataPath;
        private static string _streamingAssetsPath;
        private static string _temporaryCachePath;

        public static string DataPath
        {
            get
            {
                if(_dataPath == null)
                {
                    throw new Exception("Uninitialized PathsManager");
                }
                return _dataPath;
            }
        }

        public static string PersistentDataPath
        {
            get
            {
                if(_persistentDataPath == null)
                {
                    throw new Exception("Uninitialized PathsManager");
                }
                return _persistentDataPath;
            }
        }

        public static string StreamingAssetsPath
        {
            get
            {
                if(_streamingAssetsPath == null)
                {
                    throw new Exception("Uninitialized PathsManager");
                }
                return _streamingAssetsPath;
            }
        }

        public static string TemporaryCachePath
        {
            get
            {
                if(_temporaryCachePath == null)
                {
                    throw new Exception("Uninitialized PathsManager");
                }
                return _temporaryCachePath;
            }
        }

        public static string Combine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public static void Init(MonoBehaviour behaviour, bool inMainThread = true)
        {
            SocialPoint.Base.Debug.Assert(behaviour != null);

            if(!inMainThread)
            {
                behaviour.StartCoroutine(CollectPaths());
            }
            else
            {
                CollectPaths();
            }
        }

        private static IEnumerator CollectPaths()
        {
            _dataPath = Application.dataPath;
            _persistentDataPath = Application.persistentDataPath;
            _streamingAssetsPath = Application.streamingAssetsPath;
            _temporaryCachePath = Application.temporaryCachePath;
            return null;
        }
    }
    
}
