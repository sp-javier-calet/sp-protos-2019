using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedDoubleTest
    {
        const double Epsilon = 0.00001;

        [Test]
        public void ConversionImplicit()
        {
            double value = 168.0;
            var obfuscatedDouble = new ObfuscatedDouble(value);
            double unobfuscatedDouble = obfuscatedDouble;

            Assert.AreEqual(obfuscatedDouble, obfuscatedDouble, Epsilon);
            Assert.AreEqual(value, unobfuscatedDouble, Epsilon);
            Assert.AreEqual(value, obfuscatedDouble, Epsilon);

            Assert.AreNotEqual(value, obfuscatedDouble.ObfuscatedValue);

            double newValue = 99.0;
            obfuscatedDouble = newValue;
            Assert.AreEqual(newValue, obfuscatedDouble, Epsilon);
        }

        [Test]
        public void ComparisonEqual()
        {
            double value = 73.0;
            var obfuscatedDouble = new ObfuscatedDouble(value);

            Assert.AreEqual(obfuscatedDouble, value, Epsilon);
            Assert.AreEqual(value, obfuscatedDouble, Epsilon);
        }

        [Test]
        public void ComparisonNotEqual()
        {
            double value = 214.0;
            var obfuscatedDouble = new ObfuscatedDouble(value);

            Assert.AreNotEqual(obfuscatedDouble, 128.0);
            Assert.AreNotEqual(20.0, obfuscatedDouble);
        }

        [Test]
        public void ComparisonLess()
        {
            double value = 64.0;
            var obfuscatedDouble = new ObfuscatedDouble(value);

            Assert.Less(obfuscatedDouble, 100.0);
            Assert.Less(3, obfuscatedDouble);
        }

        [Test]
        public void ComparisonLessEqual()
        {
            double value = 95.0;
            var obfuscatedDouble = new ObfuscatedDouble(value);

            Assert.LessOrEqual(obfuscatedDouble, 243.0);
            Assert.LessOrEqual(value, obfuscatedDouble);
        }

        [Test]
        public void ComparisonGreater()
        {
            double value = 194.0;
            var obfuscatedDouble = new ObfuscatedDouble(value);

            Assert.Greater(obfuscatedDouble, 156.0);
            Assert.Greater(255.0, obfuscatedDouble);
        }

        [Test]
        public void ComparisonGreaterEqual()
        {
            double value = 137.0;
            var obfuscatedDouble = new ObfuscatedDouble(value);

            Assert.GreaterOrEqual(obfuscatedDouble, value);
            Assert.GreaterOrEqual(209.0, obfuscatedDouble);
        }

        [Test]
        public void OperatorAddition()
        {
            double value1 = 13.0;
            var obfuscatedDouble1 = new ObfuscatedDouble(value1);

            double value2 = 7.0;
            var obfuscatedDouble2 = new ObfuscatedDouble(value2);

            double addition = (double)(value1 + value2);
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
        public void OperatorSubtraction()
        {
            double value1 = 25.0;
            var obfuscatedDouble1 = new ObfuscatedDouble(value1);

            double value2 = 5.0;
            var obfuscatedDouble2 = new ObfuscatedDouble(value2);

            double subtraction = (double)(value1 - value2);
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
        public void OperatorMultiplication()
        {
            double value1 = 11.0;
            var obfuscatedDouble1 = new ObfuscatedDouble(value1);

            double value2 = 6.0;
            var obfuscatedDouble2 = new ObfuscatedDouble(value2);

            double multiplication = (double)(value1 * value2);
            double obfuscatedMultiplication = new ObfuscatedDouble(multiplication);

            Assert.AreEqual(obfuscatedDouble1 * obfuscatedDouble2, multiplication, Epsilon);
            Assert.AreEqual(obfuscatedDouble1 * value2, multiplication, Epsilon);
            Assert.AreEqual(value1 * obfuscatedDouble2, multiplication, Epsilon);
            Assert.AreEqual(obfuscatedDouble1 * obfuscatedDouble2, obfuscatedMultiplication, Epsilon);
            Assert.AreEqual(obfuscatedDouble1 * value2, obfuscatedMultiplication, Epsilon);
            Assert.AreEqual(value1 * obfuscatedDouble2, obfuscatedMultiplication, Epsilon);
        }

        [Test]
        public void OperatorDivision()
        {
            double value1 = 80.0;
            var obfuscatedDouble1 = new ObfuscatedDouble(value1);

            double value2 = 8.0;
            var obfuscatedDouble2 = new ObfuscatedDouble(value2);

            double division = (double)(value1 / value2);
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