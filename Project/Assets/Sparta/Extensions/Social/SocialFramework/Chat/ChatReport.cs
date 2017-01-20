using SocialPoint.Attributes;
using SocialPoint.Utils;

namespace SocialPoint.Social
{
    public sealed class ChatReport
    {
        const string ReportedUserKey = "reported_id";
        const string TimestampKey = "ts";
        const string MessageKey = "message";
        const string MessageIdKey = "id";
        const string MessageTextKey = "text";
        const string ExtraDataKey = "extra_data";

        public string ReportedUid{ get; private set; }
        public long Ts{ get; private set; }
        public string MessageId{ get; private set; }
        public string MessageText{ get; private set; }
        public AttrDic ExtraData{ get; private set; }

        ChatReport()
        {

        }

        public ChatReport(BaseChatMessage message, AttrDic extraData)
        {
            ReportedUid = message.MessageData.PlayerId;
            Ts = TimeUtils.Timestamp;
            MessageId = message.Uuid;
            MessageText = message.Text;
            ExtraData = extraData;
        }

        public static ChatReport Parse(AttrDic data)
        {
            var report = new ChatReport();
            report.ReportedUid = data.GetValue(ReportedUserKey).ToString();
            report.Ts = data.GetValue(TimestampKey).ToLong();
            var messageData = data.Get(MessageKey).AsDic;
            report.MessageId = messageData.GetValue(MessageIdKey).ToString();
            report.MessageText = messageData.GetValue(MessageTextKey).ToString();
            report.ExtraData = data.Get(ExtraDataKey).AsDic;

            return report;
        }

        public AttrDic Serialize()
        {
            var data = new AttrDic();
            data.SetValue(ReportedUserKey, ReportedUid);
            data.SetValue(TimestampKey, Ts);
            data.Set(ExtraDataKey, ExtraData);

            var messageData = new AttrDic();
            messageData.SetValue(MessageIdKey, MessageId);
            messageData.SetValue(MessageTextKey, MessageText);

            data.Set(MessageKey, messageData);

            return data;
        }

        public override string ToString()
        {
            return string.Format("[ChatReport: ReportedUid={0}, Ts={1}, MessageId={2}, MessageText={3}, ExtraData={4}]", ReportedUid, Ts, MessageId, MessageText, ExtraData);
        }
    }
}
