using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedULongTest
    {
        static bool AreObfuscatedEqual(ulong value1, ulong value2)
        {
            return value2 == value1 &&
                value2.Equals(value1) &&
                value1 == value2 &&
                value1.Equals(value2);
        }

        [Test]
        public void ConversionImplicit([Values((ulong)68)] ulong value)
        {
            var obfuscatedULong = new ObfuscatedULong(value);
            ulong unobfuscatedULong = obfuscatedULong;

            Assert.That(AreObfuscatedEqual(obfuscatedULong, obfuscatedULong));
            Assert.That(AreObfuscatedEqual(value, unobfuscatedULong));
            Assert.That(AreObfuscatedEqual(value, obfuscatedULong));

            Assert.AreNotEqual(value, obfuscatedULong.ObfuscatedValue);

            ulong newValue = value + 1;
            obfuscatedULong = newValue;
            Assert.That(AreObfuscatedEqual(newValue, obfuscatedULong));
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

            Assert.That(AreObfuscatedEqual(obfuscatedULong, value));
            Assert.That(AreObfuscatedEqual(value, obfuscatedULong));
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

            Assert.That(AreObfuscatedEqual(obfuscatedULong1 + obfuscatedULong2, addition));
            Assert.That(AreObfuscatedEqual(obfuscatedULong1 + value2, addition));
            Assert.That(AreObfuscatedEqual(value1 + obfuscatedULong2, addition));
            Assert.That(AreObfuscatedEqual(obfuscatedULong1 + obfuscatedULong2, obfuscatedAddition));
            Assert.That(AreObfuscatedEqual(obfuscatedULong1 + value2, obfuscatedAddition));
            Assert.That(AreObfuscatedEqual(value1 + obfuscatedULong2, obfuscatedAddition));

            ++value1;
            ++obfuscatedULong1;
            Assert.That(AreObfuscatedEqual(value1, obfuscatedULong1));
        }

        [Test]
        public void OperatorSubtraction([Values((ulong)25)] ulong value1, [Values((ulong)5)] ulong value2)
        {
            var obfuscatedULong1 = new ObfuscatedULong(value1);
            var obfuscatedULong2 = new ObfuscatedULong(value2);

            ulong subtraction = value1 - value2;
            ulong obfuscatedSubtraction = new ObfuscatedULong(subtraction);

            Assert.That(AreObfuscatedEqual(obfuscatedULong1 - obfuscatedULong2, subtraction));
            Assert.That(AreObfuscatedEqual(obfuscatedULong1 - value2, subtraction));
            Assert.That(AreObfuscatedEqual(value1 - obfuscatedULong2, subtraction));
            Assert.That(AreObfuscatedEqual(obfuscatedULong1 - obfuscatedULong2, obfuscatedSubtraction));
            Assert.That(AreObfuscatedEqual(obfuscatedULong1 - value2, obfuscatedSubtraction));
            Assert.That(AreObfuscatedEqual(value1 - obfuscatedULong2, obfuscatedSubtraction));

            --value1;
            --obfuscatedULong1;
            Assert.That(AreObfuscatedEqual(value1, obfuscatedULong1));
        }

        [Test]
        public void OperatorMultiplication([Values((ulong)11)] ulong value1, [Values((ulong)6)] ulong value2)
        {
            var obfuscatedULong1 = new ObfuscatedULong(value1);
            var obfuscatedULong2 = new ObfuscatedULong(value2);

            ulong multiplication = value1 * value2;
            ulong obfuscatedMultiplication = new ObfuscatedULong(multiplication);

            Assert.That(AreObfuscatedEqual(obfuscatedULong1 * obfuscatedULong2, multiplication));
            Assert.That(AreObfuscatedEqual(obfuscatedULong1 * value2, multiplication));
            Assert.That(AreObfuscatedEqual(value1 * obfuscatedULong2, multiplication));
            Assert.That(AreObfuscatedEqual(obfuscatedULong1 * obfuscatedULong2, obfuscatedMultiplication));
            Assert.That(AreObfuscatedEqual(obfuscatedULong1 * value2, obfuscatedMultiplication));
            Assert.That(AreObfuscatedEqual(value1 * obfuscatedULong2, obfuscatedMultiplication));
        }

        [Test]
        public void OperatorDivision([Values((ulong)80)] ulong value1, [Values((ulong)8)] ulong value2)
        {
            var obfuscatedULong1 = new ObfuscatedULong(value1);
            var obfuscatedULong2 = new ObfuscatedULong(value2);

            ulong division = value1 / value2;
            ulong obfuscatedDivision = new ObfuscatedULong(division);

            Assert.That(AreObfuscatedEqual(obfuscatedULong1 / obfuscatedULong2, division));
            Assert.That(AreObfuscatedEqual(obfuscatedULong1 / value2, division));
            Assert.That(AreObfuscatedEqual(value1 / obfuscatedULong2, division));
            Assert.That(AreObfuscatedEqual(obfuscatedULong1 / obfuscatedULong2, obfuscatedDivision));
            Assert.That(AreObfuscatedEqual(obfuscatedULong1 / value2, obfuscatedDivision));
            Assert.That(AreObfuscatedEqual(value1 / obfuscatedULong2, obfuscatedDivision));
        }
    }
}