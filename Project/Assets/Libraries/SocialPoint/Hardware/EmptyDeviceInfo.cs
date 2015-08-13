
namespace SocialPoint.Hardware
{
    public class EmptyDeviceInfo : IDeviceInfo
    {
        EmptyMemoryInfo _memoryInfo;
        EmptyStorageInfo _storageInfo;
        EmptyAppInfo _appInfo;
        EmptyNetworkInfo _networkInfo;

        public EmptyDeviceInfo()
        {
            MemoryInfo = new EmptyMemoryInfo();
            StorageInfo = new EmptyStorageInfo();
            AppInfo = new EmptyAppInfo();
            NetworkInfo = new EmptyNetworkInfo();
        }

        public string String { get; set; }

        public string Uid { get; set; }

        public IMemoryInfo MemoryInfo { get; set; } 

        public IStorageInfo StorageInfo
        {
            get;
            set;
        }

        public IAppInfo AppInfo
        {
            get;
            set;
        }

        public INetworkInfo NetworkInfo
        {
            get;
            set;
        }

        public string Platform
        {
            get;
            set;
        }

        public string PlatformVersion
        {
            get;
            set;
        }

        public string Model
        {
            get;
            set;
        }

        public string Language
        {
            get;
            set;
        }

        public string AdvertisingId
        {
            get;
            set;
        }

        public string IDFA
        {
            get
            {
                return AdvertisingId;
            }
        }

        public bool AdvertisingIdEnabled
        {
            get;
            set;
        }

        public bool Rooted
        {
            get;
            set;
        }

        override public string ToString()
        {
            return InfoToStringExtension.ToString(this);
        }
    }
}

