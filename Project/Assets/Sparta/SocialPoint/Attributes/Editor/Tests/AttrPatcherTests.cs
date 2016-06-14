//  Author:
//    Miguel-Janer 

using System;
using NUnit.Framework;

namespace SocialPoint.Attributes
{
    [TestFixture]
    [Category("SocialPoint.Attributes")]
    public class AttrPatcherTests
    {
        AttrPatcher _patcher;

        [SetUp]
        public void Setup()
        {
            _patcher = new AttrPatcher();
        }

        [Test]
        public void Add_Dictionary()
        {
            AttrDic data = new AttrDic();
            AttrList patch = new AttrList();

            AttrDic op = new AttrDic();
            op.SetValue("op", "add");
            op.SetValue("path", "/foo");
            op.SetValue("value", "bar");
            patch.Add(op);

            _patcher.Patch(patch, data);

            Assert.That(data.ContainsKey("foo"));
            Assert.That(data.GetValue("foo").ToString().Equals("bar"));
        }

        [Test]
        public void Add_List()
        {
            AttrDic data = new AttrDic();
            AttrList dataList = new AttrList();
            dataList.AddValue(1);
            dataList.AddValue(3);
            data.Set("foo", dataList);

            AttrList patch = new AttrList();

            AttrDic op = new AttrDic();
            op.SetValue("op", "add");
            op.SetValue("path", "/foo/1");
            op.SetValue("value", 2);
            patch.Add(op);

            _patcher.Patch(patch, data);

            Assert.That(data.ContainsKey("foo"));
            Assert.That(data.Get("foo").AsList.Get(1).AsValue.ToInt() == 2);
        }

        [Test]
        public void Add_Complex()
        {
            AttrDic data = new AttrDic();
            AttrList dataList = new AttrList();
            AttrDic dataDict = new AttrDic();
            dataList.Add(dataDict);
            data.Set("foo", dataList);

            AttrList patch = new AttrList();
            AttrDic op = new AttrDic();
            op.SetValue("op", "add");
            op.SetValue("path", "/foo/0/bar");
            op.SetValue("value", "baz");
            patch.Add(op);

            _patcher.Patch(patch, data);
            Assert.That("baz" == data.Get("foo").AsList.Get(0).AsDic.Get("bar").ToString());
        }
    }
}

