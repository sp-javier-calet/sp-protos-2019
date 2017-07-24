using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedByteTest
    {
        [Test]
        public void ConversionImplicit()
        {
            byte value = 168;
            var obfuscatedByte = new ObfuscatedByte(value);
            byte unobfuscatedByte = obfuscatedByte;

            Assert.AreEqual(obfuscatedByte, obfuscatedByte);
            Assert.AreEqual(value, unobfuscatedByte);
            Assert.AreEqual(value, obfuscatedByte);

            Assert.AreNotEqual(value, obfuscatedByte.ObfuscatedValue);

            byte newValue = 99;
            obfuscatedByte = newValue;
            Assert.AreEqual(newValue, obfuscatedByte);
        }

        [Test]
        public void ComparisonEqual()
        {
            byte value = 73;
            var obfuscatedByte = new ObfuscatedByte(value);

            Assert.AreEqual(obfuscatedByte, value);
            Assert.AreEqual(value, obfuscatedByte);
        }

        [Test]
        public void ComparisonNotEqual()
        {
            byte value = 214;
            var obfuscatedByte = new ObfuscatedByte(value);

            Assert.AreNotEqual(obfuscatedByte, 128);
            Assert.AreNotEqual(20, obfuscatedByte);
        }

        [Test]
        public void ComparisonLess()
        {
            byte value = 64;
            var obfuscatedByte = new ObfuscatedByte(value);

            Assert.Less(obfuscatedByte, 100);
            Assert.Less(3, obfuscatedByte);
        }

        [Test]
        public void ComparisonLessEqual()
        {
            byte value = 95;
            var obfuscatedByte = new ObfuscatedByte(value);

            Assert.LessOrEqual(obfuscatedByte, 243);
            Assert.LessOrEqual(value, obfuscatedByte);
        }

        [Test]
        public void ComparisonGreater()
        {
            byte value = 194;
            var obfuscatedByte = new ObfuscatedByte(value);

            Assert.Greater(obfuscatedByte, 156);
            Assert.Greater(255, obfuscatedByte);
        }

        [Test]
        public void ComparisonGreaterEqual()
        {
            byte value = 137;
            var obfuscatedByte = new ObfuscatedByte(value);

            Assert.GreaterOrEqual(obfuscatedByte, value);
            Assert.GreaterOrEqual(209, obfuscatedByte);
        }

        [Test]
        public void OperatorAddition()
        {
            byte value1 = 13;
            var obfuscatedByte1 = new ObfuscatedByte(value1);

            byte value2 = 7;
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
        public void OperatorSubtraction()
        {
            byte value1 = 25;
            var obfuscatedByte1 = new ObfuscatedByte(value1);

            byte value2 = 5;
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
        public void OperatorMultiplication()
        {
            byte value1 = 11;
            var obfuscatedByte1 = new ObfuscatedByte(value1);

            byte value2 = 6;
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
        public void OperatorDivision()
        {
            byte value1 = 80;
            var obfuscatedByte1 = new ObfuscatedByte(value1);

            byte value2 = 8;
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