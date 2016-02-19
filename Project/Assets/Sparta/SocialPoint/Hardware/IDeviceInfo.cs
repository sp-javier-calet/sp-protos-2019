
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

        [System.Obsolete("Use AdvertisingId instead", true)]
        string IDFA { get; }

    }

}

