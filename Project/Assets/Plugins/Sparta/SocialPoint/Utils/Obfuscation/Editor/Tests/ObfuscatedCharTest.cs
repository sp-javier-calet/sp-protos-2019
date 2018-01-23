using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedCharTest
    {
        bool AreObfuscatedEqual(char value1, char value2)
        {
            return value2 == value1 &&
            value2.Equals(value1) &&
            value1 == value2 &&
            value1.Equals(value2);
        }

        [Test]
        public void ConversionImplicit([Values((char)68)] char value)
        {
            var obfuscatedChar = new ObfuscatedChar(value);
            char unobfuscatedChar = obfuscatedChar;

            Assert.That(AreObfuscatedEqual(obfuscatedChar, obfuscatedChar));
            Assert.That(AreObfuscatedEqual(value, unobfuscatedChar));
            Assert.That(AreObfuscatedEqual(value, obfuscatedChar));

            Assert.AreNotEqual(value, obfuscatedChar.ObfuscatedValue);

            char newValue = (char)(value + 1);
            obfuscatedChar = newValue;
            Assert.That(AreObfuscatedEqual(newValue, obfuscatedChar));
        }

        [Test]
        public void ToString([Values((char)32)] char value)
        {
            var obfuscatedChar = new ObfuscatedChar(value);

            Assert.AreEqual(obfuscatedChar.ToString(), value.ToString());
        }

        [Test]
        public void ComparisonEqual([Values((char)73)] char value)
        {
            var obfuscatedChar = new ObfuscatedChar(value);

            Assert.That(AreObfuscatedEqual(obfuscatedChar, value));
        }

        [Test]
        public void ComparisonNotEqual([Values((char)114)] char value)
        {
            var obfuscatedChar = new ObfuscatedChar(value);

            char differentValue = (char)(value + 1);
            Assert.AreNotEqual(obfuscatedChar, differentValue);
            Assert.AreNotEqual(differentValue, obfuscatedChar);
        }

        [Test]
        public void ComparisonLess([Values((char)64)] char value1, [Values((char)100)] char value2)
        {
            Assert.Less(value1, value2);

            var obfuscatedChar1 = new ObfuscatedChar(value1);
            var obfuscatedChar2 = new ObfuscatedChar(value2);

            Assert.Less(obfuscatedChar1, obfuscatedChar2);
            Assert.Less(obfuscatedChar1, value2);
            Assert.Less(value1, obfuscatedChar2);
        }

        [Test]
        public void ComparisonLessEqual([Values((char)95, (char)101)] char value1, [Values((char)101)] char value2)
        {
            Assert.LessOrEqual(value1, value2);

            var obfuscatedChar1 = new ObfuscatedChar(value1);
            var obfuscatedChar2 = new ObfuscatedChar(value2);

            Assert.LessOrEqual(obfuscatedChar1, obfuscatedChar2);
            Assert.LessOrEqual(obfuscatedChar1, value2);
            Assert.LessOrEqual(value1, obfuscatedChar2);
        }

        [Test]
        public void ComparisonGreater([Values((char)127)] char value1, [Values((char)41)] char value2)
        {
            Assert.Greater(value1, value2);

            var obfuscatedChar1 = new ObfuscatedChar(value1);
            var obfuscatedChar2 = new ObfuscatedChar(value2);

            Assert.Greater(obfuscatedChar1, obfuscatedChar2);
            Assert.Greater(obfuscatedChar1, value2);
            Assert.Greater(value1, obfuscatedChar2);
        }

        [Test]
        public void ComparisonGreaterEqual([Values((char)117, (char)54)] char value1, [Values((char)54)] char value2)
        {
            Assert.GreaterOrEqual(value1, value2);

            var obfuscatedChar1 = new ObfuscatedChar(value1);
            var obfuscatedChar2 = new ObfuscatedChar(value2);

            Assert.GreaterOrEqual(obfuscatedChar1, obfuscatedChar2);
            Assert.GreaterOrEqual(obfuscatedChar1, value2);
            Assert.GreaterOrEqual(value1, obfuscatedChar2);
        }

        [Test]
        public void OperatorAddition([Values((char)13)] char value1, [Values((char)7)] char value2)
        {
            var obfuscatedChar1 = new ObfuscatedChar(value1);
            var obfuscatedChar2 = new ObfuscatedChar(value2);

            char addition = (char)(value1 + value2);
            char obfuscatedAddition = new ObfuscatedChar(addition);

            Assert.That(AreObfuscatedEqual(obfuscatedChar1 + obfuscatedChar2, addition));
            Assert.That(AreObfuscatedEqual(obfuscatedChar1 + value2, addition));
            Assert.That(AreObfuscatedEqual(value1 + obfuscatedChar2, addition));
            Assert.That(AreObfuscatedEqual(obfuscatedChar1 + obfuscatedChar2, obfuscatedAddition));
            Assert.That(AreObfuscatedEqual(obfuscatedChar1 + value2, obfuscatedAddition));
            Assert.That(AreObfuscatedEqual(value1 + obfuscatedChar2, obfuscatedAddition));

            ++value1;
            ++obfuscatedChar1;
            Assert.That(AreObfuscatedEqual(value1, obfuscatedChar1));
        }

        [Test]
        public void OperatorSubtraction([Values((char)25)] char value1, [Values((char)5)] char value2)
        {
            var obfuscatedChar1 = new ObfuscatedChar(value1);
            var obfuscatedChar2 = new ObfuscatedChar(value2);

            char subtraction = (char)(value1 - value2);
            char obfuscatedSubtraction = new ObfuscatedChar(subtraction);

            Assert.That(AreObfuscatedEqual(obfuscatedChar1 - obfuscatedChar2, subtraction));
            Assert.That(AreObfuscatedEqual(obfuscatedChar1 - value2, subtraction));
            Assert.That(AreObfuscatedEqual(value1 - obfuscatedChar2, subtraction));
            Assert.That(AreObfuscatedEqual(obfuscatedChar1 - obfuscatedChar2, obfuscatedSubtraction));
            Assert.That(AreObfuscatedEqual(obfuscatedChar1 - value2, obfuscatedSubtraction));
            Assert.That(AreObfuscatedEqual(value1 - obfuscatedChar2, obfuscatedSubtraction));

            --value1;
            --obfuscatedChar1;
            Assert.That(AreObfuscatedEqual(value1, obfuscatedChar1));
        }

        [Test]
        public void OperatorMultiplication([Values((char)11)] char value1, [Values((char)6)] char value2)
        {
            var obfuscatedChar1 = new ObfuscatedChar(value1);
            var obfuscatedChar2 = new ObfuscatedChar(value2);

            char multiplication = (char)(value1 * value2);
            char obfuscatedMultiplication = new ObfuscatedChar(multiplication);

            Assert.That(AreObfuscatedEqual(obfuscatedChar1 * obfuscatedChar2, multiplication));
            Assert.That(AreObfuscatedEqual(obfuscatedChar1 * value2, multiplication));
            Assert.That(AreObfuscatedEqual(value1 * obfuscatedChar2, multiplication));
            Assert.That(AreObfuscatedEqual(obfuscatedChar1 * obfuscatedChar2, obfuscatedMultiplication));
            Assert.That(AreObfuscatedEqual(obfuscatedChar1 * value2, obfuscatedMultiplication));
            Assert.That(AreObfuscatedEqual(value1 * obfuscatedChar2, obfuscatedMultiplication));
        }

        [Test]
        public void OperatorDivision([Values((char)80)] char value1, [Values((char)8)] char value2)
        {
            var obfuscatedChar1 = new ObfuscatedChar(value1);
            var obfuscatedChar2 = new ObfuscatedChar(value2);

            char division = (char)(value1 / value2);
            char obfuscatedDivision = new ObfuscatedChar(division);

            Assert.That(AreObfuscatedEqual(obfuscatedChar1 / obfuscatedChar2, division));
            Assert.That(AreObfuscatedEqual(obfuscatedChar1 / value2, division));
            Assert.That(AreObfuscatedEqual(value1 / obfuscatedChar2, division));
            Assert.That(AreObfuscatedEqual(obfuscatedChar1 / obfuscatedChar2, obfuscatedDivision));
            Assert.That(AreObfuscatedEqual(obfuscatedChar1 / value2, obfuscatedDivision));
            Assert.That(AreObfuscatedEqual(value1 / obfuscatedChar2, obfuscatedDivision));
        }
    }
}