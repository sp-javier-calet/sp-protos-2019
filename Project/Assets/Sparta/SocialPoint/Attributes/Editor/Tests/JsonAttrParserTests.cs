using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;

namespace SocialPoint.Attributes
{
    [TestFixture]
    [Category("SocialPoint.Attributes")]
    internal class JsonAttrParserTests
    {
        [Test]
        public void SimpleTest()
        {
            var parser = new JsonAttrParser();

            var dic = parser.ParseString("{}");
            Assert.IsInstanceOf<AttrDic>(dic);
            Assert.AreEqual(0, (dic as AttrDic).Count);

            var list = parser.Parse(Encoding.UTF8.GetBytes("[]"));
            Assert.IsInstanceOf<AttrList>(list);
            Assert.AreEqual(0, (list as AttrList).Count);
        }

        [Test]
        public void ValidStringValueTest()
        {
            var parser = new JsonAttrParser();
            var val = parser.ParseString("\"a\\\"a\\\"bb\"");
            Assert.IsInstanceOf<AttrValue>(val);
        }

        static void InvalidTestDelegate()
        {
            var parser = new JsonAttrParser();
            parser.ParseString("{{");
        }

        static void InvalidStringValueTestDelegate()
        {
            var parser = new JsonAttrParser();
            parser.ParseString("\"a\\\"a\"bb\"");
        }

        #if UNITY_5_6_OR_NEWER

        [Test]
        public void InvalidTest()
        {
            Assert.Throws(typeof(SerializationException), InvalidTestDelegate);
        }

        [Test]
        public void InvalidStringValueTest()
        {
            Assert.Throws(typeof(SerializationException), InvalidStringValueTestDelegate);
        }

        #else
        
        [Test]
        [ExpectedException(typeof(SerializationException))]
        public void InvalidTest()
        {
            InvalidTestDelegate();
        }

        [Test]
        [ExpectedException(typeof(SerializationException))]
        public void InvalidStringValueTest()
        {
            InvalidStringValueTestDelegate();
        }

        #endif

        [Test]
        public void ValueTest()
        {
            var parser = new JsonAttrParser();

            var numval = parser.ParseString("4444");
            Assert.IsInstanceOf<AttrInt>(numval);
            Assert.AreEqual(4444, (numval as AttrValue).ToInt());
            
            var fracval = parser.ParseString("444.4444");
            Assert.IsInstanceOf<AttrDouble>(fracval);
            Assert.That(444.4444, Is.EqualTo((fracval as AttrValue).ToDouble()).Within(0.00001));

            var strval = parser.ParseString("\"44\\\"44\"");
            Assert.IsInstanceOf<AttrString>(strval);
            Assert.AreEqual("44\"44", (strval as AttrValue).ToString());
        }

    }
}
