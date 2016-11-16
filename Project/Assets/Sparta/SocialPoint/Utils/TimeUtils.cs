using System;
using System.Collections;
using System.Text;

namespace SocialPoint.Utils
{
    public static class TimeUtils
    {
        static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public delegate void OffsetChangeDelegate(TimeSpan change);

        static public event OffsetChangeDelegate OffsetChanged;

        static TimeSpan _offset;

        static public TimeSpan Offset
        {
            get
            {
                return _offset;
            }

            set
            {
                if(_offset != value)
                {
                    var diff = value - _offset;
                    _offset = value;
                    if(OffsetChanged != null)
                    {
                        OffsetChanged(diff);
                    }
                }
            }
        }

        static long SecondsOffset
        {
            get
            {
                return GetTimestamp(Offset);
            }
        }

        static public long GetTimestamp(TimeSpan span)
        {
            return (long)Offset.TotalSeconds;
        }

        static public long GetTimestamp(DateTime dt)
        {
            return (long)dt.Subtract(Epoch).TotalSeconds;
        }

        static public DateTime GetDateTime(long ts)
        {
            return Epoch.AddSeconds(ts);
        }

        static public double GetTimestampDouble(DateTime dt)
        {
            return dt.Subtract(Epoch).TotalSeconds;
        }

        static public long GetTimestampMilliseconds(DateTime dt)
        {
            return (long)dt.Subtract(Epoch).TotalMilliseconds;
        }

        static public DateTime GetTime(double timestamp)
        {
            return Epoch.AddSeconds(timestamp).ToLocalTime();
        }

        static public DateTime Now
        {
            get
            {
                return DateTime.UtcNow + Offset;
            }
        }

        [Obsolete("Use the Timestamp property instead")]
        static public long TimeStamp
        {
            get
            {
                return Timestamp;
            }
        }

        [Obsolete("Use the TimestampDouble property instead")]
        static public long TimeStampD
        {
            get
            {
                return Timestamp;
            }
        }

        static public long Timestamp
        {
            get
            {
                return GetTimestamp(Now);
            }
        }

        static public double TimestampDouble
        {
            get
            {
                return GetTimestampDouble(Now);
            }
        }

        static public long TimestampMilliseconds
        {
            get
            {
                return GetTimestampMilliseconds(Now);
            }
        }

        public static string FormatTime(long timeInSeconds)
        {
            return FormatTime((float)timeInSeconds);
        }

        public static string FormatTime(float time)
        {
            var ts = TimeSpan.FromSeconds(time);

            if(ts.Days > 0)
            {
                return ts.Hours > 0 ? ts.FormatTime("{0}d {1}h") : ts.FormatTime("{0}d");
            }

            if(ts.Hours > 0)
            {
                return ts.Minutes > 0 ? ts.FormatTime("{1}h {2}m") : ts.FormatTime("{1}h");

            }

            if(ts.Minutes > 0)
            {
                return ts.Seconds > 0 ? ts.FormatTime("{2}m {3}s") : ts.FormatTime("{2}m ");
            }

            return ts.FormatTime("{3}s");
        }

        public static string FormatTime(this TimeSpan ts, string formatString)
        {
            StringBuilder stringBuilder = StringUtils.StartBuilder();

            stringBuilder.AppendFormat(formatString, ts.Days, ts.Hours, ts.Minutes, ts.Seconds);

            return StringUtils.FinishBuilder(stringBuilder);
        }
        
        //Example:
        //yield return StartCoroutine(TimeUtils.WaitForRealSeconds(0.5f));
        public static IEnumerator WaitForRealSeconds(float time)
        {
            double start = TimestampDouble;
            while(TimestampDouble < start + time)
            {
                yield return null;
            }
        }

        public static DateTime ToDateTime(this int timestamp)
        {
            return ToDateTime(timestamp);
        }

        public static DateTime ToDateTime(this long timestamp)
        {
            return Epoch.AddSeconds(timestamp).ToLocalTime();
        }

        static public DateTime ToUtcDateTime(long ts)
        {
            return Epoch.AddSeconds((double)ts);
        }
    }
}
