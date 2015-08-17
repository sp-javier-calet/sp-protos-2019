using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

using SocialPoint;
using SocialPoint.Network;
using SocialPoint.Base;

namespace SocialPoint.Network
{
    public class CurlBridge
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RequestStruct
        {
            [MarshalAs(UnmanagedType.SysInt)]
            public IntPtr
                Id;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string
                Url;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string
                Query;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string
                Method;
            [MarshalAs(UnmanagedType.SysInt)]
            public IntPtr
                Timeout;
            [MarshalAs(UnmanagedType.SysInt)]
            public IntPtr
                ActivityTimeout;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string
                Proxy;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string
                Headers;
            public byte[] Body;
            [MarshalAs(UnmanagedType.SysInt)]
            public IntPtr
                BodyLength;
        };

        #if UNITY_IOS && !UNITY_EDITOR
        const string PluginModuleName = "__Internal";
        #else
        const string PluginModuleName = "sp_unity_curl";
        #endif

        [DllImport(PluginModuleName)]
        public static extern void SPUnityCurlGetError(int id, byte[] data);

        [DllImport(PluginModuleName)]
        public static extern void SPUnityCurlGetBody(int id, byte[] data);

        [DllImport(PluginModuleName)]
        public static extern void SPUnityCurlGetHeaders(int id, byte[] data);

        [DllImport(PluginModuleName)]
        public static extern int SPUnityCurlGetCode(int id);

        [DllImport(PluginModuleName)]
        public static extern int SPUnityCurlGetErrorLength(int id);

        [DllImport(PluginModuleName)]
        public static extern int SPUnityCurlGetBodyLength(int id);

        [DllImport(PluginModuleName)]
        public static extern int SPUnityCurlGetHeadersLength(int id);

        [DllImport(PluginModuleName)]
        public static extern int SPUnityCurlGetDownloadSize(int id);

        [DllImport(PluginModuleName)]
        public static extern int SPUnityCurlGetDownloadSpeed(int id);

        [DllImport(PluginModuleName)]
        public static extern void SPUnityCurlInit();

        [DllImport(PluginModuleName)]
        public static extern void SPUnityCurlDestroy();

        [DllImport(PluginModuleName)]
        public static extern int SPUnityCurlCreateConn();

        [DllImport(PluginModuleName)]
        public static extern void SPUnityCurlDestroyConn(int id);

        [DllImport(PluginModuleName)]
        public static extern int SPUnityCurlSend(RequestStruct data);

        [DllImport(PluginModuleName)]
        public static extern int SPUnityCurlUpdate(int id);

        [DllImport(PluginModuleName)]
        public static extern void SPUnityCurlOnApplicationPause(bool pause);

    }
}