
using NUnit.Framework;

namespace SocialPoint.Attributes
{
    [TestFixture]
    [Category("SocialPoint.Attributes")]
    internal class AttrTests
    {

        [Test]
        public void HashCodeTest()
        {       
            Assert.AreEqual(new AttrEmpty().GetHashCode(), new AttrEmpty().GetHashCode());
            Assert.AreNotEqual(new AttrEmpty().GetHashCode(), new AttrInt(0).GetHashCode());
            Assert.AreEqual(new AttrBool(true).GetHashCode(), new AttrBool(true).GetHashCode());
            Assert.AreNotEqual(new AttrBool(false).GetHashCode(), new AttrBool(true).GetHashCode());
            Assert.AreEqual(new AttrInt(0).GetHashCode(), new AttrInt(0).GetHashCode());
            Assert.AreNotEqual(new AttrInt(0).GetHashCode(), new AttrInt(1).GetHashCode());
            Assert.AreEqual(new AttrLong(0).GetHashCode(), new AttrLong(0).GetHashCode());
            Assert.AreNotEqual(new AttrLong(0).GetHashCode(), new AttrLong(1).GetHashCode());
            Assert.AreEqual(new AttrFloat(0.5f).GetHashCode(), new AttrFloat(0.5f).GetHashCode());
            Assert.AreNotEqual(new AttrFloat(0.5f).GetHashCode(), new AttrFloat(1.5f).GetHashCode());
            Assert.AreEqual(new AttrDouble(0.5).GetHashCode(), new AttrDouble(0.5).GetHashCode());
            Assert.AreNotEqual(new AttrDouble(0.5).GetHashCode(), new AttrDouble(1.5).GetHashCode());
			Assert.AreEqual(new AttrString("lala").GetHashCode(), new AttrString("lala").GetHashCode());
			Assert.AreNotEqual(new AttrString("lala").GetHashCode(), new AttrString("lalo").GetHashCode());

            var list1 = new AttrList();
            list1.AddValue(0);
            list1.AddValue(true);
            var list2 = new AttrList();
            list2.AddValue(0);
            list2.AddValue(true);
            Assert.AreEqual(list1, list2);
            list2.AddValue("new");
            Assert.AreNotEqual(list1, list2);

			var dic1 = new AttrDic();
			dic1.SetValue("key1", 0);
			dic1.SetValue("key2", true);
			dic1.Set("key3", list1);
			var dic2 = new AttrDic();
			dic2.SetValue("key1", 0);
			dic2.SetValue("key2", true);
			dic2.Set("key3", list1);
			Assert.AreEqual(dic1, dic2);
			dic2.Set("key2", list2);
			Assert.AreNotEqual(dic1, dic2);

        }

       

    }
}
