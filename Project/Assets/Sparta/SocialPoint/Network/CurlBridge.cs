using System.Runtime.InteropServices;
using SocialPoint.Network;

namespace SocialPoint.Network
{
    public sealed class CurlBridge
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RequestStruct
        {
            public int Id;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Url;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Query;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Method;
            public int Timeout;
            public int ActivityTimeout;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Proxy;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Headers;
            [MarshalAs(UnmanagedType.LPArray)]
            public byte[] Body;
            public int BodyLength;
        };


        #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        const string PluginModuleName = "SPUnityPlugins";
        #elif UNITY_ANDROID && !UNITY_EDITOR
        const string PluginModuleName = "sp_unity_curl";
        #elif (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
        const string PluginModuleName = "__Internal";
        #else // linux editor and linux standalone
        const string PluginModuleName = "sp_unity_curl";
        #endif

        [DllImport(PluginModuleName)]
        public static extern void SPUnityCurlGetError(int id, byte[] data);

        [DllImport(PluginModuleName)]
        public static extern void SPUnityCurlGetBody(int id, byte[] data);

        [DllImport(PluginModuleName)]
        public static extern void SPUnityCurlGetHeaders(int id, byte[] data);

        [DllImport(PluginModuleName)]
        public static extern int SPUnityCurlGetResponseCode(int id);

        [DllImport(PluginModuleName)]
        public static extern int SPUnityCurlGetErrorCode(int id);

        [DllImport(PluginModuleName)]
        public static extern int SPUnityCurlGetErrorLength(int id);

        [DllImport(PluginModuleName)]
        public static extern int SPUnityCurlGetBodyLength(int id);

        [DllImport(PluginModuleName)]
        public static extern int SPUnityCurlGetHeadersLength(int id);

        [DllImport(PluginModuleName)]
        public static extern double SPUnityCurlGetConnectTime(int id);

        [DllImport(PluginModuleName)]
        public static extern double SPUnityCurlGetTotalTime(int id);

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

        [DllImport(PluginModuleName)]
        public static extern void SPUnityCurlSetConfig(string name);
    }
}
