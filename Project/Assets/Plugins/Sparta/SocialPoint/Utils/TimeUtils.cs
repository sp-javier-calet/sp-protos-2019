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

        public enum RoundingMode
        {
            CEIL,
            ROUND,
            FLOOR
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

        public const int SecondsInADay = 86400;

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

        public static string GetLocalizedTimeWithTypes(TimeSpan ts, TimeType[] typesTimeFormat, int maxTypesToShow = 0, RoundingMode roundingMode = RoundingMode.FLOOR)
        {
            if(maxTypesToShow <= 0)
            {
                maxTypesToShow = Int32.MaxValue;
            }

            var numTypesShown = 0;

            var sb = StringUtils.StartBuilder();
            for(int i = 0; i < typesTimeFormat.Length; i++)
            {
                var roundedTime = ts;
                bool isLastType = i == typesTimeFormat.Length - 1;
                if(isLastType)
                {
                    roundedTime = GetRoundingCorrection(ts, typesTimeFormat[i], roundingMode);
                }

                switch(typesTimeFormat[i])
                {
                case TimeType.DAY:
                    if(roundedTime.Days == 0 && numTypesShown == 0 && !isLastType)
                    {
                        continue;
                    }
                    sb.Append(roundedTime.Days + " ");
                    sb.Append((roundedTime.Days == 1) ? DayLocalized : DaysLocalized);
                    numTypesShown++;
                    break;
                case TimeType.HOUR:
                    if(roundedTime.Hours == 0 && numTypesShown == 0 && !isLastType)
                    {
                        continue;
                    }
                    sb.Append(roundedTime.Hours + " " + HourLocalized);
                    numTypesShown++;
                    break;
                case TimeType.MIN:
                    if(roundedTime.Minutes == 0 && numTypesShown == 0 && !isLastType)
                    {
                        continue;
                    }
                    sb.Append(roundedTime.Minutes + " " + MinLocalized);
                    numTypesShown++;
                    break;
                default:
                    if(roundedTime.Seconds == 0 && numTypesShown == 0 && !isLastType)
                    {
                        continue;
                    }
                    sb.Append(roundedTime.Seconds + " " + SecLocalized);
                    numTypesShown++;
                    break;
                }

                if(numTypesShown >= maxTypesToShow)
                {
                    break;
                }

                //We will add a space between unit times if there are more in the type list
                if(!isLastType)
                {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }

        static TimeSpan GetRoundingCorrection(TimeSpan ts, TimeType timeType, RoundingMode roundingMode)
        {
            switch(timeType)
            {
            case TimeType.DAY:
                return TimeSpan.FromDays(Round(ts.TotalDays, roundingMode));
            case TimeType.HOUR:
                return TimeSpan.FromHours(Round(ts.TotalHours, roundingMode));
            case TimeType.MIN:
                return TimeSpan.FromMinutes(Round(ts.TotalMinutes, roundingMode));
            case TimeType.SEC:
                return TimeSpan.FromSeconds(Round(ts.TotalSeconds, roundingMode));
            }
            return TimeSpan.Zero;
        }

        static double Round(double value, RoundingMode mode)
        {
            switch(mode)
            {
            case RoundingMode.FLOOR:
                return Math.Floor(value);
            case RoundingMode.ROUND:
                return Math.Round(value);
            case RoundingMode.CEIL:
                return Math.Ceiling(value);
            }
            return value;
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

        public static int GetSecondsUntilDay(DateTime currentDay, int amountOfDays)
        {
            int secondsUntilMidnight = (24 - currentDay.Hour) * 3600;
            secondsUntilMidnight += (60 - currentDay.Minute) * 60;
            secondsUntilMidnight += 60 - currentDay.Second;
            --amountOfDays;
            int totalSecondsUntilNextDay = SecondsInADay * amountOfDays;
            totalSecondsUntilNextDay += secondsUntilMidnight;
            return totalSecondsUntilNextDay;
        }
    }
}
