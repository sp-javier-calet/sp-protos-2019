using System;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Locale;

namespace SocialPoint.Social
{
    public class FactoryChatMessages<MessageType> 
        where MessageType : class, IChatMessage, new()
    {
        const string Tag = "SocialFramework";
        const string UserNameKey = "user_name";
        const string OldRoleKey = "old_role";
        const string NewRoleKey = "new_role";
        const string RankPromotionKey = "promotion";
        const string RankDemotionKey = "demotion";

        public Func<AttrDic, MessageType[]> ParseUnknownNotifications { private get; set; }

        public Action<MessageType, AttrDic> ParseExtraInfo { private get; set; }

        public Action<MessageType, AttrDic> SerializeExtraInfo { private get; set; }

        public Localization Localization;

        public MessageType Create(string text)
        {
            var message = new MessageType();
            message.Text = text;
            return message;
        }

        public MessageType CreateLocalized(string tid)
        {
            var message = new MessageType();
            message.Text = Localization.Get(tid);
            return message;
        }

        public MessageType CreateWarning(string text)
        {
            var message = Create(text);
            message.IsWarning = true;
            return message;
        }

        public MessageType CreateLocalizedWarning(string tid)
        {
            var message = CreateLocalized(tid);
            message.IsWarning = true;
            return message;
        }

        public AttrDic SerializeMessage(MessageType message)
        {
            var msgInfo = new AttrDic();

            if(SerializeExtraInfo != null)
            {
                SerializeExtraInfo(message, msgInfo);
            }

            msgInfo.SetValue(ConnectionManager.ChatMessageUuidKey, message.Uuid);
            msgInfo.SetValue(ConnectionManager.ChatMessageUserIdKey, message.PlayerId);
            msgInfo.SetValue(ConnectionManager.ChatMessageUserNameKey, message.PlayerName);
            msgInfo.SetValue(ConnectionManager.ChatMessageTsKey, message.Timestamp);
            msgInfo.SetValue(ConnectionManager.ChatMessageTextKey, message.Text);
            msgInfo.SetValue(ConnectionManager.ChatMessageLevelKey, message.PlayerLevel);
            msgInfo.SetValue(ConnectionManager.ChatMessageAllyNameKey, message.AllianceName);
            msgInfo.SetValue(ConnectionManager.ChatMessageAllyIdKey, message.AllianceId);
            msgInfo.SetValue(ConnectionManager.ChatMessageAllyAvatarKey, message.AllianceAvatarId);
            msgInfo.SetValue(ConnectionManager.ChatMessageAllyRoleKey, message.RankInAlliance);

            return msgInfo;
        }

        public MessageType[] ParseMessage(int type, AttrDic dic)
        {
            var messages = new MessageType[0];

            switch(type)
            {
            case NotificationTypeCode.NotificationAllianceMemberAccept:
            case NotificationTypeCode.NotificationAllianceMemberKickoff:
            case NotificationTypeCode.NotificationAllianceMemberPromote:
            case NotificationTypeCode.NotificationAlliancePlayerAutoPromote:
            case NotificationTypeCode.NotificationAlliancePlayerAutoDemote:
            case NotificationTypeCode.BroadcastAllianceEdit:
            case NotificationTypeCode.BroadcastAllianceOnlineMember:
            case NotificationTypeCode.NotificationUserChatBan:
                break;

            case NotificationTypeCode.TextMessage:
                messages = ParseChatMessage(dic);
                break;
            
            case NotificationTypeCode.BroadcastAllianceMemberAccept:
            case NotificationTypeCode.BroadcastAllianceJoin:
                messages = ParsePlayerJoinedMessage(dic);
                break;

            case NotificationTypeCode.BroadcastAllianceMemberKickoff:
            case NotificationTypeCode.BroadcastAllianceMemberLeave:
                messages = ParsePlayerLeftMessage(dic);
                break;

            case NotificationTypeCode.BroadcastAllianceMemberPromote:
                messages = ParseMemberPromotedMessage(dic);
                break;

            case NotificationTypeCode.BroadcastAllianceMemberRankChange:
                messages = ParseTwoMembersAutoPromotedMessage(dic);
                break;

            case NotificationTypeCode.NotificationAllianceJoinRequest:
                messages = ParseJoinRequestMessage(dic);
                break;
            default:
                if(ParseUnknownNotifications != null)
                {
                    messages = ParseUnknownNotifications(dic);
                }
                break;
            }

            return messages;
        }

        MessageType[] ParseChatMessage(AttrDic dic)
        {
            if(!dic.ContainsKey(ConnectionManager.ChatMessageInfoKey))
            {
                Log.e(Tag, "Received chat message of text type does not contain the main parent");
                return new MessageType[0];
            }

            var msgInfo = dic.Get(ConnectionManager.ChatMessageInfoKey).AsDic;
            if(!Validate(msgInfo, new string[] {
                ConnectionManager.ChatMessageUserIdKey,
                ConnectionManager.ChatMessageUserNameKey,
                ConnectionManager.ChatMessageTsKey,
                ConnectionManager.ChatMessageTextKey,
                ConnectionManager.ChatMessageLevelKey,
                ConnectionManager.ChatMessageAllyNameKey,
                ConnectionManager.ChatMessageAllyIdKey,
                ConnectionManager.ChatMessageAllyAvatarKey,
                ConnectionManager.ChatMessageAllyRoleKey
            }))
            {
                Log.e(Tag, "Received chat message of text type does not contain all the mandatory fields");
                return new MessageType[0];
            }

            var message = Create(msgInfo.GetValue(ConnectionManager.ChatMessageTextKey).ToString());

            message.PlayerId = msgInfo.GetValue(ConnectionManager.ChatMessageUserIdKey).ToString();
            message.PlayerName = msgInfo.GetValue(ConnectionManager.ChatMessageUserNameKey).ToString();
            message.PlayerLevel = msgInfo.GetValue(ConnectionManager.ChatMessageLevelKey).ToInt();
            message.Timestamp = msgInfo.GetValue(ConnectionManager.ChatMessageTsKey).ToLong();
           
            message.AllianceName = msgInfo.GetValue(ConnectionManager.ChatMessageAllyNameKey).ToString();
            message.AllianceId = msgInfo.GetValue(ConnectionManager.ChatMessageAllyIdKey).ToString();
            message.HasAlliance = !string.IsNullOrEmpty(message.AllianceId);
            message.AllianceAvatarId = msgInfo.GetValue(ConnectionManager.ChatMessageAllyAvatarKey).ToInt();
            message.RankInAlliance = msgInfo.GetValue(ConnectionManager.ChatMessageAllyRoleKey).ToInt();

            if(ParseExtraInfo != null)
            {
                ParseExtraInfo(message, msgInfo);
            }

            return new MessageType[]{ message };
        }

        MessageType[] ParsePlayerJoinedMessage(AttrDic dic)
        {   
            if(!Validate(dic, UserNameKey))
            {
                Log.e(Tag, "Received chat message of player joined type does not contain all the mandatory fields");
                return new MessageType[0];
            }

            var playerName = dic.GetValue(UserNameKey).ToString();
            var message = CreateWarning(string.Format(Localization.Get("socialFramework.ChatPlayerJoined"), playerName));

            return new MessageType[] { message };
        }

        MessageType[] ParsePlayerLeftMessage(AttrDic dic)
        {
            if(!Validate(dic, UserNameKey))
            {
                Log.e(Tag, "Received chat message of player left type does not contain all the mandatory fields");
                return new MessageType[0];
            }

            var playerName = dic.GetValue(UserNameKey).ToString();
            var message = CreateWarning(string.Format(Localization.Get("socialFramework.ChatPlayerLeft"), playerName));

            return new MessageType[] { message };
        }

        MessageType[] ParseMemberPromotedMessage(AttrDic dic)
        {
            if(!Validate(dic, new string[] { 
                UserNameKey, 
                OldRoleKey, 
                NewRoleKey 
            }))
            {
                Log.e(Tag, "Received chat message of member promoted type does not contain all the mandatory fields");
                return new MessageType[0];
            }

            var playerName = dic.GetValue(UserNameKey).ToString();
            // TODO Alliance management.
            var message = CreateWarning(playerName);
            return new MessageType[] { message };
        }

        MessageType[] ParseTwoMembersAutoPromotedMessage(AttrDic dic)
        {
            if(!Validate(dic, new string[] { 
                RankPromotionKey, 
                RankDemotionKey
            }))
            {
                Log.e(Tag, "Received chat message of two members changed rank type does not contain all the mandatory fields");
                return new MessageType[0];
            }

            // TODO Alliance management.
            return new MessageType[0];
        }

        MessageType[] ParseJoinRequestMessage(AttrDic dic)
        {
            var message = CreateWarning(Localization.Get("socialFramework.ChatJoinRequest"));
            return new MessageType[] { message };
        }

        bool Validate(AttrDic dic, string requiredValue)
        {
            return Validate(dic, new string[]{ requiredValue });
        }

        bool Validate(AttrDic dic, string[] requiredValues)
        {
            for(int i = 0; i < requiredValues.Length; ++i)
            {
                if(Attr.IsNullOrEmpty(dic.Get(requiredValues[i])))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
