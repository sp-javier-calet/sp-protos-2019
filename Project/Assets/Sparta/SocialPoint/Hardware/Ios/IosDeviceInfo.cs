using System;
using UnityEngine;
using SocialPoint.IosKeychain;

namespace SocialPoint.Hardware
{
    public class IosDeviceInfo : IDeviceInfo
    {
        // check: https://socialpoint.atlassian.net/wiki/display/MT/iOS+Keychain
        const string UidKeychainItemId = "SPDeviceID";
        const string UidKeychainAccessGroup = "es.socialpoint";

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

        private string _string = null;

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

        private string _uid = null;

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
                            UnityEngine.Debug.LogError("Could not write IosDeviceInfo.Uid to ios keychain: " + e);
                        }
                    }
                }
                return _uid;
            }
        }

        private readonly string _platform = "ios";

        public string Platform
        {
            get
            {
                return _platform;
            }
        }

        private string _platformVersion = null;

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

        private string _advertisingId = null;

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
            get
            {
                return IosHardwareBridge.SPUnityHardwareGetDeviceAdvertisingIdEnabled();
            }
        }

        private bool _rooted;
        private bool _rootedLoaded;

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
