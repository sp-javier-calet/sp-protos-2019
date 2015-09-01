using System;
using UnityEngine;
using SocialPoint.Base;

namespace SocialPoint.Hardware
{
#if UNITY_ANDROID
    public class AndroidDeviceInfo : IDeviceInfo
    {
        AndroidMemoryInfo _memoryInfo;
        AndroidStorageInfo _storageInfo;
        AndroidAppInfo _appInfo;
        AndroidNetworkInfo _networkInfo;
        string _keychainAccessGroup;

        public AndroidDeviceInfo()
        {
            _memoryInfo = new AndroidMemoryInfo();
            _storageInfo = new AndroidStorageInfo();
            _appInfo = new AndroidAppInfo();
            _networkInfo = new AndroidNetworkInfo();
        }

        public static AndroidJavaObject GetSystemService(string name)
        {
            var ctx = new AndroidJavaClass("android.content.Context");
            var val = ctx.GetStatic<string>(name);
            return AndroidContext.CurrentActivity.Call<AndroidJavaObject>("getSystemService", val);
        }

        public static AndroidJavaObject ActivityManager
        {
            get
            {
                return GetSystemService("ACTIVITY_SERVICE");
            }
        }

        public static AndroidJavaObject AdvertisingIdClient
        {
            get
            {
                return new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
            }
        }

        private string _string = null;

        public string String
        {
            get
            {
                if(_string == null)
                {
                    var build = new AndroidJavaClass("android.os.Build");
                    var manufacturer = build.GetStatic<string>("MANUFACTURER");
                    var model = build.GetStatic<string>("MODEL");
                    if(model.StartsWith(manufacturer))
                    {
                        _string = model;
                    }
                    else
                    {
                        _string = manufacturer + " " + model;
                    }
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
                    var objResolver = AndroidContext.ContentResolver;
                    var clsSettings = new AndroidJavaClass("android.provider.Settings$Secure");
                    _uid = clsSettings.CallStatic<string>("getString", objResolver, "android_id");
                }
                return _uid;
            }
        }

        private readonly string _platform = "android";

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
                    _platformVersion = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<string>("RELEASE");
                }
                return _platformVersion;
            }
        }

        private bool? _isGooglePlayServicesAvailable = null;

        public bool IsGooglePlayServicesAvailable
        {
            get
            {
                if(!_isGooglePlayServicesAvailable.HasValue)
                {
                    try
                    {
                        try {
                            var availabilityClass =  new AndroidJavaClass("com.google.android.gms.common.GoogleApiAvailability");
                            int availabilityCode = availabilityClass.CallStatic<int>("isGooglePlayServicesAvailable", AndroidContext.CurrentActivity);
                            _isGooglePlayServicesAvailable = (availabilityCode == 0);
                        } 
                        catch (AndroidJavaException)
                        {
                            var availabilityClass =  new AndroidJavaClass("com.google.android.gms.common.GooglePlayServicesUtil");
                            int availabilityCode = availabilityClass.CallStatic<int>("isGooglePlayServicesAvailable", AndroidContext.CurrentActivity);
                            _isGooglePlayServicesAvailable = (availabilityCode == 0);
                        }
                    }
                    catch
                    {
                        _isGooglePlayServicesAvailable = false;
                        Debug.LogError("Error retrieving Google Play Services data");
                    }
                }
                return _isGooglePlayServicesAvailable.HasValue && (bool)_isGooglePlayServicesAvailable;
            }
        }

        private string _advertisingId = null;

        public string AdvertisingId
        {
            get
            {
                if(_advertisingId == null)
                {
                    if(IsGooglePlayServicesAvailable)
                    {
                        var adInfo = AdvertisingIdClient.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", AndroidContext.CurrentActivity);
                        _advertisingId = adInfo.Call<string>("getId").ToString();
                    }
                    else
                    {
                        _advertisingId = "";
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

        private bool _advertisingIdEnabled;
        private bool _advertisingIdEnabledLoaded;

        public bool AdvertisingIdEnabled
        {
            get
            {
                if(!_advertisingIdEnabledLoaded)
                {
                    if(IsGooglePlayServicesAvailable)
                    {
                        var adInfo = AdvertisingIdClient.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", AndroidContext.CurrentActivity);
                        _advertisingIdEnabled = !adInfo.Call<bool>("isLimitAdTrackingEnabled");
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
        
        private bool _rooted;
        private bool _rootedLoaded;
        
        public bool Rooted
        {
            get
            {
                if(!_rootedLoaded)
                {
                    var paths = new String[]{ "/sbin/su", "/system/bin/su", "/system/xbin/su", "/data/local/xbin/su",
                        "/data/local/bin/su", "/system/sd/xbin/su", "/system/bin/failsafe/su", "/data/local/su" };
                    foreach (var path in paths)
                    {
                        if(System.IO.File.Exists(path))
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

