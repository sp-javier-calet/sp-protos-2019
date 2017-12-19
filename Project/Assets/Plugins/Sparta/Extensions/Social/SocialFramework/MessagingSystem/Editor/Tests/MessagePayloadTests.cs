using NUnit.Framework;
using NSubstitute;
using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    [TestFixture]
    [Category("SocialPoint.Social.MessagingSystem")]
    class MessagePayloadTests
    {
        [Test]
        public void PlainTextPayload()
        {
            const string kTitle = "Awesome Messaging System";
            const string kText = "Who did this component? It works really well!";
            var attrData = new AttrDic();
            attrData.SetValue("title", kTitle);
            attrData.SetValue("text", kText);

            var factory = new MessagePayloadPlainTextFactory();
            var payload = factory.CreatePayload(attrData);

            Assert.IsInstanceOf<MessagePayloadPlainText>(payload);
            var textPayload = payload as MessagePayloadPlainText;
            Assert.AreEqual(kTitle, textPayload.Title);
            Assert.AreEqual(kText, textPayload.Text);
        }
    }
}
