using NUnit.Framework;

namespace SocialPoint.Hardware
{
    [TestFixture]
    [Category("SocialPoint.Hardware")]
    public class StorageAmountTests
    {
        [Test]
        public void Basic([Values(100UL)] ulong amount)
        {
            var s = new StorageAmount(amount);
            Assert.AreEqual(amount, s.Amount);
            Assert.AreEqual(amount, s.Bytes);
        }

        [Test]
        public void DifferentUnit([Values(150UL)] ulong amount)
        {
            var s = new StorageAmount(amount, StorageUnit.KiloBytes);
            Assert.AreEqual(amount*1024, s.Bytes);
        }

        [Test]
        public void ToAmount([Values(1500UL)] ulong amount)
        {
            var s = new StorageAmount(amount, StorageUnit.KiloBytes);
            Assert.AreEqual(amount/1024, s.ToAmount(StorageUnit.MegaBytes));
        }

        [Test]
        public void Transform([Values(1UL)] ulong amount)
        {
            var s = new StorageAmount(amount, StorageUnit.MegaBytes);
            var t = s.Transform(StorageUnit.Bytes);
            Assert.AreEqual(StorageUnit.Bytes, t.Unit);
            Assert.AreEqual(amount*1024*1024, t.Amount);
        }
    }
}
