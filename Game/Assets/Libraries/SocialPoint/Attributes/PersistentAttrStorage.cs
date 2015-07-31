﻿using System;
using System.IO;
using UnityEngine;
using SocialPoint.Utils;
using SocialPoint.IO;

namespace SocialPoint.Attributes
{
    public class PersistentAttrStorage : IAttrStorage
    {
        private IAttrSerializer Serializer;
        private IAttrParser Parser;
        private string Root = string.Empty;

        public PersistentAttrStorage()
            : this(new JsonAttrParser(), new JsonAttrSerializer(), PathsManager.PersistentDataPath)
        {
        }

        public PersistentAttrStorage(string root)
            : this(new JsonAttrParser(), new JsonAttrSerializer(), root)
        {            
        }

        public PersistentAttrStorage(IAttrParser parser, IAttrSerializer serializer, string root)
        {
            Parser = parser;
            Serializer = serializer;
            Root = root;
        }

        private string GetPath(string key)
        {
            return string.Format("{0}/{1}", Root, key);
        }

        #region IAttrStorage implementation

        public Attr Load(string key)
        {
            string str;
            if(!FileUtils.Exists(GetPath(key)))
            {
                return null;
            }
            using(StreamReader file = new StreamReader(GetPath(key)))
            {
                str = file.ReadToEnd();
            }
            return Parser.Parse(new Data(str));
        }

        public void Save(string key, Attr attr)
        {
            Data data = Serializer.Serialize(attr);
            if(FileUtils.Exists(Root) == false)
            {
                FileUtils.CreateDirectory(Root);
            }
            using(StreamWriter file = new StreamWriter(GetPath(key)))
            {
                file.Write(data);
            }
        }

        public bool Has(string key)
        {
            return FileUtils.Exists(GetPath(key));
        }

        public void Remove(string key)
        {
            FileUtils.Delete(GetPath(key));
        }

        public string[] StoredKeys
        {
            get
            {
                string[] keys = new string[0];
                if(Directory.Exists(Root) == false)
                {
                    //TODO: launch exception
                    return keys;
                }
                DirectoryInfo info = new DirectoryInfo(Root);
                var files = info.GetFiles();
                keys = new string[files.Length];
                if(files.Length > 0)
                {
                    for(int i = 0; i < files.Length; i++)
                    {
                        keys[i] = files[i].Name;
                    }
                }
                return keys;
            }
        }

        #endregion

    }
}

