
using NUnit.Framework;
using SocialPoint.Utils;
using System.Text;
using System.Runtime.Serialization;

namespace SocialPoint.Attributes
{
    [TestFixture]
    [Category("SocialPoint.Attributes")]
    internal class JsonAttrSerializerTests
    {

        [Test]
        public void SimpleTest()
        {        
            var serializer = new JsonAttrSerializer();

            var dic = new AttrDic();
            var list = new AttrList();
            dic["key"] = list;
            list.AddValue(1);
            list.AddValue(1.4);
            list.AddValue("string");

            var str = serializer.SerializeString(dic);

            Assert.AreEqual("{\"key\":[1,1.4,\"string\"]}", str);
        }

       

    }
}
