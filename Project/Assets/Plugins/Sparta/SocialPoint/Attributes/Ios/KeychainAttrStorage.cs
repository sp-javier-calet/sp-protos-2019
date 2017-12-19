using SocialPoint.IosKeychain;

namespace SocialPoint.Attributes
{
    public class KeychainAttrStorage : IAttrStorage
    {
        public IAttrSerializer Serializer = new JsonAttrSerializer();
        public IAttrParser Parser = new JsonAttrParser();
        public string Prefix = string.Empty;
        public string AccessGroup = string.Empty;
        public string Service = string.Empty;

        public KeychainAttrStorage(string prefix = null, string accessGroup = null, string service = null)
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
            return str == null ? null : Parser.ParseString(str);
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
            var data = Serializer.SerializeString(attr);
            new KeychainItem(key, AccessGroup, Service).Value = data;
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
