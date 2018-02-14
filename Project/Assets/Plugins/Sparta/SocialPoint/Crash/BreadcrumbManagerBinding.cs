#if (UNITY_ANDROID || UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
#define UNITY_DEVICE
#endif
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_DEVICE
#define NATIVE_BREADCRUMBS
#endif

#if NATIVE_BREADCRUMBS
using System.Runtime.InteropServices;
#endif

namespace SocialPoint.Crash
{
    public sealed class BreadcrumbManagerBinding
    {
        #if NATIVE_BREADCRUMBS

        #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        const string PluginModuleName = "SPUnityPlugins";
        #elif UNITY_ANDROID
        const string PluginModuleName = "sp_unity_crash";
        #elif (UNITY_IOS || UNITY_TVOS)
        const string PluginModuleName = "__Internal";
        #endif

        /* Native plugin interface */

        [DllImport(PluginModuleName)]
        static extern void SPUnityBreadcrumbManager_SetMaxLogs(int maxLogs);

        [DllImport(PluginModuleName)]
        static extern void SPUnityBreadcrumbManager_SetDumpFilePath(string directory, string file);

        [DllImport(PluginModuleName)]
        static extern void SPUnityBreadcrumbManager_Log(string info);

        [DllImport(PluginModuleName)]
        static extern void SPUnityBreadcrumbManager_DumpToFile();

        [DllImport(PluginModuleName)]
        static extern void SPUnityBreadcrumbManager_Clear();

        #endif

        /* Game interface */

        public static void SetMaxLogs(int maxLogs)
        {
            #if NATIVE_BREADCRUMBS
            SPUnityBreadcrumbManager_SetMaxLogs(maxLogs);
            #endif
        }

        public static void SetDumpFilePath(string directory, string file)
        {
            #if NATIVE_BREADCRUMBS
            SPUnityBreadcrumbManager_SetDumpFilePath(directory, file);
            #endif
        }

        public static void Log(string info)
        {
            #if NATIVE_BREADCRUMBS
            SPUnityBreadcrumbManager_Log(info);
            #endif
        }

        public static void DumpToFile()
        {
            #if NATIVE_BREADCRUMBS
            SPUnityBreadcrumbManager_DumpToFile();
            #endif
        }

        public static void Clear(int maxLogs)
        {
            #if NATIVE_BREADCRUMBS
            SPUnityBreadcrumbManager_Clear();
            #endif
        }
    }
}