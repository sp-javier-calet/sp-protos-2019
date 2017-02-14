using System.IO;
using UnityEditor;
using UnityEngine;

namespace SocialPoint.TransparentBundles
{
    [System.Serializable]
    public class TBConfig : ScriptableObject
    {
        public string project = "";
        private const string _configDefaultPath = "Assets/Sparta/Config/TransparentBundles/TBConfig.asset";

        /// <summary>
        /// Tries to find the TBConfig file in the project, if it doesn't exists it will be created.
        /// </summary>
        /// <returns>TBConfig scriptable object reference</returns>
        public static TBConfig GetConfig()
        {
            var file = AssetDatabase.FindAssets("t:TBConfig");
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


        public static bool IsValid()
        {
            var file = AssetDatabase.FindAssets("t:TBConfig");
            TBConfig config = null;

            if(file.Length == 1)
            {
                config = AssetDatabase.LoadAssetAtPath<TBConfig>(AssetDatabase.GUIDToAssetPath(file[0]));
            }
            else if(file.Length > 1)
            {
                throw new System.Exception("More than one TBConfig found, remove one.");
            }

            return file.Length == 1 && !string.IsNullOrEmpty(config.project);
        }

        /// <summary>
        /// Gets the project set in the TBConfig File.
        /// </summary>
        /// <returns>string with the project configured in the TBConfig</returns>
        public static string GetProject()
        {
            var file = AssetDatabase.FindAssets("t:TBConfig");
            TBConfig config = AssetDatabase.LoadAssetAtPath<TBConfig>(AssetDatabase.GUIDToAssetPath(file[0]));            

            return config.project;
        }

    }
}
