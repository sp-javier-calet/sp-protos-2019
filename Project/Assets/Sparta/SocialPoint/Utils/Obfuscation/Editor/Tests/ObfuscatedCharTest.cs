using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedCharTest
    {
        [Test]
        public void ConversionImplicit()
        {
            char value = (char)1680;
            var obfuscatedChar = new ObfuscatedChar(value);
            char unobfuscatedChar = obfuscatedChar;

            Assert.AreEqual(obfuscatedChar, obfuscatedChar);
            Assert.AreEqual(value, unobfuscatedChar);
            Assert.AreEqual(value, obfuscatedChar);

            Assert.AreNotEqual(value, obfuscatedChar.ObfuscatedValue);
        }

        [Test]
        public void ComparisonEqual()
        {
            char value = (char)730;
            var obfuscatedChar = new ObfuscatedChar(value);

            Assert.AreEqual(obfuscatedChar, value);
            Assert.AreEqual(value, obfuscatedChar);
        }

        [Test]
        public void ComparisonNotEqual()
        {
            char value = (char)2140;
            var obfuscatedChar = new ObfuscatedChar(value);

            Assert.AreNotEqual(obfuscatedChar, 1280);
            Assert.AreNotEqual(200, obfuscatedChar);
        }

        [Test]
        public void ComparisonLess()
        {
            char value = (char)640;
            var obfuscatedChar = new ObfuscatedChar(value);

            Assert.Less(obfuscatedChar, 1000);
            Assert.Less(30, obfuscatedChar);
        }

        [Test]
        public void ComparisonLessEqual()
        {
            char value = (char)950;
            var obfuscatedChar = new ObfuscatedChar(value);

            Assert.LessOrEqual(obfuscatedChar, 2430);
            Assert.LessOrEqual(value, obfuscatedChar);
        }

        [Test]
        public void ComparisonGreater()
        {
            char value = (char)1940;
            var obfuscatedChar = new ObfuscatedChar(value);

            Assert.Greater(obfuscatedChar, 1560);
            Assert.Greater(2550, obfuscatedChar);
        }

        [Test]
        public void ComparisonGreaterEqual()
        {
            char value = (char)1370;
            var obfuscatedChar = new ObfuscatedChar(value);

            Assert.GreaterOrEqual(obfuscatedChar, value);
            Assert.GreaterOrEqual(2090, obfuscatedChar);
        }

        [Test]
        public void OperatorAddition()
        {
            char value1 = (char)130;
            var obfuscatedChar1 = new ObfuscatedChar(value1);

            char value2 = (char)70;
            var obfuscatedChar2 = new ObfuscatedChar(value2);

            char addition = (char)(value1 + value2);
            char obfuscatedAddition = new ObfuscatedChar(addition);

            Assert.AreEqual(obfuscatedChar1 + obfuscatedChar2, addition);
            Assert.AreEqual(obfuscatedChar1 + value2, addition);
            Assert.AreEqual(value1 + obfuscatedChar2, addition);
            Assert.AreEqual(obfuscatedChar1 + obfuscatedChar2, obfuscatedAddition);
            Assert.AreEqual(obfuscatedChar1 + value2, obfuscatedAddition);
            Assert.AreEqual(value1 + obfuscatedChar2, obfuscatedAddition);

            ++value1;
            ++obfuscatedChar1;
            Assert.AreEqual(value1, obfuscatedChar1);
        }

        [Test]
        public void OperatorSubtraction()
        {
            char value1 = (char)250;
            var obfuscatedChar1 = new ObfuscatedChar(value1);

            char value2 = (char)50;
            var obfuscatedChar2 = new ObfuscatedChar(value2);

            char subtraction = (char)(value1 - value2);
            char obfuscatedSubtraction = new ObfuscatedChar(subtraction);

            Assert.AreEqual(obfuscatedChar1 - obfuscatedChar2, subtraction);
            Assert.AreEqual(obfuscatedChar1 - value2, subtraction);
            Assert.AreEqual(value1 - obfuscatedChar2, subtraction);
            Assert.AreEqual(obfuscatedChar1 - obfuscatedChar2, obfuscatedSubtraction);
            Assert.AreEqual(obfuscatedChar1 - value2, obfuscatedSubtraction);
            Assert.AreEqual(value1 - obfuscatedChar2, obfuscatedSubtraction);

            --value1;
            --obfuscatedChar1;
            Assert.AreEqual(value1, obfuscatedChar1);
        }

        [Test]
        public void OperatorMultiplication()
        {
            char value1 = (char)110;
            var obfuscatedChar1 = new ObfuscatedChar(value1);

            char value2 = (char)60;
            var obfuscatedChar2 = new ObfuscatedChar(value2);

            char multiplication = (char)(value1 * value2);
            char obfuscatedMultiplication = new ObfuscatedChar(multiplication);

            Assert.AreEqual(obfuscatedChar1 * obfuscatedChar2, multiplication);
            Assert.AreEqual(obfuscatedChar1 * value2, multiplication);
            Assert.AreEqual(value1 * obfuscatedChar2, multiplication);
            Assert.AreEqual(obfuscatedChar1 * obfuscatedChar2, obfuscatedMultiplication);
            Assert.AreEqual(obfuscatedChar1 * value2, obfuscatedMultiplication);
            Assert.AreEqual(value1 * obfuscatedChar2, obfuscatedMultiplication);
        }

        [Test]
        public void OperatorDivision()
        {
            char value1 = (char)800;
            var obfuscatedChar1 = new ObfuscatedChar(value1);

            char value2 = (char)80;
            var obfuscatedChar2 = new ObfuscatedChar(value2);

            char division = (char)(value1 / value2);
            char obfuscatedDivision = new ObfuscatedChar(division);

            Assert.AreEqual(obfuscatedChar1 / obfuscatedChar2, division);
            Assert.AreEqual(obfuscatedChar1 / value2, division);
            Assert.AreEqual(value1 / obfuscatedChar2, division);
            Assert.AreEqual(obfuscatedChar1 / obfuscatedChar2, obfuscatedDivision);
            Assert.AreEqual(obfuscatedChar1 / value2, obfuscatedDivision);
            Assert.AreEqual(value1 / obfuscatedChar2, obfuscatedDivision);
        }
    }
}