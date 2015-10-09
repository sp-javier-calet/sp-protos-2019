using UnityEngine;

namespace SocialPoint.Attributes
{
    public class PlayerPrefsAttrStorage : IAttrStorage
    {
        public IAttrSerializer Serializer = new JsonAttrSerializer();
        public IAttrParser Parser = new JsonAttrParser();
        public string Prefix = string.Empty;

        public PlayerPrefsAttrStorage()
        {
        }

        public Attr Load(string key)
        {
            if(Prefix != null)
            {
                key = Prefix + key;
            }
            var str = PlayerPrefs.GetString(key, null);
            if(str == null)
            {
                return null;
            }
            return Parser.ParseString(str);
        }

        public bool Has(string key)
        {
            if(Prefix != null)
            {
                key = Prefix + key;
            }
            return PlayerPrefs.HasKey(key);
        }

        public void Save(string key, Attr attr)
        {
            var data = Serializer.SerializeString(attr);
            if(Prefix != null)
            {
                key = Prefix + key;
            }
            PlayerPrefs.SetString(key, data);
            PlayerPrefs.Save();
        }

        public void Remove(string key)
        {
            if(Prefix != null)
            {
                key = Prefix + key;
            }
            PlayerPrefs.DeleteKey(key);
        }
    }
}
