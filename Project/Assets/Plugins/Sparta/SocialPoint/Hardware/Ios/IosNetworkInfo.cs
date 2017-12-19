using System;

namespace SocialPoint.Hardware
{
    public sealed class IosNetworkInfo : INetworkInfo
    {
        public IosNetworkInfo ()
        {
        }

        public INetworkInfoStatus Connectivity
        {
            get
            {
                string state = IosHardwareBridge.SPUnityHardwareGetNetworkConnectivity();
                if(state == "none")
                {
                    return INetworkInfoStatus.NotReachable;
                }
                else if(state == "wifi")
                {
                    return INetworkInfoStatus.ReachableViaWiFi;
                }
                else if(state == "wwan")
                {
                    return INetworkInfoStatus.ReachableViaWWAN;
                }
                else
                {
                    return INetworkInfoStatus.Unknown;
                }
            }
        }

        public Uri Proxy
        {
            get
            {
                try
                {
                    return new Uri("http://" + IosHardwareBridge.SPUnityHardwareGetNetworkProxy());
                }
                catch
                {
                    return null;
                }
            }
        }

        public string IpAddress
        {
            get
            {
                return IosHardwareBridge.SPUnityHardwareGetNetworkIpAddress();
            }
        }

        override public string ToString()
        {
            return InfoToStringExtension.ToString(this);
        }
    }
}

