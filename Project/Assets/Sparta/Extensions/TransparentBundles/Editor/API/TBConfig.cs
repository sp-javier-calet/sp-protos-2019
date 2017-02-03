using System.IO;
using UnityEditor;
using UnityEngine;

namespace SocialPoint.TransparentBundles
{
    public class TBConfig : ScriptableObject
    {
        public string project = null;
        private const string _configDefaultPath = "Assets/Sparta/Config/TransparentBundles/TBConfig.asset";

        /// <summary>
        /// Tries to find the TBConfig file in the project, if it doesn't exists it will be created.
        /// </summary>
        /// <returns>TBConfig scriptable object reference</returns>
        public static TBConfig GetConfig()
        {
            string project = string.Empty;
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


        /// <summary>
        /// Gets the project set in the TBConfig File.
        /// </summary>
        /// <returns>string with the project configured in the TBConfig</returns>
        public static string GetProject()
        {
            string project = string.Empty;
            var file = AssetDatabase.FindAssets("t:TBConfig");
            TBConfig config;

            if(file.Length != 1)
            {
                throw new System.Exception("No single TBConfig found, try logging in manually.");
            }
            else
            {
                config = AssetDatabase.LoadAssetAtPath<TBConfig>(AssetDatabase.GUIDToAssetPath(file[0]));
            }

            return config.project;
        }

    }
}
