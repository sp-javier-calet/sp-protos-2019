using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedLongTest
    {
        [Test]
        public void ConversionImplicit()
        {
            long value = 168;
            var obfuscatedLong = new ObfuscatedLong(value);
            long unobfuscatedLong = obfuscatedLong;

            Assert.AreEqual(obfuscatedLong, obfuscatedLong);
            Assert.AreEqual(value, unobfuscatedLong);
            Assert.AreEqual(value, obfuscatedLong);

            Assert.AreNotEqual(value, obfuscatedLong.ObfuscatedValue);
        }

        [Test]
        public void ComparisonEqual()
        {
            long value = 73;
            var obfuscatedLong = new ObfuscatedLong(value);

            Assert.AreEqual(obfuscatedLong, value);
            Assert.AreEqual(value, obfuscatedLong);
        }

        [Test]
        public void ComparisonNotEqual()
        {
            long value = 214;
            var obfuscatedLong = new ObfuscatedLong(value);

            Assert.AreNotEqual(obfuscatedLong, 128);
            Assert.AreNotEqual(20, obfuscatedLong);
        }

        [Test]
        public void ComparisonLess()
        {
            long value = 64;
            var obfuscatedLong = new ObfuscatedLong(value);

            Assert.Less(obfuscatedLong, 100);
            Assert.Less(3, obfuscatedLong);
        }

        [Test]
        public void ComparisonLessEqual()
        {
            long value = 95;
            var obfuscatedLong = new ObfuscatedLong(value);

            Assert.LessOrEqual(obfuscatedLong, 243);
            Assert.LessOrEqual(value, obfuscatedLong);
        }

        [Test]
        public void ComparisonGreater()
        {
            long value = 194;
            var obfuscatedLong = new ObfuscatedLong(value);

            Assert.Greater(obfuscatedLong, 156);
            Assert.Greater(255, obfuscatedLong);
        }

        [Test]
        public void ComparisonGreaterEqual()
        {
            long value = 137;
            var obfuscatedLong = new ObfuscatedLong(value);

            Assert.GreaterOrEqual(obfuscatedLong, value);
            Assert.GreaterOrEqual(209, obfuscatedLong);
        }

        [Test]
        public void OperatorAddition()
        {
            long value1 = 13;
            var obfuscatedLong1 = new ObfuscatedLong(value1);

            long value2 = 7;
            var obfuscatedLong2 = new ObfuscatedLong(value2);

            long addition = (long)(value1 + value2);
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
        public void OperatorSubtraction()
        {
            long value1 = 25;
            var obfuscatedLong1 = new ObfuscatedLong(value1);

            long value2 = 5;
            var obfuscatedLong2 = new ObfuscatedLong(value2);

            long subtraction = (long)(value1 - value2);
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
        public void OperatorMultiplication()
        {
            long value1 = 11;
            var obfuscatedLong1 = new ObfuscatedLong(value1);

            long value2 = 6;
            var obfuscatedLong2 = new ObfuscatedLong(value2);

            long multiplication = (long)(value1 * value2);
            long obfuscatedMultiplication = new ObfuscatedLong(multiplication);

            Assert.AreEqual(obfuscatedLong1 * obfuscatedLong2, multiplication);
            Assert.AreEqual(obfuscatedLong1 * value2, multiplication);
            Assert.AreEqual(value1 * obfuscatedLong2, multiplication);
            Assert.AreEqual(obfuscatedLong1 * obfuscatedLong2, obfuscatedMultiplication);
            Assert.AreEqual(obfuscatedLong1 * value2, obfuscatedMultiplication);
            Assert.AreEqual(value1 * obfuscatedLong2, obfuscatedMultiplication);
        }

        [Test]
        public void OperatorDivision()
        {
            long value1 = 80;
            var obfuscatedLong1 = new ObfuscatedLong(value1);

            long value2 = 8;
            var obfuscatedLong2 = new ObfuscatedLong(value2);

            long division = (long)(value1 / value2);
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