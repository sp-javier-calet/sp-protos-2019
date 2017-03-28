using System;
using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.Network.ServerEvents
{
    public interface IServerEventTracker
    {
        Action<Metric> SendMetric { get; set; }

        Action<Log, bool> SendLog { get; set; }

        Action<string, AttrDic, ErrorDelegate> SendTrack { get; set; }
    }
}

