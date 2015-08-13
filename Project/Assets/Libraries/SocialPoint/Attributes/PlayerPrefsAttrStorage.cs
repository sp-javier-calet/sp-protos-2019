using UnityEngine;
using SocialPoint.Utils;

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
            string str = PlayerPrefs.GetString(key);
            return Parser.Parse(new Data(str));
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
            Data data = Serializer.Serialize(attr);
            if(Prefix != null)
            {
                key = Prefix + key;
            }
            PlayerPrefs.SetString(key, data.String);
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
