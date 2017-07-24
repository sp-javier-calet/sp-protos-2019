using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedULongTest
    {
        [Test]
        public void ConversionImplicit()
        {
            ulong value = 168;
            var obfuscatedULong = new ObfuscatedULong(value);
            ulong unobfuscatedULong = obfuscatedULong;

            Assert.AreEqual(obfuscatedULong, obfuscatedULong);
            Assert.AreEqual(value, unobfuscatedULong);
            Assert.AreEqual(value, obfuscatedULong);

            Assert.AreNotEqual(value, obfuscatedULong.ObfuscatedValue);

            ulong newValue = 99;
            obfuscatedULong = newValue;
            Assert.AreEqual(newValue, obfuscatedULong);
        }

        [Test]
        public void ComparisonEqual()
        {
            ulong value = 73;
            var obfuscatedULong = new ObfuscatedULong(value);

            Assert.AreEqual(obfuscatedULong, value);
            Assert.AreEqual(value, obfuscatedULong);
        }

        [Test]
        public void ComparisonNotEqual()
        {
            ulong value = 214;
            var obfuscatedULong = new ObfuscatedULong(value);

            Assert.AreNotEqual(obfuscatedULong, 128);
            Assert.AreNotEqual(20, obfuscatedULong);
        }

        [Test]
        public void ComparisonLess()
        {
            ulong value = 64;
            var obfuscatedULong = new ObfuscatedULong(value);

            Assert.Less(obfuscatedULong, 100);
            Assert.Less(3, obfuscatedULong);
        }

        [Test]
        public void ComparisonLessEqual()
        {
            ulong value = 95;
            var obfuscatedULong = new ObfuscatedULong(value);

            Assert.LessOrEqual(obfuscatedULong, 243);
            Assert.LessOrEqual(value, obfuscatedULong);
        }

        [Test]
        public void ComparisonGreater()
        {
            ulong value = 194;
            var obfuscatedULong = new ObfuscatedULong(value);

            Assert.Greater(obfuscatedULong, 156);
            Assert.Greater(255, obfuscatedULong);
        }

        [Test]
        public void ComparisonGreaterEqual()
        {
            ulong value = 137;
            var obfuscatedULong = new ObfuscatedULong(value);

            Assert.GreaterOrEqual(obfuscatedULong, value);
            Assert.GreaterOrEqual(209, obfuscatedULong);
        }

        [Test]
        public void OperatorAddition()
        {
            ulong value1 = 13;
            var obfuscatedULong1 = new ObfuscatedULong(value1);

            ulong value2 = 7;
            var obfuscatedULong2 = new ObfuscatedULong(value2);

            ulong addition = (ulong)(value1 + value2);
            ulong obfuscatedAddition = new ObfuscatedULong(addition);

            Assert.AreEqual(obfuscatedULong1 + obfuscatedULong2, addition);
            Assert.AreEqual(obfuscatedULong1 + value2, addition);
            Assert.AreEqual(value1 + obfuscatedULong2, addition);
            Assert.AreEqual(obfuscatedULong1 + obfuscatedULong2, obfuscatedAddition);
            Assert.AreEqual(obfuscatedULong1 + value2, obfuscatedAddition);
            Assert.AreEqual(value1 + obfuscatedULong2, obfuscatedAddition);

            ++value1;
            ++obfuscatedULong1;
            Assert.AreEqual(value1, obfuscatedULong1);
        }

        [Test]
        public void OperatorSubtraction()
        {
            ulong value1 = 25;
            var obfuscatedULong1 = new ObfuscatedULong(value1);

            ulong value2 = 5;
            var obfuscatedULong2 = new ObfuscatedULong(value2);

            ulong subtraction = (ulong)(value1 - value2);
            ulong obfuscatedSubtraction = new ObfuscatedULong(subtraction);

            Assert.AreEqual(obfuscatedULong1 - obfuscatedULong2, subtraction);
            Assert.AreEqual(obfuscatedULong1 - value2, subtraction);
            Assert.AreEqual(value1 - obfuscatedULong2, subtraction);
            Assert.AreEqual(obfuscatedULong1 - obfuscatedULong2, obfuscatedSubtraction);
            Assert.AreEqual(obfuscatedULong1 - value2, obfuscatedSubtraction);
            Assert.AreEqual(value1 - obfuscatedULong2, obfuscatedSubtraction);

            --value1;
            --obfuscatedULong1;
            Assert.AreEqual(value1, obfuscatedULong1);
        }

        [Test]
        public void OperatorMultiplication()
        {
            ulong value1 = 11;
            var obfuscatedULong1 = new ObfuscatedULong(value1);

            ulong value2 = 6;
            var obfuscatedULong2 = new ObfuscatedULong(value2);

            ulong multiplication = (ulong)(value1 * value2);
            ulong obfuscatedMultiplication = new ObfuscatedULong(multiplication);

            Assert.AreEqual(obfuscatedULong1 * obfuscatedULong2, multiplication);
            Assert.AreEqual(obfuscatedULong1 * value2, multiplication);
            Assert.AreEqual(value1 * obfuscatedULong2, multiplication);
            Assert.AreEqual(obfuscatedULong1 * obfuscatedULong2, obfuscatedMultiplication);
            Assert.AreEqual(obfuscatedULong1 * value2, obfuscatedMultiplication);
            Assert.AreEqual(value1 * obfuscatedULong2, obfuscatedMultiplication);
        }

        [Test]
        public void OperatorDivision()
        {
            ulong value1 = 80;
            var obfuscatedULong1 = new ObfuscatedULong(value1);

            ulong value2 = 8;
            var obfuscatedULong2 = new ObfuscatedULong(value2);

            ulong division = (ulong)(value1 / value2);
            ulong obfuscatedDivision = new ObfuscatedULong(division);

            Assert.AreEqual(obfuscatedULong1 / obfuscatedULong2, division);
            Assert.AreEqual(obfuscatedULong1 / value2, division);
            Assert.AreEqual(value1 / obfuscatedULong2, division);
            Assert.AreEqual(obfuscatedULong1 / obfuscatedULong2, obfuscatedDivision);
            Assert.AreEqual(obfuscatedULong1 / value2, obfuscatedDivision);
            Assert.AreEqual(value1 / obfuscatedULong2, obfuscatedDivision);
        }
    }
}