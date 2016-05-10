using UnityEngine;

namespace SocialPoint.Hardware
{
    public interface IDeviceInfo
    {
        string String { get; }

        /**
         *  Returns the uid of the device
         */
        string Uid { get; }

        /**
         *  Returns the memory info of the device
         */
        IMemoryInfo MemoryInfo { get; }

        /**
         *  Returns the storage info of the device
         */
        IStorageInfo StorageInfo { get; }

        /**
         *  Returns the application info
         */
        IAppInfo AppInfo { get; }

        /**
         *  Returns the network info
         */
        INetworkInfo NetworkInfo { get; }

        /**
         *  Returns the platform of the device
         */
        string Platform { get; }

        /**
         *  Returns the platform version of the device
         */
        string PlatformVersion { get; }

        /**
         *  Returns the Architecture of the device
         */
        string Architecture { get; }

        /**
         *  Returns the Model of the device
         */
        string Model { get; }

        /**
         *  Returns the Language of the device
         */
        string Language { get; }

        /**
         *  Returns the Advertising Identifier (IDFA) of the device
         */
        string AdvertisingId { get; }

        /**
         *  Returns if the user allowed the advertising id to be shared
         */
        bool AdvertisingIdEnabled { get; }

        /**
         *  Returns if the device is jailbroken
         */
        bool Rooted { get; }

        /**
         *  Returns the largest size of a texture that is supported by the graphics hardware
         */
        int MaxTextureSize { get; }

        /**
         *  Returns the screen size in pixels width and height
         */
        Vector2 ScreenSize { get; }

        /**
         *  Returns the current DPI of the screen / device
         */
        float ScreenDpi { get; }

        /**
         *  Returns the number of processors
         */
        int CpuCores { get; }

        /**
         *  Returns the number of processors frequency in MHz 
         */
        int CpuFreq { get; }

        /**
         *  Returns the processor name 
         */
        string CpuModel { get; }

        /**
         *  Returns the Cpu architecture
         */
        string CpuArchitecture { get; }

        /**
         *  Returns the company responsible for this GL implementation
         */
        string OpenglVendor { get; }

        /**
         *  Returns the processor name 
         */
        string OpenglRenderer { get; }

        /**
         *  Returns a space-separated list of supported extensions to GL
         */
        string OpenglExtensions { get; }

        /**
         *  Returns a version or release number for the shading language
         */
        int OpenglShadingVersion { get; }

        /**
         *  Returns a version or release number.
         */
        string OpenglVersion { get; }

        /**
         *  Returns an approximate amount of graphics memory in megabytes
         */
        int OpenglMemorySize { get; }


        [System.Obsolete("Use AdvertisingId instead", true)]
        string IDFA { get; }

    }

}

