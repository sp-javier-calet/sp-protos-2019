using System;
using NUnit.Framework;

namespace SocialPoint.Utils
{
    [TestFixture]
    [Category("SocialPoint.Utils")]
    public sealed class CryptographyTests
    {
        [Test]
        public void HashSha256EqualTest()
        {
            UInt64 int64_1 = UInt64.MaxValue;
            UInt64 int64_2 = UInt64.MaxValue;
            string int64MaxValueHash = "80286269-91923802-1defd241-0079bcb3-f2e04613-6db5ec29-c5b0e0a9-dd242448";

            string hash1 = CryptographyUtils.GetHashSha256(int64_1.ToString());
            string hash2 = CryptographyUtils.GetHashSha256(int64_2.ToString());
            Assert.AreEqual(hash1, hash2);
            Assert.AreEqual(hash1, int64MaxValueHash); 
        }

        [Test]
        public void HashSha256NotEqualTest()
        {
            UInt64 int64_1 = UInt64.MaxValue;
            UInt64 int64_2 = UInt64.MinValue;

            string hash1 = CryptographyUtils.GetHashSha256(int64_1.ToString());
            string hash2 = CryptographyUtils.GetHashSha256(int64_2.ToString());
            Assert.AreNotEqual(hash1, hash2);

            hash1 = CryptographyUtils.GetHashSha256("String Test 1");
            hash2 = CryptographyUtils.GetHashSha256("String Test 2");
            Assert.AreNotEqual(hash1, hash2);

            hash1 = CryptographyUtils.GetHashSha256("string test");
            hash2 = CryptographyUtils.GetHashSha256("String Test");
            Assert.AreNotEqual(hash1, hash2);

            hash1 = CryptographyUtils.GetHashSha256("Test String");
            hash2 = CryptographyUtils.GetHashSha256("String Test");
            Assert.AreNotEqual(hash1, hash2);
        }
    }
}