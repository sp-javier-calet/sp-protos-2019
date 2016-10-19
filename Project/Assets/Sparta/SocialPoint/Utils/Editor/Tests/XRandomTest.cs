using System;
using NUnit.Framework;

namespace SocialPoint.Utils
{
    [TestFixture]
    [Category("SocialPoint.Utils")]
    public class XRandomTest
    {
        const int SeedA = 183783364;
        const int SeedB = 870230629;

        XRandom RNGSeedA1;
        XRandom RNGSeedA2;
        XRandom RNGSeedA3;
        XRandom RNGSeedB1;
        XRandom RNGSeedNested;

        [SetUp]
        public void SetUp()
        {
            RNGSeedA1 = new XRandom(SeedA);
            RNGSeedA2 = new XRandom(SeedA);
            RNGSeedA3 = new XRandom(SeedA);
            RNGSeedB1 = new XRandom(SeedB);
        }

        [Test]
        [Repeat(1000)]
        public void Deterministic()
        {
            var v1 = RNGSeedA1.Range(0, 100000);
            var v2 = RNGSeedA2.Range(0, 100000);
            Assert.That(v1 == v2);

            var f1 = RNGSeedA1.Range(0, 100.0f);
            var f2 = RNGSeedA2.Range(0, 100.0f);
            Assert.That(f1 == f2);

            var u1 = RNGSeedA1.Range(0u, 100u);
            var u2 = RNGSeedA2.Range(0u, 100u);
            Assert.That(u1 == u2);

            var n1 = RNGSeedA1.Next();
            var n2 = RNGSeedA2.Next();
            Assert.That(n1 == n2);
        }

        [Test]
        [Repeat(1000)]
        public void SeededRandom()
        {
            var v1 = RNGSeedA3.Range(0, 100000);
            var v2 = RNGSeedB1.Range(0, 100000);
            Assert.That(v1 != v2);
        }
    }
}