using SocialPoint.Attributes;
using SocialPoint.Social;

public class AllianceChatMessage : BaseChatMessage
{
    public static AllianceChatMessage[] ParseUnknownNotifications(AttrDic dic)
    {
        return new AllianceChatMessage[0];
    }

    public static void ParseExtraInfo(AllianceChatMessage message, AttrDic dic)
    {
    }

    public static void SerializeExtraInfo(AllianceChatMessage message, AttrDic dic)
    {
    }
}
