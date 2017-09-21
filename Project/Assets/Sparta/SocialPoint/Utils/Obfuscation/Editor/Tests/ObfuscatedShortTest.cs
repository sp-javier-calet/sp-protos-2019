using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedShortTest
    {
        [Test]
        public void ConversionImplicit([Values(68)] short value)
        {
            var obfuscatedShort = new ObfuscatedShort(value);
            short unobfuscatedShort = obfuscatedShort;

            Assert.AreEqual(obfuscatedShort, obfuscatedShort);
            Assert.AreEqual(value, unobfuscatedShort);
            Assert.AreEqual(value, obfuscatedShort);

            Assert.AreNotEqual((ulong)value, obfuscatedShort.ObfuscatedValue);

            short newValue = (short)(value + 1);
            obfuscatedShort = newValue;
            Assert.AreEqual(newValue, obfuscatedShort);
        }

        [Test]
        public void ToString([Values(32)] short value)
        {
            var obfuscatedShort = new ObfuscatedShort(value);

            Assert.AreEqual(obfuscatedShort.ToString(), value.ToString());
        }

        [Test]
        public void ComparisonEqual([Values(73)] short value)
        {
            var obfuscatedShort = new ObfuscatedShort(value);

            Assert.AreEqual(obfuscatedShort, value);
            Assert.AreEqual(value, obfuscatedShort);
        }

        [Test]
        public void ComparisonNotEqual([Values(114)] short value)
        {
            var obfuscatedShort = new ObfuscatedShort(value);

            short differentValue = (short)(value + 1);
            Assert.AreNotEqual(obfuscatedShort, differentValue);
            Assert.AreNotEqual(differentValue, obfuscatedShort);
        }

        [Test]
        public void ComparisonLess([Values(64)] short value1, [Values(100)] short value2)
        {
            Assert.Less(value1, value2);

            var obfuscatedShort1 = new ObfuscatedShort(value1);
            var obfuscatedShort2 = new ObfuscatedShort(value2);

            Assert.Less(obfuscatedShort1, obfuscatedShort2);
            Assert.Less(obfuscatedShort1, value2);
            Assert.Less(value1, obfuscatedShort2);
        }

        [Test]
        public void ComparisonLessEqual([Values(95, 101)] short value1, [Values(101)] short value2)
        {
            Assert.LessOrEqual(value1, value2);

            var obfuscatedShort1 = new ObfuscatedShort(value1);
            var obfuscatedShort2 = new ObfuscatedShort(value2);

            Assert.LessOrEqual(obfuscatedShort1, obfuscatedShort2);
            Assert.LessOrEqual(obfuscatedShort1, value2);
            Assert.LessOrEqual(value1, obfuscatedShort2);
        }

        [Test]
        public void ComparisonGreater([Values(127)] short value1, [Values(41)] short value2)
        {
            Assert.Greater(value1, value2);

            var obfuscatedShort1 = new ObfuscatedShort(value1);
            var obfuscatedShort2 = new ObfuscatedShort(value2);

            Assert.Greater(obfuscatedShort1, obfuscatedShort2);
            Assert.Greater(obfuscatedShort1, value2);
            Assert.Greater(value1, obfuscatedShort2);
        }

        [Test]
        public void ComparisonGreaterEqual([Values(117, 54)] short value1, [Values(54)] short value2)
        {
            Assert.GreaterOrEqual(value1, value2);

            var obfuscatedShort1 = new ObfuscatedShort(value1);
            var obfuscatedShort2 = new ObfuscatedShort(value2);

            Assert.GreaterOrEqual(obfuscatedShort1, obfuscatedShort2);
            Assert.GreaterOrEqual(obfuscatedShort1, value2);
            Assert.GreaterOrEqual(value1, obfuscatedShort2);
        }

        [Test]
        public void OperatorAddition([Values(13)] short value1, [Values(7)] short value2)
        {
            var obfuscatedShort1 = new ObfuscatedShort(value1);
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
        public void OperatorSubtraction([Values(25)] short value1, [Values(5)] short value2)
        {
            var obfuscatedShort1 = new ObfuscatedShort(value1);
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
        public void OperatorMultiplication([Values(11)] short value1, [Values(6)] short value2)
        {
            var obfuscatedShort1 = new ObfuscatedShort(value1);
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
        public void OperatorDivision([Values(80)] short value1, [Values(8)] short value2)
        {
            var obfuscatedShort1 = new ObfuscatedShort(value1);
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