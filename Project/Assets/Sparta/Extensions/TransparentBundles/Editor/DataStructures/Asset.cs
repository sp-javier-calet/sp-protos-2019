using System.IO;
using UnityEditor;
using UnityEngine;

namespace SocialPoint.TransparentBundles
{
    public class Asset
    {
        public string Name = "";
        public string FullName = "";
        public string Type;
        public string Guid = "";
        Object _assetObject;

        public Asset(string guid)
        {
            Guid = guid;
            Name = LoadAssetName();
            FullName = LoadAssetFullName();
            Type = LoadAssetType();
        }

        public Asset(string guid, string name)
        {
            Guid = guid;
            Name = name;
            FullName = LoadAssetFullName();
            Type = LoadAssetType();
        }

        public Object GetAssetObject()
        {
            if(_assetObject == null && Guid.Length > 0)
            {
                _assetObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(Guid));
            }

            return _assetObject;
        }

        public string LoadAssetType()
        {
            Object asset = GetAssetObject();
            return asset != null ? asset.GetType().ToString() : "";
        }

        public string LoadAssetName()
        {
            Name = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(Guid));
            return Name;
        }

        public string LoadAssetFullName()
        {
            FullName = Path.GetFileName(AssetDatabase.GUIDToAssetPath(Guid));
            return FullName;
        }
    }
}
