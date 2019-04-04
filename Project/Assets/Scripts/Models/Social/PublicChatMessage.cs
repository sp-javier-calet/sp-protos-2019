//-----------------------------------------------------------------------
// PublicChatMessage.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.Attributes;
using SocialPoint.Social;

public class PublicChatMessage : BaseChatMessage
{
    public static PublicChatMessage[] ParseUnknownNotifications(AttrDic dic)
    {
        return new PublicChatMessage[0];
    }

    public static void ParseExtraInfo(PublicChatMessage message, AttrDic dic)
    {
    }

    public static void SerializeExtraInfo(PublicChatMessage message, AttrDic dic)
    {
    }
}
