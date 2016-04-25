using UnityEngine;

namespace SocialPoint.Hardware
{
    public class UnityDeviceInfo : IDeviceInfo
    {
        EmptyMemoryInfo _memoryInfo;
        EmptyStorageInfo _storageInfo;
        EmptyAppInfo _appInfo;
        EmptyNetworkInfo _networkInfo;

        public UnityDeviceInfo()
        {
            MemoryInfo = new EmptyMemoryInfo();
            StorageInfo = new EmptyStorageInfo();
            AppInfo = new UnityAppInfo();
            NetworkInfo = new UnityNetworkInfo();
        }

        public string String
        {
            get
            {
                return SystemInfo.deviceModel;
            }
        }

        public string Uid
        {
            get
            {
                return SystemInfo.deviceUniqueIdentifier;
            }
         }

        public IMemoryInfo MemoryInfo
        {
            get;
            set;
        } 

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

        string _platform;
        public string Platform
        {
            get
            {
                if(_platform != null)
                {
                    return _platform;
                }
                switch(Application.platform)
                {
                case RuntimePlatform.IPhonePlayer:
                    return "ios";
                case RuntimePlatform.Android:
                    return "android";
                default:
                    return "unity";
                }
            }

            set
            {
                _platform = value;
            }
        }

        public string PlatformVersion
        {
            get
            {
                return SystemInfo.operatingSystem;
            }
        }

        public string Architecture
        {
            get
            {
                return "";
            }
        }

        public string Model
        {
            get
            {
                return SystemInfo.deviceModel;
            }
        }

        public string Language
        {
            get
            {
                return Application.systemLanguage.ToString();
            }
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

