using System;
using NUnit.Framework;

namespace SocialPoint.Utils
{
    [TestFixture]
    [Category("SocialPoint.Utils")]
    class RandomUtilsTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        [Repeat(1000)]
        public void UserIdTest()
        {	
            var uid1 = RandomUtils.GenerateUserId();
            var uid2 = RandomUtils.GenerateUserId();
            Assert.That(uid1 != uid2);

            var rnd1 = uid1 & 0x7FFFFFFF;
            var rnd2 = uid2 & 0x7FFFFFFF;

            Assert.That(rnd1 != rnd2);

            var ts1 = uid1 >> 31;
            var ts2 = uid2 >> 31;

            var ts = (ulong)TimeUtils.Timestamp;

            Assert.That(ts1 == ts2);
            Assert.That(ts == ts1);
        }

    }
}
