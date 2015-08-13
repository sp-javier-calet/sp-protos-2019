using UnityEngine;
using SocialPoint.Utils;
using SocialPoint.IosKeychain;

namespace SocialPoint.Attributes
{
    class KeychainAttrStorage : IAttrStorage
    {
        public IAttrSerializer Serializer = new JsonAttrSerializer();
        public IAttrParser Parser = new JsonAttrParser();
        public string Prefix = string.Empty;
        public string AccessGroup = string.Empty;
        public string Service = string.Empty;

        public KeychainAttrStorage(string prefix=null, string accessGroup=null, string service=null)
        {
            Prefix = prefix;
            AccessGroup = accessGroup;
            Service = service;
        }

        public Attr Load(string key)
        {
            if(Prefix != null)
            {
                key = Prefix + key;
            }
            var str = new KeychainItem(key, AccessGroup, Service).Value;
            return Parser.Parse(new Data(str));
        }

        public bool Has(string key)
        {
            if(Prefix != null)
            {
                key = Prefix + key;
            }
            return new KeychainItem(key, AccessGroup, Service).Value != null;
        }

        public void Save(string key, Attr attr)
        {
            if(Prefix != null)
            {
                key = Prefix + key;
            }
            Data data = Serializer.Serialize(attr);
            new KeychainItem(key, AccessGroup, Service).Value = data.String;
        }

        public void Remove(string key)
        {
            if(Prefix != null)
            {
                key = Prefix + key;
            }
            new KeychainItem(key, AccessGroup, Service).Clear();
        }
    }
}
