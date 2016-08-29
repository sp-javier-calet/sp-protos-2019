using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SocialPoint.Crash
{
    public sealed class BreadcrumbManagerBinding
    {
        #if UNITY_ANDROID
        const string PluginModuleName = "sp_unity_crash_reporter";
        #else
        const string PluginModuleName = "__Internal";
        #endif

        /* Native plugin interface */

        #if !UNITY_EDITOR
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
        #endif


        /* Game interface */

        public static void SetMaxLogs(int maxLogs)
        {
            #if !UNITY_EDITOR
            SPUnityBreadcrumbManager_SetMaxLogs(maxLogs);
            #endif
        }

        public static void SetDumpFilePath(string directory, string file)
        {
            #if !UNITY_EDITOR
            SPUnityBreadcrumbManager_SetDumpFilePath(directory, file);
            #endif
        }

        public static void Log(string info)
        {
            #if !UNITY_EDITOR
            SPUnityBreadcrumbManager_Log(info);
            #endif
        }

        public static void DumpToFile()
        {
            #if !UNITY_EDITOR
            SPUnityBreadcrumbManager_DumpToFile();
            #endif
        }

        public static void Clear(int maxLogs)
        {
            #if !UNITY_EDITOR
            SPUnityBreadcrumbManager_Clear();
            #endif
        }
    }
}