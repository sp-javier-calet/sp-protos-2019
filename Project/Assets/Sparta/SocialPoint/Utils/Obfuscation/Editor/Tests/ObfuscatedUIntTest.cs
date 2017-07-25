﻿using System;
using NUnit.Framework;

namespace SocialPoint.Utils.Obfuscation
{
    [TestFixture]
    [Category("SocialPoint.Utils.Obfuscation")]
    public class ObfuscatedUIntTest
    {
        [Test]
        public void ConversionImplicit([Values((uint)68)] uint value)
        {
            var obfuscatedUInt = new ObfuscatedUInt(value);
            uint unobfuscatedUInt = obfuscatedUInt;

            Assert.AreEqual(obfuscatedUInt, obfuscatedUInt);
            Assert.AreEqual(value, unobfuscatedUInt);
            Assert.AreEqual(value, obfuscatedUInt);

            Assert.AreNotEqual(value, obfuscatedUInt.ObfuscatedValue);

            uint newValue = value + 1;
            obfuscatedUInt = newValue;
            Assert.AreEqual(newValue, obfuscatedUInt);
        }

        [Test]
        public void ComparisonEqual([Values((uint)73)] uint value)
        {
            var obfuscatedUInt = new ObfuscatedUInt(value);

            Assert.AreEqual(obfuscatedUInt, value);
            Assert.AreEqual(value, obfuscatedUInt);
        }

        [Test]
        public void ComparisonNotEqual([Values((uint)114)] uint value)
        {
            var obfuscatedUInt = new ObfuscatedUInt(value);

            uint differentValue = value + 1;
            Assert.AreNotEqual(obfuscatedUInt, differentValue);
            Assert.AreNotEqual(differentValue, obfuscatedUInt);
        }

        [Test]
        public void ComparisonLess([Values((uint)64)] uint value1, [Values((uint)100)] uint value2)
        {
            Assert.Less(value1, value2);

            var obfuscatedUInt1 = new ObfuscatedUInt(value1);
            var obfuscatedUInt2 = new ObfuscatedUInt(value2);

            Assert.Less(obfuscatedUInt1, obfuscatedUInt2);
            Assert.Less(obfuscatedUInt1, value2);
            Assert.Less(value1, obfuscatedUInt2);
        }

        [Test]
        public void ComparisonLessEqual([Values((uint)95, (uint)101)] uint value1, [Values((uint)101)] uint value2)
        {
            Assert.LessOrEqual(value1, value2);

            var obfuscatedUInt1 = new ObfuscatedUInt(value1);
            var obfuscatedUInt2 = new ObfuscatedUInt(value2);

            Assert.LessOrEqual(obfuscatedUInt1, obfuscatedUInt2);
            Assert.LessOrEqual(obfuscatedUInt1, value2);
            Assert.LessOrEqual(value1, obfuscatedUInt2);
        }

        [Test]
        public void ComparisonGreater([Values((uint)127)] uint value1, [Values((uint)41)] uint value2)
        {
            Assert.Greater(value1, value2);

            var obfuscatedUInt1 = new ObfuscatedUInt(value1);
            var obfuscatedUInt2 = new ObfuscatedUInt(value2);

            Assert.Greater(obfuscatedUInt1, obfuscatedUInt2);
            Assert.Greater(obfuscatedUInt1, value2);
            Assert.Greater(value1, obfuscatedUInt2);
        }

        [Test]
        public void ComparisonGreaterEqual([Values((uint)117, (uint)54)] uint value1, [Values((uint)54)] uint value2)
        {
            Assert.GreaterOrEqual(value1, value2);

            var obfuscatedUInt1 = new ObfuscatedUInt(value1);
            var obfuscatedUInt2 = new ObfuscatedUInt(value2);

            Assert.GreaterOrEqual(obfuscatedUInt1, obfuscatedUInt2);
            Assert.GreaterOrEqual(obfuscatedUInt1, value2);
            Assert.GreaterOrEqual(value1, obfuscatedUInt2);
        }

