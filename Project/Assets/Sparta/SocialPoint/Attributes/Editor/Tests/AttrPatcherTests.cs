//  Author:
//    Miguel-Janer 

using System;
using NUnit.Framework;

namespace SocialPoint.Attributes
{
    [TestFixture]
    [Category ("SocialPoint.Attributes")]
    public class AttrPatcherTests
    {
        AttrPatcher _patcher;

        [SetUp]
        public void Setup()
        {
            _patcher = new AttrPatcher();
        }

        [Test]
        public void Add()
        {
            AttrDic data = new AttrDic();
            AttrList patch = new AttrList();

            AttrDic op = new AttrDic();
            op.SetValue("op", "add");
            op.SetValue("path", "/foo");
            op.SetValue("value", "bar");
            patch.Add(op);

            _patcher.Patch(patch,data);

            Assert.That(data.ContainsKey("foo"));
            Assert.That(data.GetValue("foo").ToString().Equals("bar"));
        }
    }
}

