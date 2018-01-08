#if UNITY_ANDROID
using System;
using SocialPoint.Utils;
using SocialPoint.IO;
using SocialPoint.Base;
using UnityEngine;
#endif

namespace SocialPoint.Hardware
{
    #if UNITY_ANDROID
    public class AndroidDeviceInfo : IDeviceInfo
    {
        AndroidMemoryInfo _memoryInfo;
        AndroidStorageInfo _storageInfo;
        AndroidAppInfo _appInfo;
        AndroidNetworkInfo _networkInfo;

        public AndroidDeviceInfo()
        {
            _memoryInfo = new AndroidMemoryInfo();
            _storageInfo = new AndroidStorageInfo();
            _appInfo = new AndroidAppInfo();
            _networkInfo = new AndroidNetworkInfo();
        }

        public static AndroidJavaObject GetSystemService(string name)
        {
            using(var ctx = new AndroidJavaClass("android.content.Context"))
            {
                var val = ctx.GetStatic<string>(name);
                return AndroidContext.CurrentActivity.Call<AndroidJavaObject>("getSystemService", val); // API level 1
            }
        }

        public static AndroidJavaObject ActivityManager
        {
            get
            {
                return GetSystemService("ACTIVITY_SERVICE");
            }
        }

        static AndroidJavaObject AdvertisingIdClient
        {
            get
            {
                return new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
            }
        }

        string _string;

        public string String
        {
            get
            {
                if(_string == null)
                {
                    using(var build = new AndroidJavaClass("android.os.Build")) // API level 1
                    {
                        var manufacturer = build.GetStatic<string>("MANUFACTURER"); // API level 4
                        var model = build.GetStatic<string>("MODEL"); // API level 1
                        if(model.StartsWith(manufacturer))
                        {
                            _string = model;
                        }
                        else
                        {
                            _string = manufacturer + " " + model;
                        }
                    }
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
                    var objResolver = AndroidContext.ContentResolver;
                    using(var clsSettings = new AndroidJavaClass("android.provider.Settings$Secure")) // API level 3
                    {
                        _uid = clsSettings.CallStatic<string>("getString", objResolver, "android_id"); // API level 3
                    }
                }
                return _uid;
            }
        }

        readonly string _platform = "android";

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
                    using(var buildVersion = new AndroidJavaClass("android.os.Build$VERSION"))
                    {
                        _platformVersion = buildVersion.GetStatic<string>("RELEASE"); // API level 1
                    }
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
                    if(AndroidContext.SDKVersion >= 21)
                    {
                        try
                        {
                            using(var build = new AndroidJavaClass("android.os.Build"))
                            {
                                var supported_abis = build.GetStatic<string[]>("SUPPORTED_ABIS"); // API level 21
                                _architecture = supported_abis.Length > 0 ? supported_abis[0] : string.Empty;
                            }
                        }
                        catch
                        {
                            _architecture = string.Empty;
                            Log.e("Error retrieving DeviceInfo Architecture");
                        }
                    }
                    else
                    {
                        try
                        {
                            using(var build = new AndroidJavaClass("android.os.Build"))
                            {
                                var cpu_abi = build.GetStatic<string>("CPU_ABI"); // API level 4, deprecated in API level 21
                                _architecture = cpu_abi;
                            }
                        }
                        catch
                        {
                            _architecture = string.Empty;
                            Log.e("Error retrieving DeviceInfo Architecture");
                        }
                    }
                }
                return _architecture;
            }
        }


        bool? _isGooglePlayServicesAvailable;

        public bool IsGooglePlayServicesAvailable
        {
            get
            {
                if(!_isGooglePlayServicesAvailable.HasValue)
                {
                    try
                    {
                        try
                        {
                            using(var availabilityClass = new AndroidJavaClass("com.google.android.gms.common.GoogleApiAvailability"))
                            {
                                int availabilityCode = availabilityClass.CallStatic<int>("isGooglePlayServicesAvailable", AndroidContext.CurrentActivity);
                                _isGooglePlayServicesAvailable = (availabilityCode == 0);
                            }
                        }
                        catch(AndroidJavaException)
                        {
                            using(var availabilityClass = new AndroidJavaClass("com.google.android.gms.common.GooglePlayServicesUtil"))
                            {
                                int availabilityCode = availabilityClass.CallStatic<int>("isGooglePlayServicesAvailable", AndroidContext.CurrentActivity);
                                _isGooglePlayServicesAvailable = (availabilityCode == 0);
                            }
                        }
                    }
                    catch
                    {
                        _isGooglePlayServicesAvailable = false;
                        Log.e("Error retrieving Google Play Services data");
                    }
                }
                return _isGooglePlayServicesAvailable.HasValue && (bool)_isGooglePlayServicesAvailable;
            }
        }

        string _advertisingId;

        public string AdvertisingId
        {
            get
            {
                if(_advertisingId == null)
                {
                    if(IsGooglePlayServicesAvailable)
                    {
                        try
                        {
                            using(var adIdClientObject = AdvertisingIdClient)
                            {
                                using(var adInfo = adIdClientObject.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", AndroidContext.CurrentActivity))
                                {
                                    _advertisingId = adInfo.Call<string>("getId");
                                }
                            }
                        }
                        catch(AndroidJavaException)
                        {
                            _advertisingId = string.Empty;
                        }
                    }
                    else
                    {
                        _advertisingId = string.Empty;
                    }
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

        Rect _safeAreaRectSize = Rect.zero;

        public Rect SafeAreaRectSize
        {
            get
            {
                if(_safeAreaRectSize == Rect.zero)
                {
                    _safeAreaRectSize = new Rect(0f, 0f, ScreenSize.x, ScreenSize.y);
                }
                return _safeAreaRectSize;
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

        bool _advertisingIdEnabled;
        bool _advertisingIdEnabledLoaded;

        public bool AdvertisingIdEnabled
        {
            get
            {
                if(!_advertisingIdEnabledLoaded)
                {
                    if(IsGooglePlayServicesAvailable)
                    {
                        try
                        {
                            using(var adIdClientObject = AdvertisingIdClient)
                            {
                                using(var adInfo = adIdClientObject.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", AndroidContext.CurrentActivity))
                                {
                                    _advertisingIdEnabled = !adInfo.Call<bool>("isLimitAdTrackingEnabled");
                                }
                            }
                        }
                        catch(AndroidJavaException)
                        {
                            _advertisingIdEnabled = false;
                        }
                    }
                    else
                    {
                        _advertisingIdEnabled = false;
                    }
                    _advertisingIdEnabledLoaded = true;
                }
                return _advertisingIdEnabled;
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
                    var paths = new [] { "/sbin/su", "/system/bin/su", "/system/xbin/su", "/data/local/xbin/su",
                        "/data/local/bin/su", "/system/sd/xbin/su", "/system/bin/failsafe/su", "/data/local/su"
                    };
                    for(int i = 0, pathsLength = paths.Length; i < pathsLength; i++)
                    {
                        var path = paths[i];
                        if(FileUtils.ExistsFile(path))
                        {
                            _rooted = true;
                            break;
                        }
                    }
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
    #else
    public class AndroidDeviceInfo : EmptyDeviceInfo
    {
    }
#endif
}

