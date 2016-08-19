using System;
using System.IO;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Attributes
{
    public sealed class FileAttrStorage : IAttrStorage
    {
        // Sync status between FileAttrStorage instances
        static Dictionary<string, uint> SyncStatus = new Dictionary<string, uint>();

        IAttrSerializer _serializer;
        IAttrParser _parser;
        string _root = string.Empty;
        string[] _storedKeys;
        uint _syncId = 0;

        public FileAttrStorage()
            : this(new JsonAttrParser(), new JsonAttrSerializer(), PathsManager.AppPersistentDataPath)
        {
        }

        public FileAttrStorage(string root)
            : this(new JsonAttrParser(), new JsonAttrSerializer(), root)
        {            
        }

        public FileAttrStorage(IAttrParser parser, IAttrSerializer serializer, string root)
        {
            _parser = parser;
            _serializer = serializer;
            _root = root;
        }

        string GetPath(string key)
        {
            return string.Format("{0}/{1}", _root, key);
        }

        bool IsSynced()
        {
            uint syncValue;
            SyncStatus.TryGetValue(_root, out syncValue);
            return _syncId == syncValue && _storedKeys != null;
        }

        void Invalidate()
        {
            uint sync;
            SyncStatus.TryGetValue(_root, out sync);
            SyncStatus[_root] = sync + 1;
        }

        void Sync()
        {
            SyncStatus.TryGetValue(_root, out _syncId);
        }

        #region IAttrStorage implementation

        public Attr Load(string key)
        {
            var path = GetPath(key);
            if(!FileUtils.ExistsFile(path))
            {
                return null;
            }
            var data = FileUtils.ReadAllBytes(path);
            return _parser.Parse(data);
        }

        public void Save(string key, Attr attr)
        {
            var data = _serializer.Serialize(attr);
            var path = GetPath(key);
            var dir = Path.GetDirectoryName(path);
            FileUtils.CreateDirectory(dir);
            FileUtils.WriteAllBytes(path, data);
            Invalidate();
        }

        public bool Has(string key)
        {
            return FileUtils.ExistsFile(GetPath(key));
        }

        public void Remove(string key)
        {
            FileUtils.DeleteFile(GetPath(key));
            Invalidate();
        }

        public string[] StoredKeys
        {
            get
            {
                if(!FileUtils.ExistsDirectory(_root))
                {
                    //TODO: launch exception
                    _storedKeys = new string[0];
                    return _storedKeys;
                }

                if(IsSynced())
                {
                    return _storedKeys;
                }

                // Sync status and read keys.
                Sync();
                var info = new DirectoryInfo(_root);
                var files = info.GetFiles();
                if(_storedKeys == null || _storedKeys.Length != files.Length)
                {
                    _storedKeys = new string[files.Length];
                }

                for(int i = 0; i < files.Length; i++)
                {
                    _storedKeys[i] = files[i].Name;
                }
                return _storedKeys;
            }
        }

        #endregion
        
    }
}
