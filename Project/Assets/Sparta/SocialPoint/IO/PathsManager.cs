using UnityEngine;
using System;
using System.IO;

namespace SocialPoint.IO
{
    
    public class PathsManager
    {
        private static string _dataPath;
        private static string _appPersistentDataPath;
        private static string _streamingAssetsPath;
        private static string _temporaryDataPath;
        private static Action _loaded;

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

        [Obsolete("Use AppPersistentDataPath instead")]
        public static string PersistentDataPath
        {
            get
            {
                return AppPersistentDataPath;
            }
        }

        public static string AppPersistentDataPath
        {
            get
            {
                if(_appPersistentDataPath == null)
                {
                    throw new Exception("Uninitialized PathsManager");
                }
                return _appPersistentDataPath;
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

        [Obsolete("Use TemporaryDataPath instead")]
        public static string TemporaryCachePath
        {
            get
            {
                return TemporaryDataPath;
            }
        }

        public static string TemporaryDataPath
        {
            get
            {
                if(_temporaryDataPath == null)
                {
                    throw new Exception("Uninitialized PathsManager");
                }
                return _temporaryDataPath;
            }
        }

        public static string Combine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public static void Init()
        {
            // Bundle
            _dataPath = Application.dataPath;

            // Persisten Data Path
            #if UNITY_TVOS
            _appPersistentDataPath = Application.temporaryCachePath;
            #else
            _appPersistentDataPath = Application.persistentDataPath;
            #endif

            // Streaming Assets
            _streamingAssetsPath = Application.streamingAssetsPath;

            // Temporary Data Path
            #if UNITY_ANDROID
            _temporaryDataPath = Application.persistentDataPath;
            #else
            _temporaryDataPath = Application.temporaryCachePath;
            #endif

            if(_loaded != null)
            {
                _loaded();
            }
            _loaded = null;
        }

        public static void CallOnLoaded(Action action)
        {
            if(_dataPath != null)
            {
                action();
            }
            else
            {
                _loaded += action;
            }
        }
    }  
}
