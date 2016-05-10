
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
#if !UNITY_ANDROID || UNITY_EDITOR
                return SystemInfo.deviceUniqueIdentifier;
#else
                return "0";
#endif
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

        private string _platform;

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

        public int MaxTextureSize
        {
            get
            {
                return SystemInfo.maxTextureSize;
            }
        }

        private Vector2 _screenSize = Vector2.zero;

        public Vector2 ScreenSize
        {
            get
            {
                if(_screenSize == Vector2.zero)
                {
                    _screenSize.x = Screen.width;
                    _screenSize.y = Screen.height;
                }
                return _screenSize;
            }
        }

        public float ScreenDpi
        {
            get
            {
                return Screen.dpi;
            }
        }

        public int CpuCores
        {
            get
            {
                return SystemInfo.processorCount;
            }
        }

        public int CpuFreq
        {
            get
            {
                return SystemInfo.processorFrequency;
            }
        }

        public string CpuModel
        {
            get
            {
                return SystemInfo.processorType;
            }
        }

        public string CpuArchitecture
        {
            get;
            set;
        }

        public string OpenglVendor
        {
            get
            {
                return SystemInfo.graphicsDeviceVendor;
            }
        }

        public string OpenglRenderer
        {
            get
            {
                return SystemInfo.graphicsDeviceName;
            }
        }

        public string OpenglExtensions
        {
            get;
            set;
        }

        public int OpenglShadingVersion
        {
            get
            {
                return SystemInfo.graphicsShaderLevel;
            }
        }

        public string OpenglVersion
        {
            get
            {
                return SystemInfo.graphicsDeviceVersion;
            }
        }

        public int OpenglMemorySize
        {
            get
            {
                return SystemInfo.graphicsMemorySize;
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

