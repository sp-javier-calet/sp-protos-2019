using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedDoubleTest
    {
        const double Epsilon = 0.000001;

        [Test]
        public void ConversionImplicit([Values(68.0)] double value)
        {
            var obfuscatedDouble = new ObfuscatedDouble(value);
            double unobfuscatedDouble = obfuscatedDouble;

            Assert.AreEqual(obfuscatedDouble, obfuscatedDouble, Epsilon);
            Assert.AreEqual(value, unobfuscatedDouble, Epsilon);
            Assert.AreEqual(value, obfuscatedDouble, Epsilon);

            Assert.AreNotEqual(value, obfuscatedDouble.ObfuscatedValue);

            double newValue = value + 1.0;
            obfuscatedDouble = newValue;
            Assert.AreEqual(newValue, obfuscatedDouble, Epsilon);
        }

        [Test]
        public void ToString([Values(32.0)] double value)
        {
            var obfuscatedDouble = new ObfuscatedDouble(value);

            Assert.AreEqual(obfuscatedDouble.ToString(), value.ToString());
        }

        [Test]
        public void ComparisonEqual([Values(73.0)] double value)
        {
            var obfuscatedDouble = new ObfuscatedDouble(value);

            Assert.AreEqual(obfuscatedDouble, value, Epsilon);
            Assert.AreEqual(value, obfuscatedDouble, Epsilon);
        }

        [Test]
        public void ComparisonNotEqual([Values(114.0)] double value)
        {
            var obfuscatedDouble = new ObfuscatedDouble(value);

            double differentValue = value + 1.0;
            Assert.AreNotEqual(obfuscatedDouble, differentValue);
            Assert.AreNotEqual(differentValue, obfuscatedDouble);
        }

        [Test]
        public void ComparisonLess([Values(64.0)] double value1, [Values(100.0)] double value2)
        {
            Assert.Less(value1, value2);

            var obfuscatedDouble1 = new ObfuscatedDouble(value1);
            var obfuscatedDouble2 = new ObfuscatedDouble(value2);

            Assert.Less(obfuscatedDouble1, obfuscatedDouble2);
            Assert.Less(obfuscatedDouble1, value2);
            Assert.Less(value1, obfuscatedDouble2);
        }

        [Test]
        public void ComparisonLessEqual([Values(95.0, 101.0)] double value1, [Values(101.0)] double value2)
        {
            Assert.LessOrEqual(value1, value2);

            var obfuscatedDouble1 = new ObfuscatedDouble(value1);
            var obfuscatedDouble2 = new ObfuscatedDouble(value2);

            Assert.LessOrEqual(obfuscatedDouble1, obfuscatedDouble2);
            Assert.LessOrEqual(obfuscatedDouble1, value2);
            Assert.LessOrEqual(value1, obfuscatedDouble2);
        }

        [Test]
        public void ComparisonGreater([Values(127.0)] double value1, [Values(41.0)] double value2)
        {
            Assert.Greater(value1, value2);

            var obfuscatedDouble1 = new ObfuscatedDouble(value1);
            var obfuscatedDouble2 = new ObfuscatedDouble(value2);

            Assert.Greater(obfuscatedDouble1, obfuscatedDouble2);
            Assert.Greater(obfuscatedDouble1, value2);
            Assert.Greater(value1, obfuscatedDouble2);
        }

        [Test]
        public void ComparisonGreaterEqual([Values(117.0, 54.0)] double value1, [Values(54.0)] double value2)
        {
            Assert.GreaterOrEqual(value1, value2);

            var obfuscatedDouble1 = new ObfuscatedDouble(value1);
            var obfuscatedDouble2 = new ObfuscatedDouble(value2);

            Assert.GreaterOrEqual(obfuscatedDouble1, obfuscatedDouble2);
            Assert.GreaterOrEqual(obfuscatedDouble1, value2);
            Assert.GreaterOrEqual(value1, obfuscatedDouble2);
        }

        [Test]
        public void OperatorAddition([Values(13.0)] double value1, [Values(7.0)] double value2)
        {
            var obfuscatedDouble1 = new ObfuscatedDouble(value1);
            var obfuscatedDouble2 = new ObfuscatedDouble(value2);

            double addition = value1 + value2;
            double obfuscatedAddition = new ObfuscatedDouble(addition);

            Assert.AreEqual(obfuscatedDouble1 + obfuscatedDouble2, addition, Epsilon);
            Assert.AreEqual(obfuscatedDouble1 + value2, addition, Epsilon);
            Assert.AreEqual(value1 + obfuscatedDouble2, addition, Epsilon);
            Assert.AreEqual(obfuscatedDouble1 + obfuscatedDouble2, obfuscatedAddition, Epsilon);
            Assert.AreEqual(obfuscatedDouble1 + value2, obfuscatedAddition, Epsilon);
            Assert.AreEqual(value1 + obfuscatedDouble2, obfuscatedAddition, Epsilon);

            ++value1;
            ++obfuscatedDouble1;
            Assert.AreEqual(value1, obfuscatedDouble1, Epsilon);
        }

        [Test]
        public void OperatorSubtraction([Values(25.0)] double value1, [Values(5.0)] double value2)
        {
            var obfuscatedDouble1 = new ObfuscatedDouble(value1);
            var obfuscatedDouble2 = new ObfuscatedDouble(value2);

            double subtraction = value1 - value2;
            double obfuscatedSubtraction = new ObfuscatedDouble(subtraction);

            Assert.AreEqual(obfuscatedDouble1 - obfuscatedDouble2, subtraction, Epsilon);
            Assert.AreEqual(obfuscatedDouble1 - value2, subtraction, Epsilon);
            Assert.AreEqual(value1 - obfuscatedDouble2, subtraction, Epsilon);
            Assert.AreEqual(obfuscatedDouble1 - obfuscatedDouble2, obfuscatedSubtraction, Epsilon);
            Assert.AreEqual(obfuscatedDouble1 - value2, obfuscatedSubtraction, Epsilon);
            Assert.AreEqual(value1 - obfuscatedDouble2, obfuscatedSubtraction, Epsilon);

            --value1;
            --obfuscatedDouble1;
            Assert.AreEqual(value1, obfuscatedDouble1, Epsilon);
        }

        [Test]
        public void OperatorMultiplication([Values(11.0)] double value1, [Values(6.0)] double value2)
        {
            var obfuscatedDouble1 = new ObfuscatedDouble(value1);
            var obfuscatedDouble2 = new ObfuscatedDouble(value2);

            double multiplication = value1 * value2;
            double obfuscatedMultiplication = new ObfuscatedDouble(multiplication);

            Assert.AreEqual(obfuscatedDouble1 * obfuscatedDouble2, multiplication, Epsilon);
            Assert.AreEqual(obfuscatedDouble1 * value2, multiplication, Epsilon);
            Assert.AreEqual(value1 * obfuscatedDouble2, multiplication, Epsilon);
            Assert.AreEqual(obfuscatedDouble1 * obfuscatedDouble2, obfuscatedMultiplication, Epsilon);
            Assert.AreEqual(obfuscatedDouble1 * value2, obfuscatedMultiplication, Epsilon);
            Assert.AreEqual(value1 * obfuscatedDouble2, obfuscatedMultiplication, Epsilon);
        }

        [Test]
        public void OperatorDivision([Values(80.0)] double value1, [Values(8.0)] double value2)
        {
            var obfuscatedDouble1 = new ObfuscatedDouble(value1);
            var obfuscatedDouble2 = new ObfuscatedDouble(value2);

            double division = value1 / value2;
            double obfuscatedDivision = new ObfuscatedDouble(division);

            Assert.AreEqual(obfuscatedDouble1 / obfuscatedDouble2, division, Epsilon);
            Assert.AreEqual(obfuscatedDouble1 / value2, division, Epsilon);
            Assert.AreEqual(value1 / obfuscatedDouble2, division, Epsilon);
            Assert.AreEqual(obfuscatedDouble1 / obfuscatedDouble2, obfuscatedDivision, Epsilon);
            Assert.AreEqual(obfuscatedDouble1 / value2, obfuscatedDivision, Epsilon);
            Assert.AreEqual(value1 / obfuscatedDouble2, obfuscatedDivision, Epsilon);
        }
    }
}