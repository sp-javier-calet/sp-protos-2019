using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedByteTest
    {
        [Test]
        public void ConversionImplicit([Values(68)] byte value)
        {
            var obfuscatedByte = new ObfuscatedByte(value);
            byte unobfuscatedByte = obfuscatedByte;

            Assert.AreEqual(obfuscatedByte, obfuscatedByte);
            Assert.AreEqual(value, unobfuscatedByte);
            Assert.AreEqual(value, obfuscatedByte);

            Assert.AreNotEqual(value, obfuscatedByte.ObfuscatedValue);

            byte newValue = (byte)(value + 1);
            obfuscatedByte = newValue;
            Assert.AreEqual(newValue, obfuscatedByte);
        }

        [Test]
        public void ToString([Values(32)] byte value)
        {
            var obfuscatedByte = new ObfuscatedByte(value);

            Assert.AreEqual(obfuscatedByte.ToString(), value.ToString());
        }

        [Test]
        public void ComparisonEqual([Values(73)] byte value)
        {
            var obfuscatedByte = new ObfuscatedByte(value);

            Assert.AreEqual(obfuscatedByte, value);
            Assert.AreEqual(value, obfuscatedByte);
        }

        [Test]
        public void ComparisonNotEqual([Values(114)] byte value)
        {
            var obfuscatedByte = new ObfuscatedByte(value);

            byte differentValue = (byte)(value + 1);
            Assert.AreNotEqual(obfuscatedByte, differentValue);
            Assert.AreNotEqual(differentValue, obfuscatedByte);
        }

        [Test]
        public void ComparisonLess([Values(64)] byte value1, [Values(100)] byte value2)
        {
            Assert.Less(value1, value2);

            var obfuscatedByte1 = new ObfuscatedByte(value1);
            var obfuscatedByte2 = new ObfuscatedByte(value2);

            Assert.Less(obfuscatedByte1, obfuscatedByte2);
            Assert.Less(obfuscatedByte1, value2);
            Assert.Less(value1, obfuscatedByte2);
        }

        [Test]
        public void ComparisonLessEqual([Values(95, 101)] byte value1, [Values(101)] byte value2)
        {
            Assert.LessOrEqual(value1, value2);

            var obfuscatedByte1 = new ObfuscatedByte(value1);
            var obfuscatedByte2 = new ObfuscatedByte(value2);

            Assert.LessOrEqual(obfuscatedByte1, obfuscatedByte2);
            Assert.LessOrEqual(obfuscatedByte1, value2);
            Assert.LessOrEqual(value1, obfuscatedByte2);
        }

        [Test]
        public void ComparisonGreater([Values(127)] byte value1, [Values(41)] byte value2)
        {
            Assert.Greater(value1, value2);

            var obfuscatedByte1 = new ObfuscatedByte(value1);
            var obfuscatedByte2 = new ObfuscatedByte(value2);

            Assert.Greater(obfuscatedByte1, obfuscatedByte2);
            Assert.Greater(obfuscatedByte1, value2);
            Assert.Greater(value1, obfuscatedByte2);
        }

        [Test]
        public void ComparisonGreaterEqual([Values(117, 54)] byte value1, [Values(54)] byte value2)
        {
            Assert.GreaterOrEqual(value1, value2);

            var obfuscatedByte1 = new ObfuscatedByte(value1);
            var obfuscatedByte2 = new ObfuscatedByte(value2);

            Assert.GreaterOrEqual(obfuscatedByte1, obfuscatedByte2);
            Assert.GreaterOrEqual(obfuscatedByte1, value2);
            Assert.GreaterOrEqual(value1, obfuscatedByte2);
        }

        [Test]
        public void OperatorAddition([Values(13)] byte value1, [Values(7)] byte value2)
        {
            var obfuscatedByte1 = new ObfuscatedByte(value1);
            var obfuscatedByte2 = new ObfuscatedByte(value2);

            byte addition = (byte)(value1 + value2);
            byte obfuscatedAddition = new ObfuscatedByte(addition);

            Assert.AreEqual(obfuscatedByte1 + obfuscatedByte2, addition);
            Assert.AreEqual(obfuscatedByte1 + value2, addition);
            Assert.AreEqual(value1 + obfuscatedByte2, addition);
            Assert.AreEqual(obfuscatedByte1 + obfuscatedByte2, obfuscatedAddition);
            Assert.AreEqual(obfuscatedByte1 + value2, obfuscatedAddition);
            Assert.AreEqual(value1 + obfuscatedByte2, obfuscatedAddition);

            ++value1;
            ++obfuscatedByte1;
            Assert.AreEqual(value1, obfuscatedByte1);
        }

        [Test]
        public void OperatorSubtraction([Values(25)] byte value1, [Values(5)] byte value2)
        {
            var obfuscatedByte1 = new ObfuscatedByte(value1);
            var obfuscatedByte2 = new ObfuscatedByte(value2);

            byte subtraction = (byte)(value1 - value2);
            byte obfuscatedSubtraction = new ObfuscatedByte(subtraction);

            Assert.AreEqual(obfuscatedByte1 - obfuscatedByte2, subtraction);
            Assert.AreEqual(obfuscatedByte1 - value2, subtraction);
            Assert.AreEqual(value1 - obfuscatedByte2, subtraction);
            Assert.AreEqual(obfuscatedByte1 - obfuscatedByte2, obfuscatedSubtraction);
            Assert.AreEqual(obfuscatedByte1 - value2, obfuscatedSubtraction);
            Assert.AreEqual(value1 - obfuscatedByte2, obfuscatedSubtraction);

            --value1;
            --obfuscatedByte1;
            Assert.AreEqual(value1, obfuscatedByte1);
        }

        [Test]
        public void OperatorMultiplication([Values(11)] byte value1, [Values(6)] byte value2)
        {
            var obfuscatedByte1 = new ObfuscatedByte(value1);
            var obfuscatedByte2 = new ObfuscatedByte(value2);

            byte multiplication = (byte)(value1 * value2);
            byte obfuscatedMultiplication = new ObfuscatedByte(multiplication);

            Assert.AreEqual(obfuscatedByte1 * obfuscatedByte2, multiplication);
            Assert.AreEqual(obfuscatedByte1 * value2, multiplication);
            Assert.AreEqual(value1 * obfuscatedByte2, multiplication);
            Assert.AreEqual(obfuscatedByte1 * obfuscatedByte2, obfuscatedMultiplication);
            Assert.AreEqual(obfuscatedByte1 * value2, obfuscatedMultiplication);
            Assert.AreEqual(value1 * obfuscatedByte2, obfuscatedMultiplication);
        }

        [Test]
        public void OperatorDivision([Values(80)] byte value1, [Values(8)] byte value2)
        {
            var obfuscatedByte1 = new ObfuscatedByte(value1);
            var obfuscatedByte2 = new ObfuscatedByte(value2);

            byte division = (byte)(value1 / value2);
            byte obfuscatedDivision = new ObfuscatedByte(division);

            Assert.AreEqual(obfuscatedByte1 / obfuscatedByte2, division);
            Assert.AreEqual(obfuscatedByte1 / value2, division);
            Assert.AreEqual(value1 / obfuscatedByte2, division);
            Assert.AreEqual(obfuscatedByte1 / obfuscatedByte2, obfuscatedDivision);
            Assert.AreEqual(obfuscatedByte1 / value2, obfuscatedDivision);
            Assert.AreEqual(value1 / obfuscatedByte2, obfuscatedDivision);
        }
    }
}