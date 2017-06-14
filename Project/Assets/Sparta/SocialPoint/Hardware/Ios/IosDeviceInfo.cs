using System.Collections.Generic;
using SocialPoint.IosKeychain;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.Hardware
{
    public class IosDeviceInfo : IDeviceInfo
    {
        // check: https://socialpoint.atlassian.net/wiki/display/MT/iOS+Keychain
        const string UidKeychainItemId = "SPDeviceID";
        const string UidKeychainAccessGroup = "es.socialpoint";

        static Dictionary<string, int> CpuFreqMap = new Dictionary<string, int> {
            { "iPhone1,1", 412 },  // iPhone
            { "iPod1,1", 412 },    // iPod touch
            { "iPhone1,2", 412 },  // iPhone 3G
            { "iPod2,1", 532 },    // iPod touch (2G)
            { "iPhone2,1", 600 },  // iPhone 3GS
            { "iPod3,1", 600 },    // iPod touch (3G)
            { "iPad1,1", 1000 },   // iPad
            { "iPhone3,1", 800 },  // iPhone 4
            { "iPhone3,2", 800 },  // iPhone 4
            { "iPhone3,3", 800 },  // iPhone 4
            { "iPod4,1", 800 },    // iPod touch (4G)
            { "AppleTV2,1", 800 }, // Apple TV (2G)
            { "iPad2,1", 1000 },   // iPad 2
            { "iPad2,2", 1000 },   // iPad 2
            { "iPad2,3", 1000 },   // iPad 2
            { "iPhone4,1", 800 },  // iPhone 4S
            { "iPad3,1", 1000 },   // iPad (3G)
            { "iPad3,2", 1000 },   // iPad (3G)
            { "iPad3,3", 1000 },   // iPad (3G)
            { "iPhone5,1", 1300 }, // iPhone 5
            { "iPhone5,2", 1300 }, // iPhone 5
            { "iPod5,1", 800 },    // iPod touch (5G)
            { "iPad3,4", 1400 },   // iPad (4G)
            { "iPad3,5", 1400 },   // iPad (4G)
            { "iPad3,6", 1400 },   // iPad (4G)
            { "iPad2,5", 1000 },   // iPad mini
            { "iPad2,6", 1000 },   // iPad mini
            { "iPad2,7", 1000 },   // iPad mini
            { "AppleTV3,1", 1000 },// Apple TV (3G Rev A)
            { "iPhone5,3", 1300 }, // iPhone 5c
            { "iPhone5,4", 1300 }, // iPhone 5c
            { "iPhone6,1", 1300 }, // iPhone 5s
            { "iPhone6,2", 1300 }, // iPhone 5s
            { "iPhone6,3", 1300 }, // iPhone 5s
            { "iPad4,1", 1400 },   // iPad Air
            { "iPad4,2", 1400 },   // iPad Air
            { "iPad4,3", 1400 },   // iPad Air
            { "iPad4,4", 1300 },   // iPad mini 2
            { "iPad4,5", 1300 },   // iPad mini 2
            { "iPad4,6", 1300 },   // iPad mini 2
            { "iPhone7,2", 1400 }, // iPhone 6
            { "iPhone7,1", 1400 }, // iPhone 6 Plus
            { "iPad5,3", 1500 },   // iPad Air 2
            { "iPad5,4", 1500 },   // iPad Air 2
            { "iPad4,7", 1300 },   // iPad mini 3
            { "iPad4,8", 1300 },   // iPad mini 3
            { "iPad4,9", 1300 },   // iPad mini 3
            { "Watch1,1", 520 },   // Apple Watch 38mm
            { "Watch1,2", 520 },   // Apple Watch 42mm
            { "iPod7,1", 1100 },   // iPod touch (6G)
            { "AppleTV5,3", 1400 },// Apple TV (4G)
            { "iPad6,7", 2260 },   // iPad Pro
            { "iPad6,8", 2260 },   // iPad Pro
            { "iPad5,1", 1400 },   // iPad mini 4
            { "iPad5,2", 1400 },   // iPad mini 4
            { "iPhone8,1", 1850 }, // iPhone 6S
            { "iPhone8,2", 1850 }, // iPhone 6S Plus
            { "iPhone8,4", 1850 }, // iPhone SE
            { "iPad6,3", 2160 },   // 9.7-inch iPad Pro
            { "iPad6,4", 2160 },   // 9.7-inch iPad Pro
            { "iPhone9,1", 2340 }, // iPhone 7
            { "iPhone9,3", 2340 }, // iPhone 7
            { "iPhone9,2", 2340 }, // iPhone 7 Plus
            { "iPhone9,4", 2340 }  // iPhone 7 Plus
        };

        IosMemoryInfo _memoryInfo;
        IosStorageInfo _storageInfo;
        IosAppInfo _appInfo;
        IosNetworkInfo _networkInfo;

        public IosDeviceInfo()
        {
            _memoryInfo = new IosMemoryInfo();
            _storageInfo = new IosStorageInfo();
            _appInfo = new IosAppInfo();
            _networkInfo = new IosNetworkInfo();
        }

        string _string;

        public string String
        {
            get
            {
                if(_string == null)
                {
                    _string = IosHardwareBridge.SPUnityHardwareGetDeviceString();
                }
                return _string;
            }
        }

        string _uid;

        public string Uid
        {
            get
            {
                if(_uid == null)
                {
                    var item = new KeychainItem(UidKeychainItemId, UidKeychainAccessGroup);
                    _uid = item.Value;
                    if(string.IsNullOrEmpty(_uid))
                    {
                        _uid = System.Guid.NewGuid().ToString();
                        try
                        {
                            item.Value = _uid;
                        }
                        catch(KeychainItemException e)
                        {
                            Log.e("Could not write IosDeviceInfo.Uid to ios keychain: " + e);
                        }
                    }
                }
                return _uid;
            }
        }

        readonly string _platform = "ios";

        public string Platform
        {
            get
            {
                return _platform;
            }
        }

        string _platformVersion;

        public string PlatformVersion
        {
            get
            {
                if(_platformVersion == null)
                {
                    _platformVersion = IosHardwareBridge.SPUnityHardwareGetDevicePlatformVersion();
                }
                return _platformVersion;
            }
        }

        string _architecture;

        public string Architecture
        {
            get
            {
                if(_architecture == null)
                {
                    _architecture = IosHardwareBridge.SPUnityHardwareGetDeviceArchitecture();
                }
                return _architecture;
            }
        }

        string _advertisingId;

        public string AdvertisingId
        {
            get
            {
                if(_advertisingId == null)
                {
                    _advertisingId = IosHardwareBridge.SPUnityHardwareGetDeviceAdvertisingId();
                }
                return _advertisingId;
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

        Vector2 _screenSize = Vector2.zero;

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
                //SystemInfo.processorFrequency is not supported for iOS
                int freq;
                if(!CpuFreqMap.TryGetValue(Model, out freq))
                {
                    // unknow model
                    freq = -1;
                }
                return freq;
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
            get
            {
                return Architecture;
            }
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
            get
            {
                return IosHardwareBridge.SPUnityHardwareGetDeviceAdvertisingIdEnabled();
            }
        }

        bool _rooted;
        bool _rootedLoaded;

        public bool Rooted
        {
            get
            {
                if(!_rootedLoaded)
                {
                    _rooted = IosHardwareBridge.SPUnityHardwareGetDeviceRooted();
                    _rootedLoaded = true;
                }
                return _rooted;
            }
        }

        public IMemoryInfo MemoryInfo
        {
            get
            {
                return _memoryInfo;
            }
        }

        public IStorageInfo StorageInfo
        {
            get
            {
                return _storageInfo;
            }
        }

        public IAppInfo AppInfo
        {
            get
            {
                return _appInfo;
            }
        }

        public INetworkInfo NetworkInfo
        {
            get
            {
                return _networkInfo;
            }
        }

        override public string ToString()
        {
            return InfoToStringExtension.ToString(this);
        }

    }
}
