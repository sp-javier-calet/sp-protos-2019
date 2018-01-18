using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedFloatTest
    {
        const float Epsilon = 0.000001f;

        static bool AreObfuscatedEqual(float value1, float value2, float epsilon)
        {
            return Math.Abs(value2 - value1) < epsilon &&
                Math.Abs(value1 - value2) < epsilon;
        }

        [Test]
        public void ConversionImplicit([Values(68.0f)] float value)
        {
            var obfuscatedFloat = new ObfuscatedFloat(value);
            float unobfuscatedFloat = obfuscatedFloat;

            Assert.That(AreObfuscatedEqual(obfuscatedFloat, obfuscatedFloat, Epsilon));
            Assert.That(AreObfuscatedEqual(value, unobfuscatedFloat, Epsilon));
            Assert.That(AreObfuscatedEqual(value, obfuscatedFloat, Epsilon));

            Assert.AreNotEqual(value, obfuscatedFloat.ObfuscatedValue);

            float newValue = value + 1.0f;
            obfuscatedFloat = newValue;
            Assert.That(AreObfuscatedEqual(newValue, obfuscatedFloat, Epsilon));
        }

        [Test]
        public void ToString([Values(32.0f)] float value)
        {
            var obfuscatedFloat = new ObfuscatedFloat(value);

            Assert.AreEqual(obfuscatedFloat.ToString(), value.ToString());
        }

        [Test]
        public void ComparisonEqual([Values(73.0f)] float value)
        {
            var obfuscatedFloat = new ObfuscatedFloat(value);

            Assert.That(AreObfuscatedEqual(obfuscatedFloat, value, Epsilon));
            Assert.That(AreObfuscatedEqual(value, obfuscatedFloat, Epsilon));
        }

        [Test]
        public void ComparisonNotEqual([Values(114.0f)] float value)
        {
            var obfuscatedFloat = new ObfuscatedFloat(value);

            float differentValue = value + 1.0f;
            Assert.AreNotEqual(obfuscatedFloat, differentValue);
            Assert.AreNotEqual(differentValue, obfuscatedFloat);
        }

        [Test]
        public void ComparisonLess([Values(64.0f)] float value1, [Values(100.0f)] float value2)
        {
            Assert.Less(value1, value2);

            var obfuscatedFloat1 = new ObfuscatedFloat(value1);
            var obfuscatedFloat2 = new ObfuscatedFloat(value2);

            Assert.Less(obfuscatedFloat1, obfuscatedFloat2);
            Assert.Less(obfuscatedFloat1, value2);
            Assert.Less(value1, obfuscatedFloat2);
        }

        [Test]
        public void ComparisonLessEqual([Values(95.0f, 101.0f)] float value1, [Values(101.0f)] float value2)
        {
            Assert.LessOrEqual(value1, value2);

            var obfuscatedFloat1 = new ObfuscatedFloat(value1);
            var obfuscatedFloat2 = new ObfuscatedFloat(value2);

            Assert.LessOrEqual(obfuscatedFloat1, obfuscatedFloat2);
            Assert.LessOrEqual(obfuscatedFloat1, value2);
            Assert.LessOrEqual(value1, obfuscatedFloat2);
        }

        [Test]
        public void ComparisonGreater([Values(127.0f)] float value1, [Values(41.0f)] float value2)
        {
            Assert.Greater(value1, value2);

            var obfuscatedFloat1 = new ObfuscatedFloat(value1);
            var obfuscatedFloat2 = new ObfuscatedFloat(value2);

            Assert.Greater(obfuscatedFloat1, obfuscatedFloat2);
            Assert.Greater(obfuscatedFloat1, value2);
            Assert.Greater(value1, obfuscatedFloat2);
        }

        [Test]
        public void ComparisonGreaterEqual([Values(117.0f, 54.0f)] float value1, [Values(54.0f)] float value2)
        {
            Assert.GreaterOrEqual(value1, value2);

            var obfuscatedFloat1 = new ObfuscatedFloat(value1);
            var obfuscatedFloat2 = new ObfuscatedFloat(value2);

            Assert.GreaterOrEqual(obfuscatedFloat1, obfuscatedFloat2);
            Assert.GreaterOrEqual(obfuscatedFloat1, value2);
            Assert.GreaterOrEqual(value1, obfuscatedFloat2);
        }

        [Test]
        public void OperatorAddition([Values(13.0f)] float value1, [Values(7.0f)] float value2)
        {
            var obfuscatedFloat1 = new ObfuscatedFloat(value1);
            var obfuscatedFloat2 = new ObfuscatedFloat(value2);

            float addition = value1 + value2;
            float obfuscatedAddition = new ObfuscatedFloat(addition);

            Assert.That(AreObfuscatedEqual(obfuscatedFloat1 + obfuscatedFloat2, addition, Epsilon));
            Assert.That(AreObfuscatedEqual(obfuscatedFloat1 + value2, addition, Epsilon));
            Assert.That(AreObfuscatedEqual(value1 + obfuscatedFloat2, addition, Epsilon));
            Assert.That(AreObfuscatedEqual(obfuscatedFloat1 + obfuscatedFloat2, obfuscatedAddition, Epsilon));
            Assert.That(AreObfuscatedEqual(obfuscatedFloat1 + value2, obfuscatedAddition, Epsilon));
            Assert.That(AreObfuscatedEqual(value1 + obfuscatedFloat2, obfuscatedAddition, Epsilon));

            ++value1;
            ++obfuscatedFloat1;
            Assert.That(AreObfuscatedEqual(value1, obfuscatedFloat1, Epsilon));
        }

        [Test]
        public void OperatorSubtraction([Values(25.0f)] float value1, [Values(5.0f)] float value2)
        {
            var obfuscatedFloat1 = new ObfuscatedFloat(value1);
            var obfuscatedFloat2 = new ObfuscatedFloat(value2);

            float subtraction = value1 - value2;
            float obfuscatedSubtraction = new ObfuscatedFloat(subtraction);

            Assert.That(AreObfuscatedEqual(obfuscatedFloat1 - obfuscatedFloat2, subtraction, Epsilon));
            Assert.That(AreObfuscatedEqual(obfuscatedFloat1 - value2, subtraction, Epsilon));
            Assert.That(AreObfuscatedEqual(value1 - obfuscatedFloat2, subtraction, Epsilon));
            Assert.That(AreObfuscatedEqual(obfuscatedFloat1 - obfuscatedFloat2, obfuscatedSubtraction, Epsilon));
            Assert.That(AreObfuscatedEqual(obfuscatedFloat1 - value2, obfuscatedSubtraction, Epsilon));
            Assert.That(AreObfuscatedEqual(value1 - obfuscatedFloat2, obfuscatedSubtraction, Epsilon));

            --value1;
            --obfuscatedFloat1;
            Assert.That(AreObfuscatedEqual(value1, obfuscatedFloat1, Epsilon));
        }

        [Test]
        public void OperatorMultiplication([Values(11.0f)] float value1, [Values(6.0f)] float value2)
        {
            var obfuscatedFloat1 = new ObfuscatedFloat(value1);
            var obfuscatedFloat2 = new ObfuscatedFloat(value2);

            float multiplication = value1 * value2;
            float obfuscatedMultiplication = new ObfuscatedFloat(multiplication);

            Assert.That(AreObfuscatedEqual(obfuscatedFloat1 * obfuscatedFloat2, multiplication, Epsilon));
            Assert.That(AreObfuscatedEqual(obfuscatedFloat1 * value2, multiplication, Epsilon));
            Assert.That(AreObfuscatedEqual(value1 * obfuscatedFloat2, multiplication, Epsilon));
            Assert.That(AreObfuscatedEqual(obfuscatedFloat1 * obfuscatedFloat2, obfuscatedMultiplication, Epsilon));
            Assert.That(AreObfuscatedEqual(obfuscatedFloat1 * value2, obfuscatedMultiplication, Epsilon));
            Assert.That(AreObfuscatedEqual(value1 * obfuscatedFloat2, obfuscatedMultiplication, Epsilon));
        }

        [Test]
        public void OperatorDivision([Values(80.0f)] float value1, [Values(8.0f)] float value2)
        {
            var obfuscatedFloat1 = new ObfuscatedFloat(value1);
            var obfuscatedFloat2 = new ObfuscatedFloat(value2);

            float division = value1 / value2;
            float obfuscatedDivision = new ObfuscatedFloat(division);

            Assert.That(AreObfuscatedEqual(obfuscatedFloat1 / obfuscatedFloat2, division, Epsilon));
            Assert.That(AreObfuscatedEqual(obfuscatedFloat1 / value2, division, Epsilon));
            Assert.That(AreObfuscatedEqual(value1 / obfuscatedFloat2, division, Epsilon));
            Assert.That(AreObfuscatedEqual(obfuscatedFloat1 / obfuscatedFloat2, obfuscatedDivision, Epsilon));
            Assert.That(AreObfuscatedEqual(obfuscatedFloat1 / value2, obfuscatedDivision, Epsilon));
            Assert.That(AreObfuscatedEqual(value1 / obfuscatedFloat2, obfuscatedDivision, Epsilon));
        }
    }
}