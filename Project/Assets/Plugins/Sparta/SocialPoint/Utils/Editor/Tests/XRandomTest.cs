using System;
using NUnit.Framework;
using FixMath.NET;

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
        public void Range()
        {
            const int vMin = 0;
            const int vMax = 100000;
            var v = RNGSeedA1.Range(vMin, vMax);
            Assert.That(v >= vMin);
            Assert.That(v < vMax);

            Fix64 fMin = Fix64.Zero;
            Fix64 fMax = new Fix64(10000);
            var f = RNGSeedA1.Range(fMin, fMax);
            Assert.That(f >= fMin);
            Assert.That(f < fMax);

            const uint uMin = 0u;
            const uint uMax = 100000u;
            var u = RNGSeedA1.Range(uMin, uMax);
            Assert.That(u >= uMin);
            Assert.That(u < uMax);
        }

        [Test]
        [Repeat(1000)]
        public void Negative()
        {
            const int vMin = -10000;
            const int vMax = 10000;
            var v = RNGSeedA1.Range(vMin, vMax);
            Assert.That(v >= vMin);
            Assert.That(v < vMax);

            Fix64 fMin = new Fix64(-10000);
            Fix64 fMax = new Fix64(10000);
            var f = RNGSeedA1.Range(fMin, fMax);
            Assert.That(f >= fMin);
            Assert.That(f < fMax);
        }

        [Test]
        [Repeat(1000)]
        public void Deterministic()
        {
            var v1 = RNGSeedA1.Range(0, 100000);
            var v2 = RNGSeedA2.Range(0, 100000);
            Assert.That(v1 == v2);

            var f1 = RNGSeedA1.Range(Fix64.Zero, new Fix64(100));
            var f2 = RNGSeedA2.Range(Fix64.Zero, new Fix64(100));
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