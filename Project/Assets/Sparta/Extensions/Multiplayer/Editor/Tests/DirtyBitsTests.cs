
using NUnit.Framework;
using System.IO;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    [TestFixture]
    [Category("SocialPoint.Multiplayer")]
    class DirtyBitsTests
    {
        [Test]
        public void Basic()
        {
            var bits = new DirtyBits();
            Assert.That(bits.Count == 0);
            Assert.That(bits.Finished);
        }

        [Test]
        public void Setting()
        {
            var bits = new DirtyBits();
            bits.Set(true);
            bits.Set(false);
            Assert.That(bits.Count == 2);
            Assert.That(bits.Finished);
            bits.Reset();
            Assert.That(bits.Count == 2);
            Assert.That(!bits.Finished);
            bits.Set(true);
            Assert.That(bits.Count == 2);
            bits.Set(false);
            bits.Set(true);
            Assert.That(bits.Count == 3);
            Assert.That(bits.Finished);
            bits.Clear();
            Assert.That(bits.Count == 0);
        }

        void GenericWriteAndRead(DirtyBits bits)
        {
            var size = bits.Count;
            var stream = new MemoryStream();
            var writer = new SystemBinaryWriter(stream);
            bits.Write(writer);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new SystemBinaryReader(stream);
            bits.Clear();
            bits.Read(reader, size);
        }

        [Test]
        public void WriteAndRead()
        {
            var bits = new DirtyBits();
            bits.Set(true);
            bits.Set(false);

            GenericWriteAndRead(bits);

            Assert.That(bits.Get());
            Assert.That(!bits.Get());
            Assert.That(bits.Finished);
        }

        [Test]
        public void WriteAndRead8()
        {
            var bits = new DirtyBits();
            bits.Set(true);
            bits.Set(false);
            bits.Set(true);
            bits.Set(false);
            bits.Set(true);
            bits.Set(false);
            bits.Set(true);
            bits.Set(false);

            GenericWriteAndRead(bits);

            Assert.That(bits.Get());
            Assert.That(!bits.Get());
            Assert.That(bits.Get());
            Assert.That(!bits.Get());
            Assert.That(bits.Get());
            Assert.That(!bits.Get());
            Assert.That(bits.Get());
            Assert.That(!bits.Get());
            Assert.That(bits.Finished);
        }

        [Test]
        public void WriteAndReadBig()
        {
            var bits = new DirtyBits();
            bool v = true;
            for(var i = 0; i < 60; i++)
            {
                bits.Set(v);
                v = !v;
            }
            GenericWriteAndRead(bits);
        }

    }
}
