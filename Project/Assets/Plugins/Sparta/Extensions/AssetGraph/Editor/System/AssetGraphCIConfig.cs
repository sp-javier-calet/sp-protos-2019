using System.IO;
using SocialPoint.Base;
using UnityEditor;
using UnityEngine;

namespace AssetBundleGraph
{
    [System.Serializable]
    public class AssetGraphCIConfig : ScriptableObject
    {
        public string ProjectID = "";
        public static readonly string ConfigDefaultPath = ConfigPaths.SpartaConfigEditorPath + "AssetGraph/AssetGraphCIConfig.asset";
        const string _configSearchPattern = "t:AssetGraphCIConfig";

        /// <summary>
        /// Tries to find the AssetGraphCIConfig file in the project, if it doesn't exists it will be created.
        /// </summary>
        /// <returns>AssetGraphCIConfig scriptable object reference</returns>
        public static AssetGraphCIConfig GetConfig()
        {
            var file = AssetDatabase.FindAssets(_configSearchPattern);
            AssetGraphCIConfig config;

            if(file.Length == 0)
            {
                var directory = Directory.GetParent(ConfigDefaultPath);
                if(!directory.Exists)
                {
                    directory.Create();
                }

                config = ScriptableObject.CreateInstance<AssetGraphCIConfig>();

                AssetDatabase.CreateAsset(config, ConfigDefaultPath);
            }
            else if(file.Length > 1)
            {
                throw new System.Exception("More than one config file found, please have only one.");
            }
            else
            {
                config = AssetDatabase.LoadAssetAtPath<AssetGraphCIConfig>(AssetDatabase.GUIDToAssetPath(file[0]));
            }

            return config;
        }
    }
}
