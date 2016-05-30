using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SocialPoint.Crash
{
    public class BreadcrumbManagerBinding
    {
        #if UNITY_ANDROID
        const string PluginModuleName = "sp_unity_crash_reporter";
        #else
        const string PluginModuleName = "__Internal";
        #endif

        /* Native plugin interface */

        [DllImport(PluginModuleName)]
        private static extern UIntPtr SPUnityBreadcrumbManager_Get();

        [DllImport(PluginModuleName)]
        private static extern void SPUnityBreadcrumbManager_SetMaxLogs(int maxLogs);

        [DllImport(PluginModuleName)]
        private static extern void SPUnityBreadcrumbManager_SetDumpFilePath(string directory, string file);

        [DllImport(PluginModuleName)]
        private static extern void SPUnityBreadcrumbManager_Log(string info);

        [DllImport(PluginModuleName)]
        private static extern void SPUnityBreadcrumbManager_DumpToFile();

        [DllImport(PluginModuleName)]
        private static extern void SPUnityBreadcrumbManager_Clear();


        /* Game interface */

        public static UIntPtr GetNativeObjectPointer()
        {
            return SPUnityBreadcrumbManager_Get();
        }

        public static void SetMaxLogs(int maxLogs)
        {
            SPUnityBreadcrumbManager_SetMaxLogs(maxLogs);
        }

        public static void SetDumpFilePath(string directory, string file)
        {
            SPUnityBreadcrumbManager_SetDumpFilePath(directory, file);
        }

        public static void Log(string info)
        {
            SPUnityBreadcrumbManager_Log(info);
        }

        public static void DumpToFile()
        {
            SPUnityBreadcrumbManager_DumpToFile();
        }

        public static void Clear(int maxLogs)
        {
            SPUnityBreadcrumbManager_Clear();
        }
    }
}