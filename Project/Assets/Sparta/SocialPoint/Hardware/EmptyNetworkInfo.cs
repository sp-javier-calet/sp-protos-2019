using System;

namespace SocialPoint.Hardware
{
    public class EmptyNetworkInfo : INetworkInfo
    {
        public EmptyNetworkInfo()
        {
            Connectivity = INetworkInfoStatus.Unknown;
        }

        public INetworkInfoStatus Connectivity
        {
            get;
            set;
        }

        public Uri Proxy
        {
            get;
            set;
        }


        public string IpAddress
        {
            get;
            set;
        }

        override public string ToString()
        {
            return InfoToStringExtension.ToString(this);
        }

    }
}

