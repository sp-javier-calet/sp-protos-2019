using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedShortTest
    {
        [Test]
        public void ConversionImplicit()
        {
            short value = 168;
            var obfuscatedShort = new ObfuscatedShort(value);
            short unobfuscatedShort = obfuscatedShort;

            Assert.AreEqual(obfuscatedShort, obfuscatedShort);
            Assert.AreEqual(value, unobfuscatedShort);
            Assert.AreEqual(value, obfuscatedShort);

            Assert.AreNotEqual(value, obfuscatedShort.ObfuscatedValue);
        }

        [Test]
        public void ComparisonEqual()
        {
            short value = 73;
            var obfuscatedShort = new ObfuscatedShort(value);

            Assert.AreEqual(obfuscatedShort, value);
            Assert.AreEqual(value, obfuscatedShort);
        }

        [Test]
        public void ComparisonNotEqual()
        {
            short value = 214;
            var obfuscatedShort = new ObfuscatedShort(value);

            Assert.AreNotEqual(obfuscatedShort, 128);
            Assert.AreNotEqual(20, obfuscatedShort);
        }

        [Test]
        public void ComparisonLess()
        {
            short value = 64;
            var obfuscatedShort = new ObfuscatedShort(value);

            Assert.Less(obfuscatedShort, 100);
            Assert.Less(3, obfuscatedShort);
        }

        [Test]
        public void ComparisonLessEqual()
        {
            short value = 95;
            var obfuscatedShort = new ObfuscatedShort(value);

            Assert.LessOrEqual(obfuscatedShort, 243);
            Assert.LessOrEqual(value, obfuscatedShort);
        }

        [Test]
        public void ComparisonGreater()
        {
            short value = 194;
            var obfuscatedShort = new ObfuscatedShort(value);

            Assert.Greater(obfuscatedShort, 156);
            Assert.Greater(255, obfuscatedShort);
        }

        [Test]
        public void ComparisonGreaterEqual()
        {
            short value = 137;
            var obfuscatedShort = new ObfuscatedShort(value);

            Assert.GreaterOrEqual(obfuscatedShort, value);
            Assert.GreaterOrEqual(209, obfuscatedShort);
        }

        [Test]
        public void OperatorAddition()
        {
            short value1 = 13;
            var obfuscatedShort1 = new ObfuscatedShort(value1);

            short value2 = 7;
            var obfuscatedShort2 = new ObfuscatedShort(value2);

            short addition = (short)(value1 + value2);
            short obfuscatedAddition = new ObfuscatedShort(addition);

            Assert.AreEqual(obfuscatedShort1 + obfuscatedShort2, addition);
            Assert.AreEqual(obfuscatedShort1 + value2, addition);
            Assert.AreEqual(value1 + obfuscatedShort2, addition);
            Assert.AreEqual(obfuscatedShort1 + obfuscatedShort2, obfuscatedAddition);
            Assert.AreEqual(obfuscatedShort1 + value2, obfuscatedAddition);
            Assert.AreEqual(value1 + obfuscatedShort2, obfuscatedAddition);

            ++value1;
            ++obfuscatedShort1;
            Assert.AreEqual(value1, obfuscatedShort1);
        }

        [Test]
        public void OperatorSubtraction()
        {
            short value1 = 25;
            var obfuscatedShort1 = new ObfuscatedShort(value1);

            short value2 = 5;
            var obfuscatedShort2 = new ObfuscatedShort(value2);

            short subtraction = (short)(value1 - value2);
            short obfuscatedSubtraction = new ObfuscatedShort(subtraction);

            Assert.AreEqual(obfuscatedShort1 - obfuscatedShort2, subtraction);
            Assert.AreEqual(obfuscatedShort1 - value2, subtraction);
            Assert.AreEqual(value1 - obfuscatedShort2, subtraction);
            Assert.AreEqual(obfuscatedShort1 - obfuscatedShort2, obfuscatedSubtraction);
            Assert.AreEqual(obfuscatedShort1 - value2, obfuscatedSubtraction);
            Assert.AreEqual(value1 - obfuscatedShort2, obfuscatedSubtraction);

            --value1;
            --obfuscatedShort1;
            Assert.AreEqual(value1, obfuscatedShort1);
        }

        [Test]
        public void OperatorMultiplication()
        {
            short value1 = 11;
            var obfuscatedShort1 = new ObfuscatedShort(value1);

            short value2 = 6;
            var obfuscatedShort2 = new ObfuscatedShort(value2);

            short multiplication = (short)(value1 * value2);
            short obfuscatedMultiplication = new ObfuscatedShort(multiplication);

            Assert.AreEqual(obfuscatedShort1 * obfuscatedShort2, multiplication);
            Assert.AreEqual(obfuscatedShort1 * value2, multiplication);
            Assert.AreEqual(value1 * obfuscatedShort2, multiplication);
            Assert.AreEqual(obfuscatedShort1 * obfuscatedShort2, obfuscatedMultiplication);
            Assert.AreEqual(obfuscatedShort1 * value2, obfuscatedMultiplication);
            Assert.AreEqual(value1 * obfuscatedShort2, obfuscatedMultiplication);
        }

        [Test]
        public void OperatorDivision()
        {
            short value1 = 80;
            var obfuscatedShort1 = new ObfuscatedShort(value1);

            short value2 = 8;
            var obfuscatedShort2 = new ObfuscatedShort(value2);

            short division = (short)(value1 / value2);
            short obfuscatedDivision = new ObfuscatedShort(division);

            Assert.AreEqual(obfuscatedShort1 / obfuscatedShort2, division);
            Assert.AreEqual(obfuscatedShort1 / value2, division);
            Assert.AreEqual(value1 / obfuscatedShort2, division);
            Assert.AreEqual(obfuscatedShort1 / obfuscatedShort2, obfuscatedDivision);
            Assert.AreEqual(obfuscatedShort1 / value2, obfuscatedDivision);
            Assert.AreEqual(value1 / obfuscatedShort2, obfuscatedDivision);
        }
    }
}