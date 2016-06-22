
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
            Assert.AreEqual(list1.GetHashCode(), list2.GetHashCode());
            list2.AddValue("new");
            Assert.AreNotEqual(list1.GetHashCode(), list2.GetHashCode());

            var dic1 = new AttrDic();
            dic1.SetValue("key1", 0);
            dic1.SetValue("key2", true);
            dic1.Set("key3", list1);
            var dic2 = new AttrDic();
            dic2.SetValue("key1", 0);
            dic2.SetValue("key2", true);
            dic2.Set("key3", list1);
            Assert.AreEqual(dic1.GetHashCode(), dic2.GetHashCode());
            dic2.Set("key2", list2);
            Assert.AreNotEqual(dic1.GetHashCode(), dic2.GetHashCode());

            var sync1 = new AttrDic();
            sync1.SetValue("level", 0);
            var syncData1 = new AttrDic();
            syncData1.SetValue("cash", 360);
            sync1.Set("resources", syncData1);
            var sync2 = new AttrDic();
            sync2.SetValue("level", 0);
            var syncData2 = new AttrDic();
            syncData2.SetValue("cash", 360);
            sync2.Set("resources", syncData2);
            Assert.AreEqual(sync1.GetHashCode(), sync2.GetHashCode());
            sync2.SetValue("level", 1);
            Assert.AreNotEqual(sync1.GetHashCode(), sync2.GetHashCode());
        }

        [Test]
        public void Insert_at_beggining()
        {
            var list = new AttrList();
            list.AddValue(1);
            list.AddValue(2);
            list.InsertValue(0,3);
            Assert.That(list.Count == 3);
            Assert.That(list.GetValue(0).ToInt() == 3);
        }

        [Test]
        public void Insert_at_end()
        {
            var list = new AttrList();
            list.AddValue(1);
            list.AddValue(2);
            list.InsertValue(2,3);
            Assert.That(list.Count == 3);
            Assert.That(list.GetValue(2).ToInt() == 3);
        }

        [Test]
        public void Insert_at_out_of_range()
        {
            var list = new AttrList();
            list.AddValue(1);
            list.AddValue(2);
            var success = list.InsertValue(3,3);
            Assert.That(list.Count == 2);
            Assert.That(success == false);
        }

    }
}
