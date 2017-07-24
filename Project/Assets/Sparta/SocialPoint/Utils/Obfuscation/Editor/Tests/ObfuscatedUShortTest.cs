using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedUShortTest
    {
        [Test]
        public void ConversionImplicit()
        {
            ushort value = 168;
            var obfuscatedUShort = new ObfuscatedUShort(value);
            ushort unobfuscatedUShort = obfuscatedUShort;

            Assert.AreEqual(obfuscatedUShort, obfuscatedUShort);
            Assert.AreEqual(value, unobfuscatedUShort);
            Assert.AreEqual(value, obfuscatedUShort);

            Assert.AreNotEqual(value, obfuscatedUShort.ObfuscatedValue);

            ushort newValue = 99;
            obfuscatedUShort = newValue;
            Assert.AreEqual(newValue, obfuscatedUShort);
        }

        [Test]
        public void ComparisonEqual()
        {
            ushort value = 73;
            var obfuscatedUShort = new ObfuscatedUShort(value);

            Assert.AreEqual(obfuscatedUShort, value);
            Assert.AreEqual(value, obfuscatedUShort);
        }

        [Test]
        public void ComparisonNotEqual()
        {
            ushort value = 214;
            var obfuscatedUShort = new ObfuscatedUShort(value);

            Assert.AreNotEqual(obfuscatedUShort, 128);
            Assert.AreNotEqual(20, obfuscatedUShort);
        }

        [Test]
        public void ComparisonLess()
        {
            ushort value = 64;
            var obfuscatedUShort = new ObfuscatedUShort(value);

            Assert.Less(obfuscatedUShort, 100);
            Assert.Less(3, obfuscatedUShort);
        }

        [Test]
        public void ComparisonLessEqual()
        {
            ushort value = 95;
            var obfuscatedUShort = new ObfuscatedUShort(value);

            Assert.LessOrEqual(obfuscatedUShort, 243);
            Assert.LessOrEqual(value, obfuscatedUShort);
        }

        [Test]
        public void ComparisonGreater()
        {
            ushort value = 194;
            var obfuscatedUShort = new ObfuscatedUShort(value);

            Assert.Greater(obfuscatedUShort, 156);
            Assert.Greater(255, obfuscatedUShort);
        }

        [Test]
        public void ComparisonGreaterEqual()
        {
            ushort value = 137;
            var obfuscatedUShort = new ObfuscatedUShort(value);

            Assert.GreaterOrEqual(obfuscatedUShort, value);
            Assert.GreaterOrEqual(209, obfuscatedUShort);
        }

        [Test]
        public void OperatorAddition()
        {
            ushort value1 = 13;
            var obfuscatedUShort1 = new ObfuscatedUShort(value1);

            ushort value2 = 7;
            var obfuscatedUShort2 = new ObfuscatedUShort(value2);

            ushort addition = (ushort)(value1 + value2);
            ushort obfuscatedAddition = new ObfuscatedUShort(addition);

            Assert.AreEqual(obfuscatedUShort1 + obfuscatedUShort2, addition);
            Assert.AreEqual(obfuscatedUShort1 + value2, addition);
            Assert.AreEqual(value1 + obfuscatedUShort2, addition);
            Assert.AreEqual(obfuscatedUShort1 + obfuscatedUShort2, obfuscatedAddition);
            Assert.AreEqual(obfuscatedUShort1 + value2, obfuscatedAddition);
            Assert.AreEqual(value1 + obfuscatedUShort2, obfuscatedAddition);

            ++value1;
            ++obfuscatedUShort1;
            Assert.AreEqual(value1, obfuscatedUShort1);
        }

        [Test]
        public void OperatorSubtraction()
        {
            ushort value1 = 25;
            var obfuscatedUShort1 = new ObfuscatedUShort(value1);

            ushort value2 = 5;
            var obfuscatedUShort2 = new ObfuscatedUShort(value2);

            ushort subtraction = (ushort)(value1 - value2);
            ushort obfuscatedSubtraction = new ObfuscatedUShort(subtraction);

            Assert.AreEqual(obfuscatedUShort1 - obfuscatedUShort2, subtraction);
            Assert.AreEqual(obfuscatedUShort1 - value2, subtraction);
            Assert.AreEqual(value1 - obfuscatedUShort2, subtraction);
            Assert.AreEqual(obfuscatedUShort1 - obfuscatedUShort2, obfuscatedSubtraction);
            Assert.AreEqual(obfuscatedUShort1 - value2, obfuscatedSubtraction);
            Assert.AreEqual(value1 - obfuscatedUShort2, obfuscatedSubtraction);

            --value1;
            --obfuscatedUShort1;
            Assert.AreEqual(value1, obfuscatedUShort1);
        }

        [Test]
        public void OperatorMultiplication()
        {
            ushort value1 = 11;
            var obfuscatedUShort1 = new ObfuscatedUShort(value1);

            ushort value2 = 6;
            var obfuscatedUShort2 = new ObfuscatedUShort(value2);

            ushort multiplication = (ushort)(value1 * value2);
            ushort obfuscatedMultiplication = new ObfuscatedUShort(multiplication);

            Assert.AreEqual(obfuscatedUShort1 * obfuscatedUShort2, multiplication);
            Assert.AreEqual(obfuscatedUShort1 * value2, multiplication);
            Assert.AreEqual(value1 * obfuscatedUShort2, multiplication);
            Assert.AreEqual(obfuscatedUShort1 * obfuscatedUShort2, obfuscatedMultiplication);
            Assert.AreEqual(obfuscatedUShort1 * value2, obfuscatedMultiplication);
            Assert.AreEqual(value1 * obfuscatedUShort2, obfuscatedMultiplication);
        }

        [Test]
        public void OperatorDivision()
        {
            ushort value1 = 80;
            var obfuscatedUShort1 = new ObfuscatedUShort(value1);

            ushort value2 = 8;
            var obfuscatedUShort2 = new ObfuscatedUShort(value2);

            ushort division = (ushort)(value1 / value2);
            ushort obfuscatedDivision = new ObfuscatedUShort(division);

            Assert.AreEqual(obfuscatedUShort1 / obfuscatedUShort2, division);
            Assert.AreEqual(obfuscatedUShort1 / value2, division);
            Assert.AreEqual(value1 / obfuscatedUShort2, division);
            Assert.AreEqual(obfuscatedUShort1 / obfuscatedUShort2, obfuscatedDivision);
            Assert.AreEqual(obfuscatedUShort1 / value2, obfuscatedDivision);
            Assert.AreEqual(value1 / obfuscatedUShort2, obfuscatedDivision);
        }
    }
}