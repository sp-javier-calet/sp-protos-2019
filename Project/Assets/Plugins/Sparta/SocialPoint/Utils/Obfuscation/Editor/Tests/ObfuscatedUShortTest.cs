using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedUShortTest
    {
        static bool AreObfuscatedEqual(ushort value1, ushort value2)
        {
            return value2 == value1 &&
                value2.Equals(value1) &&
                value1 == value2 &&
                value1.Equals(value2);
        }

        [Test]
        public void ConversionImplicit([Values((ushort)68)] ushort value)
        {
            var obfuscatedUShort = new ObfuscatedUShort(value);
            ushort unobfuscatedUShort = obfuscatedUShort;

            Assert.That(AreObfuscatedEqual(obfuscatedUShort, obfuscatedUShort));
            Assert.That(AreObfuscatedEqual(value, unobfuscatedUShort));
            Assert.That(AreObfuscatedEqual(value, obfuscatedUShort));

            Assert.AreNotEqual(value, obfuscatedUShort.ObfuscatedValue);

            ushort newValue = (ushort)(value + 1);
            obfuscatedUShort = newValue;
            Assert.That(AreObfuscatedEqual(newValue, obfuscatedUShort));
        }

        [Test]
        public void ToString([Values((ushort)32)] ushort value)
        {
            var obfuscatedUShort = new ObfuscatedUShort(value);

            Assert.AreEqual(obfuscatedUShort.ToString(), value.ToString());
        }

        [Test]
        public void ComparisonEqual([Values((ushort)73)] ushort value)
        {
            var obfuscatedUShort = new ObfuscatedUShort(value);

            Assert.That(AreObfuscatedEqual(obfuscatedUShort, value));
            Assert.That(AreObfuscatedEqual(value, obfuscatedUShort));
        }

        [Test]
        public void ComparisonNotEqual([Values((ushort)114)] ushort value)
        {
            var obfuscatedUShort = new ObfuscatedUShort(value);

            ushort differentValue = (ushort)(value + 1);
            Assert.AreNotEqual(obfuscatedUShort, differentValue);
            Assert.AreNotEqual(differentValue, obfuscatedUShort);
        }

        [Test]
        public void ComparisonLess([Values((ushort)64)] ushort value1, [Values((ushort)100)] ushort value2)
        {
            Assert.Less(value1, value2);

            var obfuscatedUShort1 = new ObfuscatedUShort(value1);
            var obfuscatedUShort2 = new ObfuscatedUShort(value2);

            Assert.Less(obfuscatedUShort1, obfuscatedUShort2);
            Assert.Less(obfuscatedUShort1, value2);
            Assert.Less(value1, obfuscatedUShort2);
        }

        [Test]
        public void ComparisonLessEqual([Values((ushort)95, (ushort)101)] ushort value1, [Values((ushort)101)] ushort value2)
        {
            Assert.LessOrEqual(value1, value2);

            var obfuscatedUShort1 = new ObfuscatedUShort(value1);
            var obfuscatedUShort2 = new ObfuscatedUShort(value2);

            Assert.LessOrEqual(obfuscatedUShort1, obfuscatedUShort2);
            Assert.LessOrEqual(obfuscatedUShort1, value2);
            Assert.LessOrEqual(value1, obfuscatedUShort2);
        }

        [Test]
        public void ComparisonGreater([Values((ushort)127)] ushort value1, [Values((ushort)41)] ushort value2)
        {
            Assert.Greater(value1, value2);

            var obfuscatedUShort1 = new ObfuscatedUShort(value1);
            var obfuscatedUShort2 = new ObfuscatedUShort(value2);

            Assert.Greater(obfuscatedUShort1, obfuscatedUShort2);
            Assert.Greater(obfuscatedUShort1, value2);
            Assert.Greater(value1, obfuscatedUShort2);
        }

        [Test]
        public void ComparisonGreaterEqual([Values((ushort)117, (ushort)54)] ushort value1, [Values((ushort)54)] ushort value2)
        {
            Assert.GreaterOrEqual(value1, value2);

            var obfuscatedUShort1 = new ObfuscatedUShort(value1);
            var obfuscatedUShort2 = new ObfuscatedUShort(value2);

            Assert.GreaterOrEqual(obfuscatedUShort1, obfuscatedUShort2);
            Assert.GreaterOrEqual(obfuscatedUShort1, value2);
            Assert.GreaterOrEqual(value1, obfuscatedUShort2);
        }

        [Test]
        public void OperatorAddition([Values((ushort)13)] ushort value1, [Values((ushort)7)] ushort value2)
        {
            var obfuscatedUShort1 = new ObfuscatedUShort(value1);
            var obfuscatedUShort2 = new ObfuscatedUShort(value2);

            ushort addition = (ushort)(value1 + value2);
            ushort obfuscatedAddition = new ObfuscatedUShort(addition);

            Assert.That(AreObfuscatedEqual(obfuscatedUShort1 + obfuscatedUShort2, addition));
            Assert.That(AreObfuscatedEqual(obfuscatedUShort1 + value2, addition));
            Assert.That(AreObfuscatedEqual(value1 + obfuscatedUShort2, addition));
            Assert.That(AreObfuscatedEqual(obfuscatedUShort1 + obfuscatedUShort2, obfuscatedAddition));
            Assert.That(AreObfuscatedEqual(obfuscatedUShort1 + value2, obfuscatedAddition));
            Assert.That(AreObfuscatedEqual(value1 + obfuscatedUShort2, obfuscatedAddition));

            ++value1;
            ++obfuscatedUShort1;
            Assert.That(AreObfuscatedEqual(value1, obfuscatedUShort1));
        }

        [Test]
        public void OperatorSubtraction([Values((ushort)25)] ushort value1, [Values((ushort)5)] ushort value2)
        {
            var obfuscatedUShort1 = new ObfuscatedUShort(value1);
            var obfuscatedUShort2 = new ObfuscatedUShort(value2);

            ushort subtraction = (ushort)(value1 - value2);
            ushort obfuscatedSubtraction = new ObfuscatedUShort(subtraction);

            Assert.That(AreObfuscatedEqual(obfuscatedUShort1 - obfuscatedUShort2, subtraction));
            Assert.That(AreObfuscatedEqual(obfuscatedUShort1 - value2, subtraction));
            Assert.That(AreObfuscatedEqual(value1 - obfuscatedUShort2, subtraction));
            Assert.That(AreObfuscatedEqual(obfuscatedUShort1 - obfuscatedUShort2, obfuscatedSubtraction));
            Assert.That(AreObfuscatedEqual(obfuscatedUShort1 - value2, obfuscatedSubtraction));
            Assert.That(AreObfuscatedEqual(value1 - obfuscatedUShort2, obfuscatedSubtraction));

            --value1;
            --obfuscatedUShort1;
            Assert.That(AreObfuscatedEqual(value1, obfuscatedUShort1));
        }

        [Test]
        public void OperatorMultiplication([Values((ushort)11)] ushort value1, [Values((ushort)6)] ushort value2)
        {
            var obfuscatedUShort1 = new ObfuscatedUShort(value1);
            var obfuscatedUShort2 = new ObfuscatedUShort(value2);

            ushort multiplication = (ushort)(value1 * value2);
            ushort obfuscatedMultiplication = new ObfuscatedUShort(multiplication);

            Assert.That(AreObfuscatedEqual(obfuscatedUShort1 * obfuscatedUShort2, multiplication));
            Assert.That(AreObfuscatedEqual(obfuscatedUShort1 * value2, multiplication));
            Assert.That(AreObfuscatedEqual(value1 * obfuscatedUShort2, multiplication));
            Assert.That(AreObfuscatedEqual(obfuscatedUShort1 * obfuscatedUShort2, obfuscatedMultiplication));
            Assert.That(AreObfuscatedEqual(obfuscatedUShort1 * value2, obfuscatedMultiplication));
            Assert.That(AreObfuscatedEqual(value1 * obfuscatedUShort2, obfuscatedMultiplication));
        }

        [Test]
        public void OperatorDivision([Values((ushort)80)] ushort value1, [Values((ushort)8)] ushort value2)
        {
            var obfuscatedUShort1 = new ObfuscatedUShort(value1);
            var obfuscatedUShort2 = new ObfuscatedUShort(value2);

            ushort division = (ushort)(value1 / value2);
            ushort obfuscatedDivision = new ObfuscatedUShort(division);

            Assert.That(AreObfuscatedEqual(obfuscatedUShort1 / obfuscatedUShort2, division));
            Assert.That(AreObfuscatedEqual(obfuscatedUShort1 / value2, division));
            Assert.That(AreObfuscatedEqual(value1 / obfuscatedUShort2, division));
            Assert.That(AreObfuscatedEqual(obfuscatedUShort1 / obfuscatedUShort2, obfuscatedDivision));
            Assert.That(AreObfuscatedEqual(obfuscatedUShort1 / value2, obfuscatedDivision));
            Assert.That(AreObfuscatedEqual(value1 / obfuscatedUShort2, obfuscatedDivision));
        }
    }
}