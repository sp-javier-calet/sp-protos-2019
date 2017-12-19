#if UNITY_ANDROID
using System;
using SocialPoint.Base;
using UnityEngine;
#endif

namespace SocialPoint.Hardware
{
    #if UNITY_ANDROID
    public sealed class AndroidNetworkInfo : INetworkInfo
    {
        public AndroidJavaObject SocketAddress
        {
            get
            {
                return null;
            }
        }

        Uri _proxy;
        bool _proxyLoaded;

        public Uri Proxy
        {
            get
            {
                if(!_proxyLoaded)
                {
                    try
                    {
                        const string key = "http_proxy";
                        string proxyStr;
                        var objResolver = AndroidContext.ContentResolver;
                        using(var clsSettingsSecure = new AndroidJavaClass("android.provider.Settings$Secure")) // API level 3
                        {
                            proxyStr = clsSettingsSecure.CallStatic<string>("getString", objResolver, key); // API level 3, deprecated in API level 17
                        }
                        if(string.IsNullOrEmpty(proxyStr))
                        {
                            try
                            {
                                using(var clsSettingsGlobal = new AndroidJavaClass("android.provider.Settings$Global"))
                                {
                                    proxyStr = clsSettingsGlobal.CallStatic<string>("getString", objResolver, key); // API level 17
                                }
                            }
                            catch(AndroidJavaException)
                            {
                                
                            }
                        }
                        if(string.IsNullOrEmpty(proxyStr))
                        {
                            using(var sys = new AndroidJavaClass("java.lang.System")) // API level 1
                            {
                                var host = sys.CallStatic<string>("getProperty", "http.proxyHost");
                                if(!string.IsNullOrEmpty(host))
                                {
                                    proxyStr = host;
                                    var port = sys.CallStatic<string>("getProperty", "http.proxyPort");
                                    if(!string.IsNullOrEmpty(port))
                                    {
                                        proxyStr += ":" + port;
                                    }
                                }
                            }
                        }
                        if(string.IsNullOrEmpty(proxyStr))
                        {
                            using(var proxy = new AndroidJavaClass("android.net.Proxy")) // API level 1
                            {
                                var host = proxy.CallStatic<string>("getHost", AndroidContext.CurrentActivity); // API level 1
                                if(!string.IsNullOrEmpty(host))
                                {
                                    proxyStr = host;
                                    var port = proxy.CallStatic<string>("getPort", AndroidContext.CurrentActivity); // API level 1, deprecated in API level 11
                                    if(!string.IsNullOrEmpty(port))
                                    {
                                        proxyStr += ":" + port;
                                    }
                                }
                            }
                        }
                        if(!string.IsNullOrEmpty(proxyStr))
                        {
                            _proxy = new Uri("http://" + proxyStr);
                        }
                        _proxyLoaded = true;
                    }
                    catch(Exception e)
                    {
                        Log.e("Device proxy could not be retrieved. " + e.Message);
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
    public sealed class AndroidNetworkInfo : EmptyNetworkInfo
    {
    }
#endif
}

