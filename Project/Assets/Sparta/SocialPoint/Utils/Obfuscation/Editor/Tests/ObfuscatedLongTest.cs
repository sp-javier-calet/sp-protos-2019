using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedLongTest
    {
        [Test]
        public void ConversionImplicit([Values(68)] long value)
        {
            var obfuscatedLong = new ObfuscatedLong(value);
            long unobfuscatedLong = obfuscatedLong;

            Assert.AreEqual(obfuscatedLong, obfuscatedLong);
            Assert.AreEqual(value, unobfuscatedLong);
            Assert.AreEqual(value, obfuscatedLong);

            Assert.AreNotEqual((ulong)value, obfuscatedLong.ObfuscatedValue);

            long newValue = value + 1;
            obfuscatedLong = newValue;
            Assert.AreEqual(newValue, obfuscatedLong);
        }

        [Test]
        public void ToString([Values(32)] long value)
        {
            var obfuscatedLong = new ObfuscatedLong(value);

            Assert.AreEqual(obfuscatedLong.ToString(), value.ToString());
        }

        [Test]
        public void ComparisonEqual([Values(73)] long value)
        {
            var obfuscatedLong = new ObfuscatedLong(value);

            Assert.AreEqual(obfuscatedLong, value);
            Assert.AreEqual(value, obfuscatedLong);
        }

        [Test]
        public void ComparisonNotEqual([Values(114)] long value)
        {
            var obfuscatedLong = new ObfuscatedLong(value);

            long differentValue = value + 1;
            Assert.AreNotEqual(obfuscatedLong, differentValue);
            Assert.AreNotEqual(differentValue, obfuscatedLong);
        }

        [Test]
        public void ComparisonLess([Values(64)] long value1, [Values(100)] long value2)
        {
            Assert.Less(value1, value2);

            var obfuscatedLong1 = new ObfuscatedLong(value1);
            var obfuscatedLong2 = new ObfuscatedLong(value2);

            Assert.Less(obfuscatedLong1, obfuscatedLong2);
            Assert.Less(obfuscatedLong1, value2);
            Assert.Less(value1, obfuscatedLong2);
        }

        [Test]
        public void ComparisonLessEqual([Values(95, 101)] long value1, [Values(101)] long value2)
        {
            Assert.LessOrEqual(value1, value2);

            var obfuscatedLong1 = new ObfuscatedLong(value1);
            var obfuscatedLong2 = new ObfuscatedLong(value2);

            Assert.LessOrEqual(obfuscatedLong1, obfuscatedLong2);
            Assert.LessOrEqual(obfuscatedLong1, value2);
            Assert.LessOrEqual(value1, obfuscatedLong2);
        }

        [Test]
        public void ComparisonGreater([Values(127)] long value1, [Values(41)] long value2)
        {
            Assert.Greater(value1, value2);

            var obfuscatedLong1 = new ObfuscatedLong(value1);
            var obfuscatedLong2 = new ObfuscatedLong(value2);

            Assert.Greater(obfuscatedLong1, obfuscatedLong2);
            Assert.Greater(obfuscatedLong1, value2);
            Assert.Greater(value1, obfuscatedLong2);
        }

        [Test]
        public void ComparisonGreaterEqual([Values(117, 54)] long value1, [Values(54)] long value2)
        {
            Assert.GreaterOrEqual(value1, value2);

            var obfuscatedLong1 = new ObfuscatedLong(value1);
            var obfuscatedLong2 = new ObfuscatedLong(value2);

            Assert.GreaterOrEqual(obfuscatedLong1, obfuscatedLong2);
            Assert.GreaterOrEqual(obfuscatedLong1, value2);
            Assert.GreaterOrEqual(value1, obfuscatedLong2);
        }

        [Test]
        public void OperatorAddition([Values(13)] long value1, [Values(7)] long value2)
        {
            var obfuscatedLong1 = new ObfuscatedLong(value1);
            var obfuscatedLong2 = new ObfuscatedLong(value2);

            long addition = value1 + value2;
            long obfuscatedAddition = new ObfuscatedLong(addition);

            Assert.AreEqual(obfuscatedLong1 + obfuscatedLong2, addition);
            Assert.AreEqual(obfuscatedLong1 + value2, addition);
            Assert.AreEqual(value1 + obfuscatedLong2, addition);
            Assert.AreEqual(obfuscatedLong1 + obfuscatedLong2, obfuscatedAddition);
            Assert.AreEqual(obfuscatedLong1 + value2, obfuscatedAddition);
            Assert.AreEqual(value1 + obfuscatedLong2, obfuscatedAddition);

            ++value1;
            ++obfuscatedLong1;
            Assert.AreEqual(value1, obfuscatedLong1);
        }

        [Test]
        public void OperatorSubtraction([Values(25)] long value1, [Values(5)] long value2)
        {
            var obfuscatedLong1 = new ObfuscatedLong(value1);
            var obfuscatedLong2 = new ObfuscatedLong(value2);

            long subtraction = value1 - value2;
            long obfuscatedSubtraction = new ObfuscatedLong(subtraction);

            Assert.AreEqual(obfuscatedLong1 - obfuscatedLong2, subtraction);
            Assert.AreEqual(obfuscatedLong1 - value2, subtraction);
            Assert.AreEqual(value1 - obfuscatedLong2, subtraction);
            Assert.AreEqual(obfuscatedLong1 - obfuscatedLong2, obfuscatedSubtraction);
            Assert.AreEqual(obfuscatedLong1 - value2, obfuscatedSubtraction);
            Assert.AreEqual(value1 - obfuscatedLong2, obfuscatedSubtraction);

            --value1;
            --obfuscatedLong1;
            Assert.AreEqual(value1, obfuscatedLong1);
        }

        [Test]
        public void OperatorMultiplication([Values(11)] long value1, [Values(6)] long value2)
        {
            var obfuscatedLong1 = new ObfuscatedLong(value1);
            var obfuscatedLong2 = new ObfuscatedLong(value2);

            long multiplication = value1 * value2;
            long obfuscatedMultiplication = new ObfuscatedLong(multiplication);

            Assert.AreEqual(obfuscatedLong1 * obfuscatedLong2, multiplication);
            Assert.AreEqual(obfuscatedLong1 * value2, multiplication);
            Assert.AreEqual(value1 * obfuscatedLong2, multiplication);
            Assert.AreEqual(obfuscatedLong1 * obfuscatedLong2, obfuscatedMultiplication);
            Assert.AreEqual(obfuscatedLong1 * value2, obfuscatedMultiplication);
            Assert.AreEqual(value1 * obfuscatedLong2, obfuscatedMultiplication);
        }

        [Test]
        public void OperatorDivision([Values(80)] long value1, [Values(8)] long value2)
        {
            var obfuscatedLong1 = new ObfuscatedLong(value1);
            var obfuscatedLong2 = new ObfuscatedLong(value2);

            long division = value1 / value2;
            long obfuscatedDivision = new ObfuscatedLong(division);

            Assert.AreEqual(obfuscatedLong1 / obfuscatedLong2, division);
            Assert.AreEqual(obfuscatedLong1 / value2, division);
            Assert.AreEqual(value1 / obfuscatedLong2, division);
            Assert.AreEqual(obfuscatedLong1 / obfuscatedLong2, obfuscatedDivision);
            Assert.AreEqual(obfuscatedLong1 / value2, obfuscatedDivision);
            Assert.AreEqual(value1 / obfuscatedLong2, obfuscatedDivision);
        }
    }
}