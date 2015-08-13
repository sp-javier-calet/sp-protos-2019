
using NUnit.Framework;
using SocialPoint.Utils;
using System.Runtime.Serialization;

namespace SocialPoint.Attributes
{

    [TestFixture]
    [Category("SocialPoint.Attributes")]
    internal class JsonAttrParserTests {

        [Test]
    	public void SimpleTest () {
    	
            var parser = new JsonAttrParser();

            var dic = parser.Parse(new Data("{}"));
            Assert.IsInstanceOf<AttrDic>(dic);
            Assert.AreEqual(0, (dic as AttrDic).Count);

            var list = parser.Parse(new Data("[]"));
            Assert.IsInstanceOf<AttrList>(list);
            Assert.AreEqual(0, (list as AttrList).Count);

    	}

        [Test]
        [ExpectedException(typeof(SerializationException))]
        public void InvalidTest () {
            var parser = new JsonAttrParser();
            parser.Parse(new Data("{{"));
        }

        [Test]
        [ExpectedException(typeof(SerializationException))]
        public void InvalidStringValueTest () {
            var parser = new JsonAttrParser();
            parser.Parse(new Data("\"a\\\"a\"bb\""));
        }

        [Test]
        public void ValidStringValueTest () {
            var parser = new JsonAttrParser();
            var val = parser.Parse(new Data("\"a\\\"a\\\"bb\""));
            Assert.IsInstanceOf<AttrValue>(val);
        }

        [Test]
        public void ValueTest () {

            var parser = new JsonAttrParser();

            var numval = parser.Parse(new Data("4444"));
            Assert.IsInstanceOf<AttrInt>(numval);
            Assert.AreEqual(4444, (numval as AttrValue).ToInt());
            
            var fracval = parser.Parse(new Data("444.4444"));
            Assert.IsInstanceOf<AttrDouble>(fracval);
            Assert.That(444.4444, Is.EqualTo((fracval as AttrValue).ToDouble()).Within(0.00001));

            var strval = parser.Parse(new Data("\"44\\\"44\""));
            Assert.IsInstanceOf<AttrString>(strval);
            Assert.AreEqual("44\"44", (strval as AttrValue).ToString());

        }

    }
}
