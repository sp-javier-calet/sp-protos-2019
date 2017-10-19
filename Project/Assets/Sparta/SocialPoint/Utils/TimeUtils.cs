using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;

namespace SocialPoint.Utils
{
    public static class TimeUtils
    {
        public enum TimeType
        {
            DAY,
            HOUR,
            MIN,
            SEC
        }

        public static string DayLocalized = string.Empty;
        public static string DaysLocalized = string.Empty;
        public static string HourLocalized = string.Empty;
        public static string MinLocalized = string.Empty;
        public static string SecLocalized = string.Empty;

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

        #region Localize Time System

        //This system allows to configure the output time so it can return "1 d 1 h", "1 day 20 min", "2 hours 10 min 3 sec"...
        //The tids needs have to be set in advance

        public static string GetLocalizedTimeWithTypes(long timeFromServer, List<List<TimeType>> typeTimeFormatList)
        {
            var ts = TimeSpan.FromSeconds(timeFromServer);

            for(int i = 0; i < typeTimeFormatList.Count; i++)
            {
                if(ValidateLocalizeTIme(ts, typeTimeFormatList[i][0]))
                {
                    return GetLocalizeTime(ts, typeTimeFormatList[i]);
                }
            }

            List<TimeType> secondsType = new List<TimeType>();
            secondsType.Add(TimeType.SEC);
            return GetLocalizeTime(ts, secondsType);
        }

        static bool ValidateLocalizeTIme(TimeSpan ts, TimeType typeTimeFormat)
        {
            bool validated = true;
            switch(typeTimeFormat)
            {
            case TimeType.DAY:
                validated = ts.Days > 0;
                break;
            case TimeType.HOUR:
                validated = ts.Hours > 0;
                break;
            case TimeType.MIN:
                validated = ts.Minutes > 0;
            break;
            default:
            break;
            }
            return validated;
        }

        static string GetLocalizeTime(TimeSpan ts, List<TimeType> typeTimeFormat)
        {
            var sb = StringUtils.StartBuilder();

            for(int i = 0; i < typeTimeFormat.Count; i++)
            {
                switch(typeTimeFormat[i])
                {
                case TimeType.DAY:
                    sb.Append((ts.Days).ToString());
                    sb.Append(" ");
                    sb.Append((ts.Days > 1) ? DaysLocalized : DayLocalized);
                    break;
                case TimeType.HOUR:
                    sb.Append((ts.Hours).ToString());
                    sb.Append(" ");
                    sb.Append(HourLocalized);
                    break;
                case TimeType.MIN:
                    sb.Append((ts.Minutes).ToString());
                    sb.Append(" ");
                    sb.Append(MinLocalized);
                    break;
                default:
                    sb.Append(ts.Seconds.ToString());
                    sb.Append(" ");
                    sb.Append(SecLocalized);
                    break;
                }

                //We will add a space between unit times if there are more in the type list
                if(typeTimeFormat.Count - 1 > i)
                {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }

        #endregion

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
