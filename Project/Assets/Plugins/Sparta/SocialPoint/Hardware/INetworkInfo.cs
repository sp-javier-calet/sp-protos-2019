using System;

namespace SocialPoint.Hardware
{
    public enum INetworkInfoStatus
    {
        NotReachable,
        ReachableViaWiFi,
        ReachableViaWWAN,
        Unknown
    }

    public interface INetworkInfo
    {
        INetworkInfoStatus Connectivity { get; }

        Uri Proxy { get; }

        string IpAddress { get; }
    }
}