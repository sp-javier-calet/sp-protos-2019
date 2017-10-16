using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedULongTest
    {
        [Test]
        public void ConversionImplicit([Values((ulong)68)] ulong value)
        {
            var obfuscatedULong = new ObfuscatedULong(value);
            ulong unobfuscatedULong = obfuscatedULong;

            Assert.AreEqual(obfuscatedULong, obfuscatedULong);
            Assert.AreEqual(value, unobfuscatedULong);
            Assert.AreEqual(value, obfuscatedULong);

            Assert.AreNotEqual(value, obfuscatedULong.ObfuscatedValue);

            ulong newValue = value + 1;
            obfuscatedULong = newValue;
            Assert.AreEqual(newValue, obfuscatedULong);
        }

        [Test]
        public void ToString([Values((ulong)32)] ulong value)
        {
            var obfuscatedULong = new ObfuscatedULong(value);

            Assert.AreEqual(obfuscatedULong.ToString(), value.ToString());
        }

        [Test]
        public void ComparisonEqual([Values((ulong)73)] ulong value)
        {
            var obfuscatedULong = new ObfuscatedULong(value);

            Assert.AreEqual(obfuscatedULong, value);
            Assert.AreEqual(value, obfuscatedULong);
        }

        [Test]
        public void ComparisonNotEqual([Values((ulong)114)] ulong value)
        {
            var obfuscatedULong = new ObfuscatedULong(value);

            ulong differentValue = value + 1;
            Assert.AreNotEqual(obfuscatedULong, differentValue);
            Assert.AreNotEqual(differentValue, obfuscatedULong);
        }

        [Test]
        public void ComparisonLess([Values((ulong)64)] ulong value1, [Values((ulong)100)] ulong value2)
        {
            Assert.Less(value1, value2);

            var obfuscatedULong1 = new ObfuscatedULong(value1);
            var obfuscatedULong2 = new ObfuscatedULong(value2);

            Assert.Less(obfuscatedULong1, obfuscatedULong2);
            Assert.Less(obfuscatedULong1, value2);
            Assert.Less(value1, obfuscatedULong2);
        }

        [Test]
        public void ComparisonLessEqual([Values((ulong)95, (ulong)101)] ulong value1, [Values((ulong)101)] ulong value2)
        {
            Assert.LessOrEqual(value1, value2);

            var obfuscatedULong1 = new ObfuscatedULong(value1);
            var obfuscatedULong2 = new ObfuscatedULong(value2);

            Assert.LessOrEqual(obfuscatedULong1, obfuscatedULong2);
            Assert.LessOrEqual(obfuscatedULong1, value2);
            Assert.LessOrEqual(value1, obfuscatedULong2);
        }

        [Test]
        public void ComparisonGreater([Values((ulong)127)] ulong value1, [Values((ulong)41)] ulong value2)
        {
            Assert.Greater(value1, value2);

            var obfuscatedULong1 = new ObfuscatedULong(value1);
            var obfuscatedULong2 = new ObfuscatedULong(value2);

            Assert.Greater(obfuscatedULong1, obfuscatedULong2);
            Assert.Greater(obfuscatedULong1, value2);
            Assert.Greater(value1, obfuscatedULong2);
        }

        [Test]
        public void ComparisonGreaterEqual([Values((ulong)117, (ulong)54)] ulong value1, [Values((ulong)54)] ulong value2)
        {
            Assert.GreaterOrEqual(value1, value2);

            var obfuscatedULong1 = new ObfuscatedULong(value1);
            var obfuscatedULong2 = new ObfuscatedULong(value2);

            Assert.GreaterOrEqual(obfuscatedULong1, obfuscatedULong2);
            Assert.GreaterOrEqual(obfuscatedULong1, value2);
            Assert.GreaterOrEqual(value1, obfuscatedULong2);
        }

        [Test]
        public void OperatorAddition([Values((ulong)13)] ulong value1, [Values((ulong)7)] ulong value2)
        {
            var obfuscatedULong1 = new ObfuscatedULong(value1);
            var obfuscatedULong2 = new ObfuscatedULong(value2);

            ulong addition = value1 + value2;
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
        public void OperatorSubtraction([Values((ulong)25)] ulong value1, [Values((ulong)5)] ulong value2)
        {
            var obfuscatedULong1 = new ObfuscatedULong(value1);
            var obfuscatedULong2 = new ObfuscatedULong(value2);

            ulong subtraction = value1 - value2;
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
        public void OperatorMultiplication([Values((ulong)11)] ulong value1, [Values((ulong)6)] ulong value2)
        {
            var obfuscatedULong1 = new ObfuscatedULong(value1);
            var obfuscatedULong2 = new ObfuscatedULong(value2);

            ulong multiplication = value1 * value2;
            ulong obfuscatedMultiplication = new ObfuscatedULong(multiplication);

            Assert.AreEqual(obfuscatedULong1 * obfuscatedULong2, multiplication);
            Assert.AreEqual(obfuscatedULong1 * value2, multiplication);
            Assert.AreEqual(value1 * obfuscatedULong2, multiplication);
            Assert.AreEqual(obfuscatedULong1 * obfuscatedULong2, obfuscatedMultiplication);
            Assert.AreEqual(obfuscatedULong1 * value2, obfuscatedMultiplication);
            Assert.AreEqual(value1 * obfuscatedULong2, obfuscatedMultiplication);
        }

        [Test]
        public void OperatorDivision([Values((ulong)80)] ulong value1, [Values((ulong)8)] ulong value2)
        {
            var obfuscatedULong1 = new ObfuscatedULong(value1);
            var obfuscatedULong2 = new ObfuscatedULong(value2);

            ulong division = value1 / value2;
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