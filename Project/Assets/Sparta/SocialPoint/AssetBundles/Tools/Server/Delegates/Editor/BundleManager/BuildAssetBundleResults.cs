using System.Collections.Generic;


namespace SocialPoint.Tool.Server
{
    public sealed class BuildAssetBundleResults : ToolServiceResults
    {
        /**
         * Base properties
         */
        public Dictionary<string, BuiltBundleResult> builtBundles;
    }

    public sealed class BuiltBundleResult
    {
        public string   name;
        public string[] includedAssets;
        public string   outPath;
        public string   errorMessage;
        public string   crc;
        public string   size;
        public string   parent;
        public string[] serializationErrors;
        public bool     isSerializationError;
        public bool     isSceneBundle;
        public bool     isSuccess;
        public bool     isSkipped;
        public bool     neededBuild;

        BuiltBundleResult() : base()
        {
        }

        public static BuiltBundleResult CreateFromBuiltBundle(BuiltBundle builtBundle)
        {
            var result = new BuiltBundleResult();
            result.name = builtBundle.bundleName;
            result.includedAssets = builtBundle.includs.ToArray();
            result.outPath = builtBundle.fullPath;
            result.isSceneBundle = builtBundle.isScene;
            result.isSkipped = builtBundle.isSkipped;
            result.isSuccess = builtBundle.isSuccess;
            result.neededBuild = builtBundle.neededBuild;

            if(!builtBundle.isSuccess)
            {
                result.isSerializationError = builtBundle.isSerializationError;
                result.serializationErrors = builtBundle.serializationErrors.ToArray();
                result.errorMessage = builtBundle.errorMessage;
            }

            if(builtBundle.crc > 0 && builtBundle.size > 0)
            {
                result.parent = builtBundle.parent != string.Empty ? builtBundle.parent : null;
                result.crc = builtBundle.crc.ToString();
                result.size = builtBundle.size.ToString();
            }

            return result;
        }
    }
}
