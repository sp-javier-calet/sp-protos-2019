using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedSByteTest
    {
        [Test]
        public void ConversionImplicit()
        {
            sbyte value = 40;
            var obfuscatedSByte = new ObfuscatedSByte(value);
            sbyte unobfuscatedSByte = obfuscatedSByte;

            Assert.AreEqual(obfuscatedSByte, obfuscatedSByte);
            Assert.AreEqual(value, unobfuscatedSByte);
            Assert.AreEqual(value, obfuscatedSByte);

            Assert.AreNotEqual(value, obfuscatedSByte.ObfuscatedValue);
        }

        [Test]
        public void ComparisonEqual()
        {
            sbyte value = -55;
            var obfuscatedSByte = new ObfuscatedSByte(value);

            Assert.AreEqual(obfuscatedSByte, value);
            Assert.AreEqual(value, obfuscatedSByte);
        }

        [Test]
        public void ComparisonNotEqual()
        {
            sbyte value = 86;
            var obfuscatedSByte = new ObfuscatedSByte(value);

            Assert.AreNotEqual(obfuscatedSByte, 0);
            Assert.AreNotEqual(-108, obfuscatedSByte);
        }

        [Test]
        public void ComparisonLess()
        {
            sbyte value = -64;
            var obfuscatedSByte = new ObfuscatedSByte(value);

            Assert.Less(obfuscatedSByte, -28);
            Assert.Less(-125, obfuscatedSByte);
        }

        [Test]
        public void ComparisonLessEqual()
        {
            sbyte value = -33;
            var obfuscatedSByte = new ObfuscatedSByte(value);

            Assert.LessOrEqual(obfuscatedSByte, 115);
            Assert.LessOrEqual(value, obfuscatedSByte);
        }

        [Test]
        public void ComparisonGreater()
        {
            sbyte value = 66;
            var obfuscatedSByte = new ObfuscatedSByte(value);

            Assert.Greater(obfuscatedSByte, 28);
            Assert.Greater(127, obfuscatedSByte);
        }

        [Test]
        public void ComparisonGreaterEqual()
        {
            sbyte value = 9;
            var obfuscatedSByte = new ObfuscatedSByte(value);

            Assert.GreaterOrEqual(obfuscatedSByte, value);
            Assert.GreaterOrEqual(81, obfuscatedSByte);
        }

        [Test]
        public void OperatorAddition()
        {
            sbyte value1 = 13;
            var obfuscatedSByte1 = new ObfuscatedSByte(value1);

            sbyte value2 = 7;
            var obfuscatedSByte2 = new ObfuscatedSByte(value2);

            sbyte addition = (sbyte)(value1 + value2);
            sbyte obfuscatedAddition = new ObfuscatedSByte(addition);

            Assert.AreEqual(obfuscatedSByte1 + obfuscatedSByte2, addition);
            Assert.AreEqual(obfuscatedSByte1 + value2, addition);
            Assert.AreEqual(value1 + obfuscatedSByte2, addition);
            Assert.AreEqual(obfuscatedSByte1 + obfuscatedSByte2, obfuscatedAddition);
            Assert.AreEqual(obfuscatedSByte1 + value2, obfuscatedAddition);
            Assert.AreEqual(value1 + obfuscatedSByte2, obfuscatedAddition);

            ++value1;
            ++obfuscatedSByte1;
            Assert.AreEqual(value1, obfuscatedSByte1);
        }

        [Test]
        public void OperatorSubtraction()
        {
            sbyte value1 = 25;
            var obfuscatedSByte1 = new ObfuscatedSByte(value1);

            sbyte value2 = 5;
            var obfuscatedSByte2 = new ObfuscatedSByte(value2);

            sbyte subtraction = (sbyte)(value1 - value2);
            sbyte obfuscatedSubtraction = new ObfuscatedSByte(subtraction);

            Assert.AreEqual(obfuscatedSByte1 - obfuscatedSByte2, subtraction);
            Assert.AreEqual(obfuscatedSByte1 - value2, subtraction);
            Assert.AreEqual(value1 - obfuscatedSByte2, subtraction);
            Assert.AreEqual(obfuscatedSByte1 - obfuscatedSByte2, obfuscatedSubtraction);
            Assert.AreEqual(obfuscatedSByte1 - value2, obfuscatedSubtraction);
            Assert.AreEqual(value1 - obfuscatedSByte2, obfuscatedSubtraction);

            --value1;
            --obfuscatedSByte1;
            Assert.AreEqual(value1, obfuscatedSByte1);
        }

        [Test]
        public void OperatorMultiplication()
        {
            sbyte value1 = 11;
            var obfuscatedSByte1 = new ObfuscatedSByte(value1);

            sbyte value2 = 6;
            var obfuscatedSByte2 = new ObfuscatedSByte(value2);

            sbyte multiplication = (sbyte)(value1 * value2);
            sbyte obfuscatedMultiplication = new ObfuscatedSByte(multiplication);

            Assert.AreEqual(obfuscatedSByte1 * obfuscatedSByte2, multiplication);
            Assert.AreEqual(obfuscatedSByte1 * value2, multiplication);
            Assert.AreEqual(value1 * obfuscatedSByte2, multiplication);
            Assert.AreEqual(obfuscatedSByte1 * obfuscatedSByte2, obfuscatedMultiplication);
            Assert.AreEqual(obfuscatedSByte1 * value2, obfuscatedMultiplication);
            Assert.AreEqual(value1 * obfuscatedSByte2, obfuscatedMultiplication);
        }

        [Test]
        public void OperatorDivision()
        {
            sbyte value1 = 80;
            var obfuscatedSByte1 = new ObfuscatedSByte(value1);

            sbyte value2 = 8;
            var obfuscatedSByte2 = new ObfuscatedSByte(value2);

            sbyte division = (sbyte)(value1 / value2);
            sbyte obfuscatedDivision = new ObfuscatedSByte(division);

            Assert.AreEqual(obfuscatedSByte1 / obfuscatedSByte2, division);
            Assert.AreEqual(obfuscatedSByte1 / value2, division);
            Assert.AreEqual(value1 / obfuscatedSByte2, division);
            Assert.AreEqual(obfuscatedSByte1 / obfuscatedSByte2, obfuscatedDivision);
            Assert.AreEqual(obfuscatedSByte1 / value2, obfuscatedDivision);
            Assert.AreEqual(value1 / obfuscatedSByte2, obfuscatedDivision);
        }
    }
}