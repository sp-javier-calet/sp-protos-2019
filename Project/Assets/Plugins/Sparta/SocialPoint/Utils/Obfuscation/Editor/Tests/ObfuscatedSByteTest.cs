using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedSByteTest
    {
        [Test]
        public void ConversionImplicit([Values(68)] sbyte value)
        {
            var obfuscatedSByte = new ObfuscatedSByte(value);
            sbyte unobfuscatedSByte = obfuscatedSByte;

            Assert.AreEqual(obfuscatedSByte, obfuscatedSByte);
            Assert.AreEqual(value, unobfuscatedSByte);
            Assert.AreEqual(value, obfuscatedSByte);

            Assert.AreNotEqual((ulong)value, obfuscatedSByte.ObfuscatedValue);

            sbyte newValue = (sbyte)(value + 1);
            obfuscatedSByte = newValue;
            Assert.AreEqual(newValue, obfuscatedSByte);
        }

        [Test]
        public void ToString([Values(32)] sbyte value)
        {
            var obfuscatedSByte = new ObfuscatedSByte(value);

            Assert.AreEqual(obfuscatedSByte.ToString(), value.ToString());
        }

        [Test]
        public void ComparisonEqual([Values(73)] sbyte value)
        {
            var obfuscatedSByte = new ObfuscatedSByte(value);

            Assert.AreEqual(obfuscatedSByte, value);
            Assert.AreEqual(value, obfuscatedSByte);
        }

        [Test]
        public void ComparisonNotEqual([Values(114)] sbyte value)
        {
            var obfuscatedSByte = new ObfuscatedSByte(value);

            sbyte differentValue = (sbyte)(value + 1);
            Assert.AreNotEqual(obfuscatedSByte, differentValue);
            Assert.AreNotEqual(differentValue, obfuscatedSByte);
        }

        [Test]
        public void ComparisonLess([Values(64)] sbyte value1, [Values(100)] sbyte value2)
        {
            Assert.Less(value1, value2);

            var obfuscatedSByte1 = new ObfuscatedSByte(value1);
            var obfuscatedSByte2 = new ObfuscatedSByte(value2);

            Assert.Less(obfuscatedSByte1, obfuscatedSByte2);
            Assert.Less(obfuscatedSByte1, value2);
            Assert.Less(value1, obfuscatedSByte2);
        }

        [Test]
        public void ComparisonLessEqual([Values(95, 101)] sbyte value1, [Values(101)] sbyte value2)
        {
            Assert.LessOrEqual(value1, value2);

            var obfuscatedSByte1 = new ObfuscatedSByte(value1);
            var obfuscatedSByte2 = new ObfuscatedSByte(value2);

            Assert.LessOrEqual(obfuscatedSByte1, obfuscatedSByte2);
            Assert.LessOrEqual(obfuscatedSByte1, value2);
            Assert.LessOrEqual(value1, obfuscatedSByte2);
        }

        [Test]
        public void ComparisonGreater([Values(127)] sbyte value1, [Values(41)] sbyte value2)
        {
            Assert.Greater(value1, value2);

            var obfuscatedSByte1 = new ObfuscatedSByte(value1);
            var obfuscatedSByte2 = new ObfuscatedSByte(value2);

            Assert.Greater(obfuscatedSByte1, obfuscatedSByte2);
            Assert.Greater(obfuscatedSByte1, value2);
            Assert.Greater(value1, obfuscatedSByte2);
        }

        [Test]
        public void ComparisonGreaterEqual([Values(117, 54)] sbyte value1, [Values(54)] sbyte value2)
        {
            Assert.GreaterOrEqual(value1, value2);

            var obfuscatedSByte1 = new ObfuscatedSByte(value1);
            var obfuscatedSByte2 = new ObfuscatedSByte(value2);

            Assert.GreaterOrEqual(obfuscatedSByte1, obfuscatedSByte2);
            Assert.GreaterOrEqual(obfuscatedSByte1, value2);
            Assert.GreaterOrEqual(value1, obfuscatedSByte2);
        }

        [Test]
        public void OperatorAddition([Values(13)] sbyte value1, [Values(7)] sbyte value2)
        {
            var obfuscatedSByte1 = new ObfuscatedSByte(value1);
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
        public void OperatorSubtraction([Values(25)] sbyte value1, [Values(5)] sbyte value2)
        {
            var obfuscatedSByte1 = new ObfuscatedSByte(value1);
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
        public void OperatorMultiplication([Values(11)] sbyte value1, [Values(6)] sbyte value2)
        {
            var obfuscatedSByte1 = new ObfuscatedSByte(value1);
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
        public void OperatorDivision([Values(80)] sbyte value1, [Values(8)] sbyte value2)
        {
            var obfuscatedSByte1 = new ObfuscatedSByte(value1);
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