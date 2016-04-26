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
                        var objResolver = AndroidContext.ContentResolver;
                        var clsSettings = new AndroidJavaClass("android.provider.Settings$Secure"); // API level 3
                        const string key = "http_proxy";
                        var proxyStr = clsSettings.CallStatic<string>("getString", objResolver, key);  // API level 3, deprecated in API level 17
                        if(string.IsNullOrEmpty(proxyStr))
                        {
                            try
                            {
                                clsSettings = new AndroidJavaClass("android.provider.Settings$Global");
                                proxyStr = clsSettings.CallStatic<string>("getString", objResolver, key); // API level 17
                            }
                            catch(AndroidJavaException)
                            {
                                
                            }
                        }
                        if(string.IsNullOrEmpty(proxyStr))
                        {
                            var sys = new AndroidJavaClass("java.lang.System"); // API level 1
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
                        if(string.IsNullOrEmpty(proxyStr))
                        {
                            var proxy = new AndroidJavaClass("android.net.Proxy"); // API level 1
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
                        if(!string.IsNullOrEmpty(proxyStr))
                        {
                            _proxy = new Uri("http://" + proxyStr);
                        }
                        _proxyLoaded = true;
                    }
                    catch(Exception e)
                    {
                        Debug.LogError("Device proxy could not be retrieved. " + e.Message);
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