        [Test]
        public void OperatorAddition([Values((uint)13)] uint value1, [Values((uint)7)] uint value2)
        {
            var obfuscatedUInt1 = new ObfuscatedUInt(value1);
            var obfuscatedUInt2 = new ObfuscatedUInt(value2);

            uint addition = value1 + value2;
            uint obfuscatedAddition = new ObfuscatedUInt(addition);

            Assert.AreEqual(obfuscatedUInt1 + obfuscatedUInt2, addition);
            Assert.AreEqual(obfuscatedUInt1 + value2, addition);
            Assert.AreEqual(value1 + obfuscatedUInt2, addition);
            Assert.AreEqual(obfuscatedUInt1 + obfuscatedUInt2, obfuscatedAddition);
            Assert.AreEqual(obfuscatedUInt1 + value2, obfuscatedAddition);
            Assert.AreEqual(value1 + obfuscatedUInt2, obfuscatedAddition);

            ++value1;
            ++obfuscatedUInt1;
            Assert.AreEqual(value1, obfuscatedUInt1);
        }

        [Test]
        public void OperatorSubtraction([Values((uint)25)] uint value1, [Values((uint)5)] uint value2)
        {
            var obfuscatedUInt1 = new ObfuscatedUInt(value1);
            var obfuscatedUInt2 = new ObfuscatedUInt(value2);

            uint subtraction = value1 - value2;
            uint obfuscatedSubtraction = new ObfuscatedUInt(subtraction);

            Assert.AreEqual(obfuscatedUInt1 - obfuscatedUInt2, subtraction);
            Assert.AreEqual(obfuscatedUInt1 - value2, subtraction);
            Assert.AreEqual(value1 - obfuscatedUInt2, subtraction);
            Assert.AreEqual(obfuscatedUInt1 - obfuscatedUInt2, obfuscatedSubtraction);
            Assert.AreEqual(obfuscatedUInt1 - value2, obfuscatedSubtraction);
            Assert.AreEqual(value1 - obfuscatedUInt2, obfuscatedSubtraction);

            --value1;
            --obfuscatedUInt1;
            Assert.AreEqual(value1, obfuscatedUInt1);
        }

        [Test]
        public void OperatorMultiplication([Values((uint)11)] uint value1, [Values((uint)6)] uint value2)
        {
            var obfuscatedUInt1 = new ObfuscatedUInt(value1);
            var obfuscatedUInt2 = new ObfuscatedUInt(value2);

            uint multiplication = value1 * value2;
            uint obfuscatedMultiplication = new ObfuscatedUInt(multiplication);

            Assert.AreEqual(obfuscatedUInt1 * obfuscatedUInt2, multiplication);
            Assert.AreEqual(obfuscatedUInt1 * value2, multiplication);
            Assert.AreEqual(value1 * obfuscatedUInt2, multiplication);
            Assert.AreEqual(obfuscatedUInt1 * obfuscatedUInt2, obfuscatedMultiplication);
            Assert.AreEqual(obfuscatedUInt1 * value2, obfuscatedMultiplication);
            Assert.AreEqual(value1 * obfuscatedUInt2, obfuscatedMultiplication);
        }

        [Test]
        public void OperatorDivision([Values((uint)80)] uint value1, [Values((uint)8)] uint value2)
        {
            var obfuscatedUInt1 = new ObfuscatedUInt(value1);
            var obfuscatedUInt2 = new ObfuscatedUInt(value2);

            uint division = value1 / value2;
            uint obfuscatedDivision = new ObfuscatedUInt(division);

            Assert.AreEqual(obfuscatedUInt1 / obfuscatedUInt2, division);
            Assert.AreEqual(obfuscatedUInt1 / value2, division);
            Assert.AreEqual(value1 / obfuscatedUInt2, division);
            Assert.AreEqual(obfuscatedUInt1 / obfuscatedUInt2, obfuscatedDivision);
            Assert.AreEqual(obfuscatedUInt1 / value2, obfuscatedDivision);
            Assert.AreEqual(value1 / obfuscatedUInt2, obfuscatedDivision);
        }
    }
}