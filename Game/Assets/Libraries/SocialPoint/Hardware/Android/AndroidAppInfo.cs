using UnityEngine;
using System;
using SocialPoint.Base;

namespace SocialPoint.Hardware
{
#if UNITY_ANDROID
    public class AndroidAppInfo : IAppInfo
    {
        public AndroidAppInfo()
        {
            if(Application.platform != RuntimePlatform.Android)
            {
                throw new Exception("Only android platform supported.");
            }
        }

        public static AndroidJavaObject PackageManager
        {
            get
            {
                return AndroidContext.CurrentActivity.Call<AndroidJavaObject>("getPackageManager");
            }
        }

        public static AndroidJavaObject PackageInfo
        {
            get
            {
                return PackageManager.Call<AndroidJavaObject>("getPackageInfo", PackageName, 0);
            }
        }

        public static AndroidJavaObject Locale
        {
            get
            {
                var loc = new AndroidJavaClass("java.util.Locale");
                return loc.CallStatic<AndroidJavaObject>("getDefault");
            }
        }

        private static string _packageName;

        public static string PackageName
        {
            get
            {
                if(_packageName == null)
                {
                    _packageName = AndroidContext.CurrentActivity.Call<string>("getPackageName");
                }
                return _packageName;
            }
        }

        public string SeedId
        {
            get
            {
                return PackageName;
            }
        }

        public string Id
        {
            get
            {
                return PackageName;
            }
        }

        private string _version;

        public string Version
        {
            get
            {
                if(_version == null)
                {
                    _version = String.Empty + PackageInfo.Get<int>("versionCode");
                }
                return _version;
            }
        }

        private string _shortVersion;

        public string ShortVersion
        {
            get
            {
                if(_shortVersion == null)
                {
                    _shortVersion = PackageInfo.Get<string>("versionName");
                }
                return _shortVersion;
            }
        }

        private string _language = null;

        public string Language
        {
            get
            {
                if(_language == null)
                {
                    _language = Locale.Call<string>("getLanguage");
                }
                return _language;
            }
        }

        private string _country = null;

        public string Country
        {
            get
            {
                if(_country == null)
                {
                    _country = Locale.Call<string>("getCountry");
                }
                return _country;
            }
        }

        override public string ToString()
        {
            return InfoToStringExtension.ToString(this);
        }
    }
#else
    public class AndroidAppInfo : EmptyAppInfo
    {
    }
#endif
}

