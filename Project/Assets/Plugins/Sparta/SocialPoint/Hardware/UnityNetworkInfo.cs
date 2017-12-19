using UnityEngine;
using System;
using System.Net;

namespace SocialPoint.Hardware
{
    public sealed class UnityNetworkInfo : INetworkInfo
    {
        Uri _testUri;

        public UnityNetworkInfo()
        {
            _testUri = new Uri("http://socialpoint.es");
        }

        public INetworkInfoStatus Connectivity
        {
            get
            {
                switch(Application.internetReachability)
                {
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    return INetworkInfoStatus.ReachableViaWWAN;
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    return INetworkInfoStatus.ReachableViaWiFi;
                case NetworkReachability.NotReachable:
                    return INetworkInfoStatus.NotReachable;
                default:
                    return INetworkInfoStatus.Unknown;
                }
            }
        }

        public Uri Proxy
        {
            get
            {
                var proxy = WebRequest.DefaultWebProxy != null ? WebRequest.DefaultWebProxy.GetProxy(_testUri) : null;
                return proxy == _testUri ? null : proxy;
            }
        }

        public string IpAddress
        {
            get
            {
                var host = Dns.GetHostName();
                try
                {
                    var info = Dns.GetHostEntry(host);
                    if(info.AddressList.Length > 0)
                    {
                        return info.AddressList[0].ToString();
                    }
                }
                catch(System.Net.Sockets.SocketException)
                {
                    // Do nothing
                }
                return null;
            }
        }

        override public string ToString()
        {
            return InfoToStringExtension.ToString(this);
        }
    }
}

