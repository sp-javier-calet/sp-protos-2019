using System;
using UnityEngine;

namespace SocialPoint.Hardware
{
#if UNITY_ANDROID
    public class AndroidNetworkInfo : INetworkInfo
    {
        public AndroidJavaObject SocketAddress
        {
            get
            {
                try
                {
                    var sel = new AndroidJavaClass("java.net.ProxySelector").CallStatic<AndroidJavaObject>("getDefault");
                    var proxies = sel.Call<AndroidJavaObject>("select", new AndroidJavaObject("java.net.URI", "http://www.socialpoint.es"));
                    if(proxies.Call<int>("size") == 0)
                    {
                        return null;
                    }
                    return proxies.Call<AndroidJavaObject>("get", 0).Call<AndroidJavaObject>("address");
                }
                catch(Exception e)
                {
                    Debug.LogError("Device proxy could not be retrieved. " + e.Message);
                }

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
                    var addr = SocketAddress;
                    if(addr != null)
                    {
                        var str = "http://" + addr.Call<string>("getHostName") + ":" + addr.Call<int>("getPort");
                        _proxy = new Uri(str);
                    }
                    _proxyLoaded = true;
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

