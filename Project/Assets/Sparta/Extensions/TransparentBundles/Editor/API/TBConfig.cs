using System.IO;
using SocialPoint.Base;
using UnityEditor;
using UnityEngine;

namespace SocialPoint.TransparentBundles
{
    [System.Serializable]
    public class TBConfig : ScriptableObject
    {
        public string project = "";
        public string branchName = "";
        static readonly string _configDefaultPath = ConfigPaths.SpartaConfigEditorPath + "TransparentBundles/TBConfig.asset";
        const string _configSearchPattern = "t:TBConfig";

        /// <summary>
        /// Tries to find the TBConfig file in the project, if it doesn't exists it will be created.
        /// </summary>
        /// <returns>TBConfig scriptable object reference</returns>
        public static TBConfig GetConfig()
        {
            var file = AssetDatabase.FindAssets(_configSearchPattern);
            TBConfig config;

            if(file.Length == 0)
            {
                var directory = Directory.GetParent(_configDefaultPath);
                if(!directory.Exists)
                {
                    directory.Create();
                }

                config = ScriptableObject.CreateInstance<TBConfig>();

                AssetDatabase.CreateAsset(config, _configDefaultPath);
            }
            else if(file.Length > 1)
            {
                throw new System.Exception("More than one config file found, please have only one.");
            }
            else
            {
                config = AssetDatabase.LoadAssetAtPath<TBConfig>(AssetDatabase.GUIDToAssetPath(file[0]));
            }

            return config;
        }
    }
}
