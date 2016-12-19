using UnityEngine;
using UnityEditor;
using System.IO;

namespace SocialPoint.TransparentBundles
{
    public class Asset
    {
        public string Name = "";
        public string Type;
        public string Guid = "";
        private Object _assetObject;

        public Asset(string guid)
        {
            Guid = guid;
            Name = LoadAssetName();
            Type = LoadAssetType();
        }
        public Asset(string guid, string name)
        {
            Guid = guid;
            Name = name;
            Type = LoadAssetType();
        }
        public Asset(string guid, string name, string type )
        {
            Name = name;
            Type = type;
            Guid = guid;
        }
        public Asset(string guid, string name, string type, int id)
        {
            Name = name;
            Type = type;
            Guid = guid;
        }

        public Object GetAssetObject()
        {
            if (_assetObject == null && Guid.Length > 0)
                _assetObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(Guid));

            return _assetObject;
        }

        public string LoadAssetType()
        {
            return GetAssetObject().GetType().ToString();
        }

        public string LoadAssetName()
        {
            Name = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(Guid));
            return Name;
        }
    }
}