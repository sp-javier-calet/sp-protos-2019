using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedIntTest
    {
        [Test]
        public void ConversionImplicit([Values(68)] int value)
        {
            var obfuscatedInt = new ObfuscatedInt(value);
            int unobfuscatedInt = obfuscatedInt;

            Assert.AreEqual(obfuscatedInt, obfuscatedInt);
            Assert.AreEqual(value, unobfuscatedInt);
            Assert.AreEqual(value, obfuscatedInt);

            Assert.AreNotEqual((ulong)value, obfuscatedInt.ObfuscatedValue);

            int newValue = value + 1;
            obfuscatedInt = newValue;
            Assert.AreEqual(newValue, obfuscatedInt);
        }

        [Test]
        public void ToString([Values(32)] int value)
        {
            var obfuscatedInt = new ObfuscatedInt(value);

            Assert.AreEqual(obfuscatedInt.ToString(), value.ToString());
        }

        [Test]
        public void ComparisonEqual([Values(73)] int value)
        {
            var obfuscatedInt = new ObfuscatedInt(value);

            Assert.AreEqual(obfuscatedInt, value);
            Assert.AreEqual(value, obfuscatedInt);
        }

        [Test]
        public void ComparisonNotEqual([Values(114)] int value)
        {
            var obfuscatedInt = new ObfuscatedInt(value);

            int differentValue = value + 1;
            Assert.AreNotEqual(obfuscatedInt, differentValue);
            Assert.AreNotEqual(differentValue, obfuscatedInt);
        }

        [Test]
        public void ComparisonLess([Values(64)] int value1, [Values(100)] int value2)
        {
            Assert.Less(value1, value2);

            var obfuscatedInt1 = new ObfuscatedInt(value1);
            var obfuscatedInt2 = new ObfuscatedInt(value2);

            Assert.Less(obfuscatedInt1, obfuscatedInt2);
            Assert.Less(obfuscatedInt1, value2);
            Assert.Less(value1, obfuscatedInt2);
        }

        [Test]
        public void ComparisonLessEqual([Values(95, 101)] int value1, [Values(101)] int value2)
        {
            Assert.LessOrEqual(value1, value2);

            var obfuscatedInt1 = new ObfuscatedInt(value1);
            var obfuscatedInt2 = new ObfuscatedInt(value2);

            Assert.LessOrEqual(obfuscatedInt1, obfuscatedInt2);
            Assert.LessOrEqual(obfuscatedInt1, value2);
            Assert.LessOrEqual(value1, obfuscatedInt2);
        }

        [Test]
        public void ComparisonGreater([Values(127)] int value1, [Values(41)] int value2)
        {
            Assert.Greater(value1, value2);

            var obfuscatedInt1 = new ObfuscatedInt(value1);
            var obfuscatedInt2 = new ObfuscatedInt(value2);

            Assert.Greater(obfuscatedInt1, obfuscatedInt2);
            Assert.Greater(obfuscatedInt1, value2);
            Assert.Greater(value1, obfuscatedInt2);
        }

        [Test]
        public void ComparisonGreaterEqual([Values(117, 54)] int value1, [Values(54)] int value2)
        {
            Assert.GreaterOrEqual(value1, value2);

            var obfuscatedInt1 = new ObfuscatedInt(value1);
            var obfuscatedInt2 = new ObfuscatedInt(value2);

            Assert.GreaterOrEqual(obfuscatedInt1, obfuscatedInt2);
            Assert.GreaterOrEqual(obfuscatedInt1, value2);
            Assert.GreaterOrEqual(value1, obfuscatedInt2);
        }

        [Test]
        public void OperatorAddition([Values(13)] int value1, [Values(7)] int value2)
        {
            var obfuscatedInt1 = new ObfuscatedInt(value1);
            var obfuscatedInt2 = new ObfuscatedInt(value2);

            int addition = value1 + value2;
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
        public void OperatorSubtraction([Values(25)] int value1, [Values(5)] int value2)
        {
            var obfuscatedInt1 = new ObfuscatedInt(value1);
            var obfuscatedInt2 = new ObfuscatedInt(value2);

            int subtraction = value1 - value2;
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
        public void OperatorMultiplication([Values(11)] int value1, [Values(6)] int value2)
        {
            var obfuscatedInt1 = new ObfuscatedInt(value1);
            var obfuscatedInt2 = new ObfuscatedInt(value2);

            int multiplication = value1 * value2;
            int obfuscatedMultiplication = new ObfuscatedInt(multiplication);

            Assert.AreEqual(obfuscatedInt1 * obfuscatedInt2, multiplication);
            Assert.AreEqual(obfuscatedInt1 * value2, multiplication);
            Assert.AreEqual(value1 * obfuscatedInt2, multiplication);
            Assert.AreEqual(obfuscatedInt1 * obfuscatedInt2, obfuscatedMultiplication);
            Assert.AreEqual(obfuscatedInt1 * value2, obfuscatedMultiplication);
            Assert.AreEqual(value1 * obfuscatedInt2, obfuscatedMultiplication);
        }

        [Test]
        public void OperatorDivision([Values(80)] int value1, [Values(8)] int value2)
        {
            var obfuscatedInt1 = new ObfuscatedInt(value1);
            var obfuscatedInt2 = new ObfuscatedInt(value2);

            int division = value1 / value2;
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