using UnityEngine;
using System.Collections;

namespace SocialPoint.Tool.Server
{
    public class BuildAssetBundleParameters : ToolServiceParameters
    {
        public string[] bundlesToBuild;
        public string buildTarget;
        public string textureCompressionFormat; //can be ommited if buildTarget is not 'android'
		public bool forceRebuild = false;

        public string bmConfiger;   //if omited will use default file
        public string bundleData;   //if omited will use default file
        public string buildStates;  //if omited will use default file
        public string urls;         //if omited will use default file
    }
}
