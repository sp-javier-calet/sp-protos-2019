using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SocialPoint.AssetBundlesClient
{
    public static class Utility
    {
        public static string GetPlatformName()
        {
#if UNITY_EDITOR
            return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
            return GetPlatformForAssetBundles(Application.platform);
#endif
        }

        #if UNITY_EDITOR
        static string GetPlatformForAssetBundles(BuildTarget target)
        {
            switch(target)
            {
            case BuildTarget.Android:
                return "android_etc";
            case BuildTarget.iOS:
                return "ios";
            default:
                return "ios";
            }
        }
        #endif

        static string GetPlatformForAssetBundles(RuntimePlatform platform)
        {
            switch(platform)
            {
            case RuntimePlatform.Android:
                return "android_etc";
            case RuntimePlatform.IPhonePlayer:
                return "ios";
            default:
                return "ios";
            }
        }
    }
}
