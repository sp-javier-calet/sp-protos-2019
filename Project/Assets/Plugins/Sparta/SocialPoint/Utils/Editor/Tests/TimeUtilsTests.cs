using System;
using NUnit.Framework;

namespace SocialPoint.Utils
{
    [TestFixture]
    [Category("SocialPoint.Utils")]
    public class TimeUtilsTests
    {
        [SetUp]
        public void SetUp()
        {
            TimeUtils.DayLocalized = "d";
            TimeUtils.DaysLocalized = "D";
            TimeUtils.HourLocalized = "h";
            TimeUtils.MinLocalized = "m";
            TimeUtils.SecLocalized = "s";
        }

        [Test]
        public void LocalizedSeconds()
        {
            const int numSeconds = 50;
            var ts = TimeSpan.FromSeconds(numSeconds);
            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.SEC}, 1);
            Assert.AreEqual(numSeconds + " " + TimeUtils.SecLocalized, time);
        }

        [Test]
        public void LocalizedMinutes()
        {
            const int num = 2;
            var ts = TimeSpan.FromMinutes(num);
            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.MIN});
            Assert.AreEqual(num + " " + TimeUtils.MinLocalized, time);
        }

        [Test]
        public void LocalizedHours()
        {
            const int num = 6;
            var ts = TimeSpan.FromHours(num);
            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.HOUR});
            Assert.AreEqual(num + " " + TimeUtils.HourLocalized, time);
        }

        [Test]
        public void LocalizedDay()
        {
            const int num = 1;
            var ts = TimeSpan.FromDays(num);
            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.DAY});
            Assert.AreEqual(num + " " + TimeUtils.DayLocalized, time);
        }

        [Test]
        public void LocalizedDays()
        {
            const int num = 3;
            var ts = TimeSpan.FromDays(num);
            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.DAY});
            Assert.AreEqual(num + " " + TimeUtils.DaysLocalized, time);
        }

        [Test]
        public void LocalizedZeroSecs()
        {
            var time = TimeUtils.GetLocalizedTimeWithTypes(TimeSpan.Zero, new []{TimeUtils.TimeType.SEC});
            Assert.AreEqual("0 " + TimeUtils.SecLocalized, time);
        }

        [Test]
        public void LocalizedZeroMins()
        {
            const int num = 0;
            var ts = TimeSpan.FromMinutes(num);
            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.MIN});
            Assert.AreEqual(num + " " + TimeUtils.MinLocalized, time);
        }

        [Test]
        public void LocalizedZeroHours()
        {
            const int num = 0;
            var ts = TimeSpan.FromHours(num);
            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.HOUR});
            Assert.AreEqual(num + " " + TimeUtils.HourLocalized, time);
        }

        [Test]
        public void LocalizedZeroDay()
        {
            const int num = 0;
            var ts = TimeSpan.FromDays(num);
            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.DAY});
            Assert.AreEqual(num + " " + TimeUtils.DaysLocalized, time);
        }

        [Test]
        public void LocalizedAll()
        {
            const int days = 3;
            const int hours = 4;
            const int minutes = 27;
            const int seconds = 32;

            var ts = TimeSpan.FromDays(days) + TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);

            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.DAY, TimeUtils.TimeType.HOUR, TimeUtils.TimeType.MIN, TimeUtils.TimeType.SEC});
            Assert.AreEqual(days + " " + TimeUtils.DaysLocalized + " " + hours + " " + TimeUtils.HourLocalized + " " + minutes + " " + TimeUtils.MinLocalized + " " + seconds + " " + TimeUtils.SecLocalized, time);
        }

        [Test]
        public void LocalizedAllScrambled()
        {
            const int days = 3;
            const int hours = 4;
            const int minutes = 27;
            const int seconds = 32;

            var ts = TimeSpan.FromDays(days) + TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);

            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.MIN, TimeUtils.TimeType.SEC, TimeUtils.TimeType.HOUR, TimeUtils.TimeType.DAY});
            Assert.AreEqual(minutes + " " + TimeUtils.MinLocalized + " " + seconds + " " + TimeUtils.SecLocalized + " " + hours + " " + TimeUtils.HourLocalized + " " + days + " " + TimeUtils.DaysLocalized, time);
        }

        [Test]
        public void LocalizedMaxTypes()
        {
            const int days = 3;
            const int hours = 4;
            const int minutes = 27;
            const int seconds = 32;

            var ts = TimeSpan.FromDays(days) + TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);

            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.DAY, TimeUtils.TimeType.HOUR, TimeUtils.TimeType.MIN, TimeUtils.TimeType.SEC}, 2);
            Assert.AreEqual(days + " " + TimeUtils.DaysLocalized + " " + hours + " " + TimeUtils.HourLocalized, time);
        }

        [Test]
        public void LocalizedMaxTypesWithInitialZeroValue()
        {
            const int days = 0;
            const int hours = 0;
            const int minutes = 27;
            const int seconds = 32;

            var ts = TimeSpan.FromDays(days) + TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);

            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.DAY, TimeUtils.TimeType.HOUR, TimeUtils.TimeType.MIN, TimeUtils.TimeType.SEC}, 2);
            Assert.AreEqual(minutes + " " + TimeUtils.MinLocalized + " " + seconds + " " + TimeUtils.SecLocalized, time);
        }

        [Test]
        public void LocalizedMaxTypesWithMidZeroValue()
        {
            const int days = 3;
            const int hours = 0;
            const int minutes = 27;
            const int seconds = 32;

            var ts = TimeSpan.FromDays(days) + TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);

            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.DAY, TimeUtils.TimeType.HOUR, TimeUtils.TimeType.MIN, TimeUtils.TimeType.SEC}, 2);
            Assert.AreEqual(days + " " + TimeUtils.DaysLocalized + " " + hours + " " + TimeUtils.HourLocalized, time);
        }

        [Test]
        public void LocalizedRoundingDown()
        {
            const int minutes = 2;
            const int seconds = 29;
            var ts = TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.MIN}, 0, TimeUtils.RoundingMode.ROUND);
            Assert.AreEqual(minutes + " " + TimeUtils.MinLocalized, time);
        }

        [Test]
        public void LocalizedRoundingUp()
        {
            const int minutes = 2;
            const int seconds = 31;
            var ts = TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.MIN}, 0, TimeUtils.RoundingMode.ROUND);
            Assert.AreEqual((minutes + 1) + " " + TimeUtils.MinLocalized, time);
        }

        [Test]
        public void LocalizedFloor()
        {
            const int days = 2;
            const int hours = 23;
            var ts = TimeSpan.FromDays(days) + TimeSpan.FromHours(hours);
            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.DAY}, 0, TimeUtils.RoundingMode.FLOOR);
            Assert.AreEqual(days + " " + TimeUtils.DaysLocalized, time);
        }

        [Test]
        public void LocalizedCeil()
        {
            const int days = 2;
            const int hours = 1;
            var ts = TimeSpan.FromDays(days) + TimeSpan.FromHours(hours);
            var time = TimeUtils.GetLocalizedTimeWithTypes(ts, new []{TimeUtils.TimeType.DAY}, 0, TimeUtils.RoundingMode.CEIL);
            Assert.AreEqual((days + 1) + " " + TimeUtils.DaysLocalized, time);
        }
    }
}