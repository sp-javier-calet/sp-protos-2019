using SocialPoint.IosKeychain;
using UnityEngine;

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
                            Debug.LogError("Could not write IosDeviceInfo.Uid to ios keychain: "+e);
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
