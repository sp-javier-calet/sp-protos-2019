using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedUIntTest
    {
        [Test]
        public void ConversionImplicit()
        {
            uint value = 168;
            var obfuscatedUInt = new ObfuscatedUInt(value);
            uint unobfuscatedUInt = obfuscatedUInt;

            Assert.AreEqual(obfuscatedUInt, obfuscatedUInt);
            Assert.AreEqual(value, unobfuscatedUInt);
            Assert.AreEqual(value, obfuscatedUInt);

            Assert.AreNotEqual(value, obfuscatedUInt.ObfuscatedValue);
        }

        [Test]
        public void ComparisonEqual()
        {
            uint value = 73;
            var obfuscatedUInt = new ObfuscatedUInt(value);

            Assert.AreEqual(obfuscatedUInt, value);
            Assert.AreEqual(value, obfuscatedUInt);
        }

        [Test]
        public void ComparisonNotEqual()
        {
            uint value = 214;
            var obfuscatedUInt = new ObfuscatedUInt(value);

            Assert.AreNotEqual(obfuscatedUInt, 128);
            Assert.AreNotEqual(20, obfuscatedUInt);
        }

        [Test]
        public void ComparisonLess()
        {
            uint value = 64;
            var obfuscatedUInt = new ObfuscatedUInt(value);

            Assert.Less(obfuscatedUInt, 100);
            Assert.Less(3, obfuscatedUInt);
        }

        [Test]
        public void ComparisonLessEqual()
        {
            uint value = 95;
            var obfuscatedUInt = new ObfuscatedUInt(value);

            Assert.LessOrEqual(obfuscatedUInt, 243);
            Assert.LessOrEqual(value, obfuscatedUInt);
        }

        [Test]
        public void ComparisonGreater()
        {
            uint value = 194;
            var obfuscatedUInt = new ObfuscatedUInt(value);

            Assert.Greater(obfuscatedUInt, 156);
            Assert.Greater(255, obfuscatedUInt);
        }

        [Test]
        public void ComparisonGreaterEqual()
        {
            uint value = 137;
            var obfuscatedUInt = new ObfuscatedUInt(value);

            Assert.GreaterOrEqual(obfuscatedUInt, value);
            Assert.GreaterOrEqual(209, obfuscatedUInt);
        }

        [Test]
        public void OperatorAddition()
        {
            uint value1 = 13;
            var obfuscatedUInt1 = new ObfuscatedUInt(value1);

            uint value2 = 7;
            var obfuscatedUInt2 = new ObfuscatedUInt(value2);

            uint addition = (uint)(value1 + value2);
            uint obfuscatedAddition = new ObfuscatedUInt(addition);

            Assert.AreEqual(obfuscatedUInt1 + obfuscatedUInt2, addition);
            Assert.AreEqual(obfuscatedUInt1 + value2, addition);
            Assert.AreEqual(value1 + obfuscatedUInt2, addition);
            Assert.AreEqual(obfuscatedUInt1 + obfuscatedUInt2, obfuscatedAddition);
            Assert.AreEqual(obfuscatedUInt1 + value2, obfuscatedAddition);
            Assert.AreEqual(value1 + obfuscatedUInt2, obfuscatedAddition);

            ++value1;
            ++obfuscatedUInt1;
            Assert.AreEqual(value1, obfuscatedUInt1);
        }

        [Test]
        public void OperatorSubtraction()
        {
            uint value1 = 25;
            var obfuscatedUInt1 = new ObfuscatedUInt(value1);

            uint value2 = 5;
            var obfuscatedUInt2 = new ObfuscatedUInt(value2);

            uint subtraction = (uint)(value1 - value2);
            uint obfuscatedSubtraction = new ObfuscatedUInt(subtraction);

            Assert.AreEqual(obfuscatedUInt1 - obfuscatedUInt2, subtraction);
            Assert.AreEqual(obfuscatedUInt1 - value2, subtraction);
            Assert.AreEqual(value1 - obfuscatedUInt2, subtraction);
            Assert.AreEqual(obfuscatedUInt1 - obfuscatedUInt2, obfuscatedSubtraction);
            Assert.AreEqual(obfuscatedUInt1 - value2, obfuscatedSubtraction);
            Assert.AreEqual(value1 - obfuscatedUInt2, obfuscatedSubtraction);

            --value1;
            --obfuscatedUInt1;
            Assert.AreEqual(value1, obfuscatedUInt1);
        }

        [Test]
        public void OperatorMultiplication()
        {
            uint value1 = 11;
            var obfuscatedUInt1 = new ObfuscatedUInt(value1);

            uint value2 = 6;
            var obfuscatedUInt2 = new ObfuscatedUInt(value2);

            uint multiplication = (uint)(value1 * value2);
            uint obfuscatedMultiplication = new ObfuscatedUInt(multiplication);

            Assert.AreEqual(obfuscatedUInt1 * obfuscatedUInt2, multiplication);
            Assert.AreEqual(obfuscatedUInt1 * value2, multiplication);
            Assert.AreEqual(value1 * obfuscatedUInt2, multiplication);
            Assert.AreEqual(obfuscatedUInt1 * obfuscatedUInt2, obfuscatedMultiplication);
            Assert.AreEqual(obfuscatedUInt1 * value2, obfuscatedMultiplication);
            Assert.AreEqual(value1 * obfuscatedUInt2, obfuscatedMultiplication);
        }

        [Test]
        public void OperatorDivision()
        {
            uint value1 = 80;
            var obfuscatedUInt1 = new ObfuscatedUInt(value1);

            uint value2 = 8;
            var obfuscatedUInt2 = new ObfuscatedUInt(value2);

            uint division = (uint)(value1 / value2);
            uint obfuscatedDivision = new ObfuscatedUInt(division);

            Assert.AreEqual(obfuscatedUInt1 / obfuscatedUInt2, division);
            Assert.AreEqual(obfuscatedUInt1 / value2, division);
            Assert.AreEqual(value1 / obfuscatedUInt2, division);
            Assert.AreEqual(obfuscatedUInt1 / obfuscatedUInt2, obfuscatedDivision);
            Assert.AreEqual(obfuscatedUInt1 / value2, obfuscatedDivision);
            Assert.AreEqual(value1 / obfuscatedUInt2, obfuscatedDivision);
        }
    }
}