using System;
using UnityEngine;
using SocialPoint.Base;

namespace SocialPoint.Hardware
{
#if UNITY_ANDROID
    public class AndroidNetworkInfo : INetworkInfo
    {
        public AndroidJavaObject SocketAddress
        {
            get
            {

                return null;
            }
        }

        private Uri _proxy;
        private bool _proxyLoaded = false;

        public Uri Proxy
        {
            get
            {
                if(!_proxyLoaded)
                {
                    try
                    {
                        var objResolver = AndroidContext.ContentResolver;
                        var clsSettings = new AndroidJavaClass("android.provider.Settings$Secure");
                        var key = "http_proxy";
                        var proxyStr = clsSettings.CallStatic<string>("getString", objResolver, key);
                        if(string.IsNullOrEmpty(proxyStr))
                        {
                            clsSettings = new AndroidJavaClass("android.provider.Settings$Global");
                            proxyStr = clsSettings.CallStatic<string>("getString", objResolver, key);
                        }
                        if(!string.IsNullOrEmpty(proxyStr))
                        {
                            _proxy = new Uri("http://"+proxyStr);
                        }
                        _proxyLoaded = true;
                    }
                    catch(Exception e)
                    {
                        UnityEngine.Debug.LogError("Device proxy could not be retrieved. " + e.Message);
                    }
                }
                return _proxy;
            }
        }

        public INetworkInfoStatus Connectivity
        {
            get
            {
                return new UnityNetworkInfo().Connectivity;
            }
        }

        public string IpAddress
        {
            get
            {
                return new UnityNetworkInfo().IpAddress;
            }
        }

        override public string ToString()
        {
            return InfoToStringExtension.ToString(this);
        }
    }
#else
    public class AndroidNetworkInfo : EmptyNetworkInfo
    {
    }
#endif
}

