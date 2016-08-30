using System;

namespace BundleManagerJSON
{
    public sealed class JSONDataConfig
    {
        public bool             exportWithBundle = false;
        public string[]         excludedComponents = null;
        public bool             serializeNonCustomGameObjects = false;
        public bool             usePrefabCopies = true;
    }

    public sealed class JSONConfig
    {
        /**
        * Export json with bundle
        */ 
        public static bool ExportWithBundle
        {
            get{ return JSONDataAccessor.JSONDataConfig.exportWithBundle;}
            set
            {
                if(JSONDataAccessor.JSONDataConfig.exportWithBundle != value)
                {
                    JSONDataAccessor.JSONDataConfig.exportWithBundle = value;
                    JSONDataAccessor.SaveJSONDataConfig();
                }
            }
        }

        /**
        * Excluded Components (no serializable)
        */ 
        public static string[] ExcludedComponents
        {
            get{ return JSONDataAccessor.JSONDataConfig.excludedComponents;}
            set
            {
                JSONDataAccessor.JSONDataConfig.excludedComponents = value;
                JSONDataAccessor.SaveJSONDataConfig();
            }
        }

        /**
         * Serialize GameObjects that do not contain custom components 
         */
        public static bool SerializeNonCustomGameObjects
        {
            get{ return JSONDataAccessor.JSONDataConfig.serializeNonCustomGameObjects;}
            set
            {
                if(JSONDataAccessor.JSONDataConfig.serializeNonCustomGameObjects != value)
                {
                    JSONDataAccessor.JSONDataConfig.serializeNonCustomGameObjects = value;
                    JSONDataAccessor.SaveJSONDataConfig();
                }
            }
        }

        /**
         * When serializing assetBundles, use temporal asset copies of added prefabs 
         * (this prevents modifications on the target prefab when removing and adding components)
         */
        public static bool UsePrefabCopies
        {
            get{ return JSONDataAccessor.JSONDataConfig.usePrefabCopies;}
            set
            {
                if(JSONDataAccessor.JSONDataConfig.usePrefabCopies != value)
                {
                    JSONDataAccessor.JSONDataConfig.usePrefabCopies = value;
                    JSONDataAccessor.SaveJSONDataConfig();
                }
            }
        }
        public static string usePrefabCopies_tt = "When serializing assetBundles, use temporal asset copies of added prefabs." +
            "This prevents modifications on the target prefab when removing and adding components but can cause missing link problems " +
            "if prefabs are referenced from another serialized game object components.";
    }
}
