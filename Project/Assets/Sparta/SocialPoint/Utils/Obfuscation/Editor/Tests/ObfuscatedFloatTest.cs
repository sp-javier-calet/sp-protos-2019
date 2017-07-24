using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedFloatTest
    {
        const double Epsilon = 0.00001f;

        [Test]
        public void ConversionImplicit()
        {
            float value = 168.0f;
            var obfuscatedFloat = new ObfuscatedFloat(value);
            float unobfuscatedFloat = obfuscatedFloat;

            Assert.AreEqual(obfuscatedFloat, obfuscatedFloat, Epsilon);
            Assert.AreEqual(value, unobfuscatedFloat, Epsilon);
            Assert.AreEqual(value, obfuscatedFloat, Epsilon);

            Assert.AreNotEqual(value, obfuscatedFloat.ObfuscatedValue);
        }

        [Test]
        public void ComparisonEqual()
        {
            float value = 73.0f;
            var obfuscatedFloat = new ObfuscatedFloat(value);

            Assert.AreEqual(obfuscatedFloat, value, Epsilon);
            Assert.AreEqual(value, obfuscatedFloat, Epsilon);
        }

        [Test]
        public void ComparisonNotEqual()
        {
            float value = 214.0f;
            var obfuscatedFloat = new ObfuscatedFloat(value);

            Assert.AreNotEqual(obfuscatedFloat, 128.0f);
            Assert.AreNotEqual(20.0f, obfuscatedFloat);
        }

        [Test]
        public void ComparisonLess()
        {
            float value = 64.0f;
            var obfuscatedFloat = new ObfuscatedFloat(value);

            Assert.Less(obfuscatedFloat, 100.0f);
            Assert.Less(3.0f, obfuscatedFloat);
        }

        [Test]
        public void ComparisonLessEqual()
        {
            float value = 95.0f;
            var obfuscatedFloat = new ObfuscatedFloat(value);

            Assert.LessOrEqual(obfuscatedFloat, 243.0f);
            Assert.LessOrEqual(value, obfuscatedFloat);
        }

        [Test]
        public void ComparisonGreater()
        {
            float value = 194.0f;
            var obfuscatedFloat = new ObfuscatedFloat(value);

            Assert.Greater(obfuscatedFloat, 156.0f);
            Assert.Greater(255.0f, obfuscatedFloat);
        }

        [Test]
        public void ComparisonGreaterEqual()
        {
            float value = 137.0f;
            var obfuscatedFloat = new ObfuscatedFloat(value);

            Assert.GreaterOrEqual(obfuscatedFloat, value);
            Assert.GreaterOrEqual(209.0f, obfuscatedFloat);
        }

        [Test]
        public void OperatorAddition()
        {
            float value1 = 13.0f;
            var obfuscatedFloat1 = new ObfuscatedFloat(value1);

            float value2 = 7.0f;
            var obfuscatedFloat2 = new ObfuscatedFloat(value2);

            float addition = (float)(value1 + value2);
            float obfuscatedAddition = new ObfuscatedFloat(addition);

            Assert.AreEqual(obfuscatedFloat1 + obfuscatedFloat2, addition, Epsilon);
            Assert.AreEqual(obfuscatedFloat1 + value2, addition, Epsilon);
            Assert.AreEqual(value1 + obfuscatedFloat2, addition, Epsilon);
            Assert.AreEqual(obfuscatedFloat1 + obfuscatedFloat2, obfuscatedAddition, Epsilon);
            Assert.AreEqual(obfuscatedFloat1 + value2, obfuscatedAddition, Epsilon);
            Assert.AreEqual(value1 + obfuscatedFloat2, obfuscatedAddition, Epsilon);

            ++value1;
            ++obfuscatedFloat1;
            Assert.AreEqual(value1, obfuscatedFloat1, Epsilon);
        }

        [Test]
        public void OperatorSubtraction()
        {
            float value1 = 25.0f;
            var obfuscatedFloat1 = new ObfuscatedFloat(value1);

            float value2 = 5.0f;
            var obfuscatedFloat2 = new ObfuscatedFloat(value2);

            float subtraction = (float)(value1 - value2);
            float obfuscatedSubtraction = new ObfuscatedFloat(subtraction);

            Assert.AreEqual(obfuscatedFloat1 - obfuscatedFloat2, subtraction, Epsilon);
            Assert.AreEqual(obfuscatedFloat1 - value2, subtraction, Epsilon);
            Assert.AreEqual(value1 - obfuscatedFloat2, subtraction, Epsilon);
            Assert.AreEqual(obfuscatedFloat1 - obfuscatedFloat2, obfuscatedSubtraction, Epsilon);
            Assert.AreEqual(obfuscatedFloat1 - value2, obfuscatedSubtraction, Epsilon);
            Assert.AreEqual(value1 - obfuscatedFloat2, obfuscatedSubtraction, Epsilon);

            --value1;
            --obfuscatedFloat1;
            Assert.AreEqual(value1, obfuscatedFloat1, Epsilon);
        }

        [Test]
        public void OperatorMultiplication()
        {
            float value1 = 11.0f;
            var obfuscatedFloat1 = new ObfuscatedFloat(value1);

            float value2 = 6.0f;
            var obfuscatedFloat2 = new ObfuscatedFloat(value2);

            float multiplication = (float)(value1 * value2);
            float obfuscatedMultiplication = new ObfuscatedFloat(multiplication);

            Assert.AreEqual(obfuscatedFloat1 * obfuscatedFloat2, multiplication, Epsilon);
            Assert.AreEqual(obfuscatedFloat1 * value2, multiplication, Epsilon);
            Assert.AreEqual(value1 * obfuscatedFloat2, multiplication, Epsilon);
            Assert.AreEqual(obfuscatedFloat1 * obfuscatedFloat2, obfuscatedMultiplication, Epsilon);
            Assert.AreEqual(obfuscatedFloat1 * value2, obfuscatedMultiplication, Epsilon);
            Assert.AreEqual(value1 * obfuscatedFloat2, obfuscatedMultiplication, Epsilon);
        }

        [Test]
        public void OperatorDivision()
        {
            float value1 = 80.0f;
            var obfuscatedFloat1 = new ObfuscatedFloat(value1);

            float value2 = 8.0f;
            var obfuscatedFloat2 = new ObfuscatedFloat(value2);

            float division = (float)(value1 / value2);
            float obfuscatedDivision = new ObfuscatedFloat(division);

            Assert.AreEqual(obfuscatedFloat1 / obfuscatedFloat2, division, Epsilon);
            Assert.AreEqual(obfuscatedFloat1 / value2, division, Epsilon);
            Assert.AreEqual(value1 / obfuscatedFloat2, division, Epsilon);
            Assert.AreEqual(obfuscatedFloat1 / obfuscatedFloat2, obfuscatedDivision, Epsilon);
            Assert.AreEqual(obfuscatedFloat1 / value2, obfuscatedDivision, Epsilon);
            Assert.AreEqual(value1 / obfuscatedFloat2, obfuscatedDivision, Epsilon);
        }
    }
}