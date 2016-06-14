﻿//  Author:
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

        [Test]
        public void Remove_Dictionary()
        {
            AttrDic data = new AttrDic();
            data.SetValue("foo", "bar");

            AttrList patch = new AttrList();
            AttrDic op = new AttrDic();
            op.SetValue("op", "remove");
            op.SetValue("path", "/foo");
            patch.Add(op);

            _patcher.Patch(patch, data);

            Assert.That(data.Count == 0);
        }

        [Test]
        public void Remove_List()
        {
            AttrDic data = new AttrDic();
            AttrList dataList = new AttrList();
            dataList.AddValue(1);
            dataList.AddValue(2);
            dataList.AddValue(3);
            data.Set("foo", dataList);

            AttrList patch = new AttrList();
            AttrDic op = new AttrDic();
            op.SetValue("op", "remove");
            op.SetValue("path", "/foo/1");
            patch.Add(op);

            _patcher.Patch(patch, data);

            Assert.That(data.Get("foo").AsList.Count == 2);
            Assert.That(data.Get("foo").AsList.GetValue(1).ToInt() == 3);
        }

        [Test]
        public void Remove_Complex()
        {
            AttrDic data = new AttrDic();
            AttrList dataList = new AttrList();
            AttrDic dataDict = new AttrDic();
            dataDict.SetValue("bar", "baz");
            dataList.Add(dataDict);
            data.Set("foo", dataList);

            AttrList patch = new AttrList();
            AttrDic op = new AttrDic();
            op.SetValue("op", "remove");
            op.SetValue("path", "/foo/0/bar");
            patch.Add(op);

            _patcher.Patch(patch, data);

            Assert.That(data.Get("foo").AsList.Get(0).AsDic.Count == 0);
        }

        [Test]
        public void Replace_Dictionary()
        {
            AttrDic data = new AttrDic();
            data.SetValue("foo", "bar");

            AttrList patch = new AttrList();
            AttrDic op = new AttrDic();
            op.SetValue("op", "replace");
            op.SetValue("path", "/foo");
            op.SetValue("value", 1);
            patch.Add(op);

            _patcher.Patch(patch, data);

            Assert.That(data.Count == 1);
            Assert.That(data.GetValue("foo").AttrValueType == AttrValueType.INT);
            Assert.That(data.GetValue("foo").ToInt() == 1);
        }

        [Test]
        public void Replace_List()
        {
            AttrDic data = new AttrDic();
            AttrList dataList = new AttrList();
            dataList.AddValue(1);
            dataList.AddValue(2);
            dataList.AddValue(3);
            data.Set("foo", dataList);

            AttrList patch = new AttrList();
            AttrDic op = new AttrDic();
            op.SetValue("op", "replace");
            op.SetValue("path", "/foo/1");
            op.SetValue("value", 4);
            patch.Add(op);

            _patcher.Patch(patch, data);

            Assert.That(data.Get("foo").AsList.Count == 3);
            Assert.That(data.Get("foo").AsList.GetValue(1).ToInt() == 4);
            Assert.That(data.Get("foo").AsList.GetValue(2).ToInt() == 3);
        }

        [Test]
        public void Replace_Complex()
        {
            AttrDic data = new AttrDic();
            AttrList dataList = new AttrList();
            AttrDic dataDict = new AttrDic();
            dataDict.SetValue("bar", "baz");
            dataList.Add(dataDict);
            data.Set("foo", dataList);

            AttrList patch = new AttrList();
            AttrDic op = new AttrDic();
            op.SetValue("op", "replace");
            op.SetValue("path", "/foo/0/bar");
            op.SetValue("value", 1);
            patch.Add(op);

            _patcher.Patch(patch, data);

            Assert.That(data.Get("foo").AsList.Get(0).AsDic.Count == 1);
            Assert.That(data.Get("foo").AsList.Get(0).AsDic.GetValue("bar").ToInt() == 1);
            Assert.That(data.Get("foo").AsList.Get(0).AsDic.GetValue("bar").AttrValueType == AttrValueType.INT);
        }
    }
}

