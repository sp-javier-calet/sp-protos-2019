
using NUnit.Framework;
using NSubstitute;
using System;

namespace SocialPoint.Attributes
{
    [TestFixture]
    [Category("SocialPoint.Attributes")]
    internal class TransitionAttrStorageTests
    {

        [Test]
        public void LoadFromFromTest()
        {       
            var from = Substitute.For<IAttrStorage>();
            var to = Substitute.For<IAttrStorage>();

            var key = "test";
            var value = new AttrString();
            from.Load(key).Returns(value);


            var storage = new TransitionAttrStorage(from, to);
            storage.Load(key);

            from.Received(1).Load(key);
            to.Received(0).Load(key);
            to.Received(1).Save(key, value);
        }

        [Test]
        public void LoadFromToTest()
        {       
            var from = Substitute.For<IAttrStorage>();
            var to = Substitute.For<IAttrStorage>();

            var key = "test";
            var value = new AttrString();
            from.Load(key).Returns((x) => null);
            to.Load(key).Returns(value);
            
            var storage = new TransitionAttrStorage(from, to);
            storage.Load(key);
            
            from.Received(1).Load(key);
            to.Received(1).Load(key);
            to.Received(0).Save(key, value);
            from.Received(1).Save(key, value);
        }

        [Test]
        public void HasTest()
        {       
            var from = Substitute.For<IAttrStorage>();
            var to = Substitute.For<IAttrStorage>();
            
            var key = "test";
            from.Has(key).Returns(true);
            to.Has(key).Returns(false);
            var storage = new TransitionAttrStorage(from, to);

            Assert.IsTrue(storage.Has(key));
        }

        [Test]
        public void SaveTest()
        {  
            var from = Substitute.For<IAttrStorage>();
            var to = Substitute.For<IAttrStorage>();
            
            var key = "test";
            var value = new AttrString();
            var storage = new TransitionAttrStorage(from, to);

            storage.Save(key, value);

            from.Received(1).Save(key, value);
            to.Received(1).Save(key, value);
        }

        [Test]
        public void RemoveTest()
        {  
            var from = Substitute.For<IAttrStorage>();
            var to = Substitute.For<IAttrStorage>();
            
            var key = "test";
            var storage = new TransitionAttrStorage(from, to);
            
            storage.Remove(key);
            
            from.Received(1).Remove(key);
            to.Received(1).Remove(key);
        }

        [Test]
        public void SaveExceptionOldValueTest()
        {  
            var from = Substitute.For<IAttrStorage>();
            var to = Substitute.For<IAttrStorage>();
            
            var key = "test";
            var value = new AttrString();
            var oldFrom = new AttrInt(5);
            var storage = new TransitionAttrStorage(from, to);

            from.Load(key).Returns(oldFrom);
            to.When(x => x.Save(key, value)).Do((x) => { throw new Exception(); });

            Assert.Throws<Exception> (() => {
                storage.Save(key, value);
            });
            
            from.Received(1).Save(key, value);
            to.Received(1).Save(key, value);
            from.Received(1).Load(key);
            from.Received(1).Save(key, oldFrom);
        }

        [Test]
        public void SaveExceptionOldRemoveTest()
        {  
            var from = Substitute.For<IAttrStorage>();
            var to = Substitute.For<IAttrStorage>();
            
            var key = "test";
            var value = new AttrString();
            var storage = new TransitionAttrStorage(from, to);
            
            from.Load(key).Returns((x) => null);
            to.When(x => x.Save(key, value)).Do((x) => { throw new Exception(); });
            
            Assert.Throws<Exception> (() => {
                storage.Save(key, value);
            });
            
            from.Received(1).Save(key, value);
            to.Received(1).Save(key, value);
            from.Received(1).Load(key);
            from.Received(1).Remove(key);
        }

        [Test]
        public void RemoveExceptionOldValueTest()
        {  
            var from = Substitute.For<IAttrStorage>();
            var to = Substitute.For<IAttrStorage>();
            
            var key = "test";
            var oldFrom = new AttrInt(5);
            var storage = new TransitionAttrStorage(from, to);
            
            from.Load(key).Returns(oldFrom);
            to.When(x => x.Remove(key)).Do((x) => { throw new Exception(); });
            
            Assert.Throws<Exception> (() => {
                storage.Remove(key);
            });
            
            from.Received(1).Remove(key);
            to.Received(1).Remove(key);
            from.Received(1).Load(key);
            from.Received(1).Save(key, oldFrom);
        }
        
        [Test]
        public void RemoveExceptionOldRemoveTest()
        {  
            var from = Substitute.For<IAttrStorage>();
            var to = Substitute.For<IAttrStorage>();
            
            var key = "test";
            var storage = new TransitionAttrStorage(from, to);
            
            from.Load(key).Returns((x) => null);
            to.When(x => x.Remove(key)).Do((x) => { throw new Exception(); });
            
            Assert.Throws<Exception> (() => {
                storage.Remove(key);
            });
            
            from.Received(1).Remove(key);
            to.Received(1).Remove(key);
            from.Received(1).Load(key);
            from.Received(0).Save(key, null);
        }

    }
}
