using NUnit.Framework;
using NSubstitute;
using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    [TestFixture]
    [Category("SocialPoint.Social.MessagingSystem")]
    class MessageTests
    {
        class MessageOrigin : IMessageOrigin
        {
            public string Identifier
            {
                get
                {
                    return "testOrigin";
                }
            }
        }

        class MessageOrigin2 : IMessageOrigin
        {
            public string Identifier
            {
                get
                {
                    return "testOrigin2";
                }
            }
        }

        class MessagePayload : IMessagePayload
        {
            public string Identifier
            {
                get
                {
                    return "testPayload";
                }
            }

            public AttrDic Serialize()
            {
                return Attr.InvalidDic;
            }
        }

        class MessagePayload2 : IMessagePayload
        {
            public string Identifier
            {
                get
                {
                    return "testPayload2";
                }
            }

            public AttrDic Serialize()
            {
                return Attr.InvalidDic;
            }
        }

        [Test]
        public void GetId()
        {
            const string messageId = "fake_id";
            var msg = new Message(messageId, 0, null, null);
            Assert.AreEqual(messageId, msg.Id);
        }

        [Test]
        public void GetTimestamp()
        {
            const int messageTs = 123456789;
            var msg = new Message("fake_id", messageTs, null, null);
            Assert.AreEqual(messageTs, msg.Timestamp);
        }

        [Test]
        public void GetOrigin()
        {
            var msg = new Message("fake_id", 0, new MessageOrigin(), null);
            var origin = msg.Origin<MessageOrigin>();
            Assert.IsNotNull(origin);
            Assert.AreEqual(origin.GetType(), typeof(MessageOrigin));
        }

        [Test]
        public void GetOriginInvalid()
        {
            var msg = new Message("fake_id", 0, new MessageOrigin(), null);
            var origin = msg.Origin<MessageOrigin2>();
            Assert.IsNull(origin);
        }

        [Test]
        public void GetPayload()
        {
            var msg = new Message("fake_id", 0, null, new MessagePayload());
            var payload = msg.Payload<MessagePayload>();
            Assert.IsNotNull(payload);
            Assert.AreEqual(payload.GetType(), typeof(MessagePayload));
        }

        [Test]
        public void GetPayloadInvalid()
        {
            var msg = new Message("fake_id", 0, null, new MessagePayload());
            var payload = msg.Payload<MessagePayload2>();
            Assert.IsNull(payload);
        }

        [Test]
        public void AddProperties()
        {
            const string Property1 = "prop1";
            const string Property2 = "prop2";
            var msg = new Message("fake_id", 0, null, null);

            Assert.IsFalse(msg.HasProperty(Property1));
            Assert.IsFalse(msg.HasProperty(Property2));

            msg.AddProperty(Property1);
            Assert.IsTrue(msg.HasProperty(Property1));
            Assert.IsFalse(msg.HasProperty(Property2));

            msg.AddProperty(Property2);
            Assert.IsTrue(msg.HasProperty(Property1));
            Assert.IsTrue(msg.HasProperty(Property2));
        }

        [Test]
        public void RemoveProperties()
        {
            const string Property1 = "prop1";
            const string Property2 = "prop2";
            var msg = new Message("fake_id", 0, null, null);

            msg.AddProperty(Property1);
            msg.AddProperty(Property2);

            Assert.IsTrue(msg.HasProperty(Property1));
            Assert.IsTrue(msg.HasProperty(Property2));

            msg.RemoveProperty(Property1);

            Assert.IsFalse(msg.HasProperty(Property1));
            Assert.IsTrue(msg.HasProperty(Property2));

            msg.RemoveProperty(Property2);

            Assert.IsFalse(msg.HasProperty(Property1));
            Assert.IsFalse(msg.HasProperty(Property2));
        }
    }
}
