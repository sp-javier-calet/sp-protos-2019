using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    class MessagePayloadPlainText : IMessagePayload
    {
        public string Title{ get; private set; }

        public string Text{ get; }

        internal MessagePayloadPlainText(string title, string text)
        {
            Title = title;
            Text = text;
        }

        public string GetIdentifier()
        {
            return Identifier;
        }

        public AttrDic Serialize()
        {
            var dic = new AttrDic();
            dic.SetValue(MessagePayloadPlainTextFactory.kTitleKey, Title);
            dic.SetValue(MessagePayloadPlainTextFactory.kTextKey, Text);
            return dic;
        }

        public const string Identifier = "plain_text";
    }

    class MessagePayloadPlainTextFactory : IMessagePayloadFactory
    {
        public const string kTitleKey = "title";
        public const string kTextKey = "text";

        public IMessagePayload CreatePayload(AttrDic data)
        {
            return new MessagePayloadPlainText(data.GetValue(kTitleKey).ToString(), data.GetValue(kTextKey).ToString());
        }
    }
}
