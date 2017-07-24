using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedIntTest
    {
        [Test]
        public void ConversionImplicit()
        {
            int value = 168;
            var obfuscatedInt = new ObfuscatedInt(value);
            int unobfuscatedInt = obfuscatedInt;

            Assert.AreEqual(obfuscatedInt, obfuscatedInt);
            Assert.AreEqual(value, unobfuscatedInt);
            Assert.AreEqual(value, obfuscatedInt);

            Assert.AreNotEqual(value, obfuscatedInt.ObfuscatedValue);

            int newValue = 99;
            obfuscatedInt = newValue;
            Assert.AreEqual(newValue, obfuscatedInt);
        }

        [Test]
        public void ComparisonEqual()
        {
            int value = 73;
            var obfuscatedInt = new ObfuscatedInt(value);

            Assert.AreEqual(obfuscatedInt, value);
            Assert.AreEqual(value, obfuscatedInt);
        }

        [Test]
        public void ComparisonNotEqual()
        {
            int value = 214;
            var obfuscatedInt = new ObfuscatedInt(value);

            Assert.AreNotEqual(obfuscatedInt, 128);
            Assert.AreNotEqual(20, obfuscatedInt);
        }

        [Test]
        public void ComparisonLess()
        {
            int value = 64;
            var obfuscatedInt = new ObfuscatedInt(value);

            Assert.Less(obfuscatedInt, 100);
            Assert.Less(3, obfuscatedInt);
        }

        [Test]
        public void ComparisonLessEqual()
        {
            int value = 95;
            var obfuscatedInt = new ObfuscatedInt(value);

            Assert.LessOrEqual(obfuscatedInt, 243);
            Assert.LessOrEqual(value, obfuscatedInt);
        }

        [Test]
        public void ComparisonGreater()
        {
            int value = 194;
            var obfuscatedInt = new ObfuscatedInt(value);

            Assert.Greater(obfuscatedInt, 156);
            Assert.Greater(255, obfuscatedInt);
        }

        [Test]
        public void ComparisonGreaterEqual()
        {
            int value = 137;
            var obfuscatedInt = new ObfuscatedInt(value);

            Assert.GreaterOrEqual(obfuscatedInt, value);
            Assert.GreaterOrEqual(209, obfuscatedInt);
        }

        [Test]
        public void OperatorAddition()
        {
            int value1 = 13;
            var obfuscatedInt1 = new ObfuscatedInt(value1);

            int value2 = 7;
            var obfuscatedInt2 = new ObfuscatedInt(value2);

            int addition = (int)(value1 + value2);
            int obfuscatedAddition = new ObfuscatedInt(addition);

            Assert.AreEqual(obfuscatedInt1 + obfuscatedInt2, addition);
            Assert.AreEqual(obfuscatedInt1 + value2, addition);
            Assert.AreEqual(value1 + obfuscatedInt2, addition);
            Assert.AreEqual(obfuscatedInt1 + obfuscatedInt2, obfuscatedAddition);
            Assert.AreEqual(obfuscatedInt1 + value2, obfuscatedAddition);
            Assert.AreEqual(value1 + obfuscatedInt2, obfuscatedAddition);

            ++value1;
            ++obfuscatedInt1;
            Assert.AreEqual(value1, obfuscatedInt1);
        }

        [Test]
        public void OperatorSubtraction()
        {
            int value1 = 25;
            var obfuscatedInt1 = new ObfuscatedInt(value1);

            int value2 = 5;
            var obfuscatedInt2 = new ObfuscatedInt(value2);

            int subtraction = (int)(value1 - value2);
            int obfuscatedSubtraction = new ObfuscatedInt(subtraction);

            Assert.AreEqual(obfuscatedInt1 - obfuscatedInt2, subtraction);
            Assert.AreEqual(obfuscatedInt1 - value2, subtraction);
            Assert.AreEqual(value1 - obfuscatedInt2, subtraction);
            Assert.AreEqual(obfuscatedInt1 - obfuscatedInt2, obfuscatedSubtraction);
            Assert.AreEqual(obfuscatedInt1 - value2, obfuscatedSubtraction);
            Assert.AreEqual(value1 - obfuscatedInt2, obfuscatedSubtraction);

            --value1;
            --obfuscatedInt1;
            Assert.AreEqual(value1, obfuscatedInt1);
        }

        [Test]
        public void OperatorMultiplication()
        {
            int value1 = 11;
            var obfuscatedInt1 = new ObfuscatedInt(value1);

            int value2 = 6;
            var obfuscatedInt2 = new ObfuscatedInt(value2);

            int multiplication = (int)(value1 * value2);
            int obfuscatedMultiplication = new ObfuscatedInt(multiplication);

            Assert.AreEqual(obfuscatedInt1 * obfuscatedInt2, multiplication);
            Assert.AreEqual(obfuscatedInt1 * value2, multiplication);
            Assert.AreEqual(value1 * obfuscatedInt2, multiplication);
            Assert.AreEqual(obfuscatedInt1 * obfuscatedInt2, obfuscatedMultiplication);
            Assert.AreEqual(obfuscatedInt1 * value2, obfuscatedMultiplication);
            Assert.AreEqual(value1 * obfuscatedInt2, obfuscatedMultiplication);
        }

        [Test]
        public void OperatorDivision()
        {
            int value1 = 80;
            var obfuscatedInt1 = new ObfuscatedInt(value1);

            int value2 = 8;
            var obfuscatedInt2 = new ObfuscatedInt(value2);

            int division = (int)(value1 / value2);
            int obfuscatedDivision = new ObfuscatedInt(division);

            Assert.AreEqual(obfuscatedInt1 / obfuscatedInt2, division);
            Assert.AreEqual(obfuscatedInt1 / value2, division);
            Assert.AreEqual(value1 / obfuscatedInt2, division);
            Assert.AreEqual(obfuscatedInt1 / obfuscatedInt2, obfuscatedDivision);
            Assert.AreEqual(obfuscatedInt1 / value2, obfuscatedDivision);
            Assert.AreEqual(value1 / obfuscatedInt2, obfuscatedDivision);
        }
    }
}