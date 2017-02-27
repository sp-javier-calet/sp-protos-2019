using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.ServerEvents;
using SocialPoint.Utils;

namespace SocialPoint.ServerEvents
{
    public enum MetricType
    {
        Timing,
        Gauge,
        Counter,
        Set,
        Histogram
    }

    public class Metric
    {
        const string AttrKeyStat = "stat";
        const string AttrKeyValue = "value";
        const string AttrKeyTimestamp = "timestamp";
        const string AttrKeyTags = "tags";

        public MetricType MetricType{ private set; get; }

        public string Stat{ private set; get; }

        public int Value{ private set; get; }

        public long Time{ private set; get; }

        public string[] Tags{ private set; get; }

        public Metric(MetricType type, string stat, int value, string[] tags = null)
        {
            MetricType = type;
            Stat = stat;
            Value = value;
            Time = TimeUtils.Timestamp;
            Tags = tags?? new string[] { };
        }

        public Attr ToAttr()
        {
            var dic = new AttrDic();
            dic.SetValue(AttrKeyStat, Stat);
            dic.SetValue(AttrKeyValue, Value);
            dic.SetValue(AttrKeyTimestamp, Time);
            var tagsList = new AttrList();
            for (int i = 0; i < Tags.Length; i++)
            {
                tagsList.Add(new AttrString(Tags[i]));
            }
            dic.Set(AttrKeyTags, tagsList);
            return dic;
        }
    }
}