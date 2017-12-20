
using NUnit.Framework;
using SocialPoint.Utils;
using System.Text;
using System.Runtime.Serialization;

namespace SocialPoint.Attributes
{
    [TestFixture]
    [Category("SocialPoint.Attributes")]
    internal class UrlQueryAttrSerializerTests
    {

        [Test]
        public void SimpleTest()
        {        
            var serializer = new UrlQueryAttrSerializer();

            var dic = new AttrDic();
            var list = new AttrList();
            dic["key"] = list;
            list.AddValue(1);
            list.AddValue(1.4);
            list.AddValue("str=ing");

            var str = serializer.SerializeString(dic);

            Assert.AreEqual("key[]=1&key[]=1.4&key[]=str%3Ding", str);
        }

       

    }
}
