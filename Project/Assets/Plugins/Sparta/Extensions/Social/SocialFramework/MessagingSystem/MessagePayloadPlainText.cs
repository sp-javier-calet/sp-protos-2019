using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public class MessagePayloadPlainText : IMessagePayload
    {
        public string Title{ get; private set; }

        public string Text{ get; private set; }

        internal MessagePayloadPlainText(string title, string text)
        {
            Title = title;
            Text = text;
        }

        public string Identifier
        {
            get
            {
                return IdentifierKey;
            }
        }

        public AttrDic Serialize()
        {
            var dic = new AttrDic();
            dic.SetValue(MessagePayloadPlainTextFactory.kTitleKey, Title);
            dic.SetValue(MessagePayloadPlainTextFactory.kTextKey, Text);
            return dic;
        }

        public override string ToString()
        {
            return string.Format("[MessagePayloadPlainText: Title={0}, Text={1}]", Title, Text);
        }

        public const string IdentifierKey = "plain_text";
    }

    public sealed class MessagePayloadPlainTextFactory : IMessagePayloadFactory
    {
        public const string kTitleKey = "title";
        public const string kTextKey = "text";

        public IMessagePayload CreatePayload(AttrDic data)
        {
            return new MessagePayloadPlainText(data.GetValue(kTitleKey).ToString(), data.GetValue(kTextKey).ToString());
        }
    }
}
