
using NUnit.Framework;
using SocialPoint.Utils;
using System.Collections.Generic;
using System;

namespace SocialPoint.Attributes
{
    [TestFixture]
    [Category("SocialPoint.Attributes")]
    internal class UrlQueryAttrParserTests
    {

        [Test]
        public void SimpleTest()
        {        
            var parser = new UrlQueryAttrParser();

            var query = "test[aaa]=bbb&test[ccc]=ddd&test[ddd][]=eee";
            var data = parser.ParseString(query).AsDic;

            Assert.AreEqual(new AttrString("bbb"), data["test"].AsDic["aaa"]);
            Assert.AreEqual(new AttrList(new List<string>{ "eee" }), data["test"].AsDic["ddd"]);
        }

        [Test]
        public void ListTest()
        {        
            var parser = new UrlQueryAttrParser();

            var query = new Uri("http://google.com/?test[0]=value&test[1]=value2").Query;
            var data = parser.ParseString(query).AsDic;

            Assert.AreEqual(new AttrList(new List<string>{ "value", "value2" }), data["test"]);
        }

        [Test]
        public void ListDicTest()
        {        
            var parser = new UrlQueryAttrParser();

            var query = "test[0]=bbb&test[ccc]=ddd";
            var data = parser.ParseString(query).AsDic;

            Assert.AreEqual(new AttrString("bbb"), data["test"].AsDic["0"]);
            Assert.AreEqual(new AttrString("ddd"), data["test"].AsDic["ccc"]);
        }

        [Test]
        public void LongListTest()
        {        
            var parser = new UrlQueryAttrParser();

            var query = "test[2]=bbb";
            var data = parser.ParseString(query).AsDic;

            var expected = new AttrList();
            expected.Add(new AttrString());
            expected.Add(new AttrString());
            expected.Add(new AttrString("bbb"));
            Assert.AreEqual(expected, data["test"]);
        }

        [Test]
        public void ValueOverwriteTest()
        {        
            var parser = new UrlQueryAttrParser();

            var query = "test=bbb&test[]=ddd";
            var data = parser.ParseString(query).AsDic;

            Assert.AreEqual(new AttrList(new List<string>{ "ddd" }), data["test"]);
        }

        [Test]
        public void ComplexTest()
        {        
            var query = "test[aaa]=bbb&test[ccc]=ddd&test[ddd][]=eee&test[1][lolo][2][lulu][lele]=3234243234";
            var data = new UrlQueryAttrParser().ParseString(query);
            var requery = new UrlQueryAttrSerializer().SerializeString(data);
            var redata = new UrlQueryAttrParser().ParseString(requery);
            Assert.AreEqual(data, redata);
        }
    }
}