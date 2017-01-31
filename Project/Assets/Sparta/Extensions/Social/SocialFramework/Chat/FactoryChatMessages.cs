using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Locale;

namespace SocialPoint.Social
{
    public class FactoryChatMessages<MessageType> 
        where MessageType : class, IChatMessage, new()
    {
        const string Tag = "SocialFramework";

        #region Attr keys

        const string UserIdKey = "user_id";
        const string UserNameKey = "user_name";
        const string UserNameTwoDaysLaterKey = "UserName";
        const string OldRoleKey = "old_role";
        const string NewRoleKey = "new_role";
        const string NewRoleTwoDaysLaterKey = "NewRole";
        const string RankPromotionKey = "promotion";
        const string RankDemotionKey = "demotion";

        #endregion

        public Func<AttrDic, MessageType[]> ParseUnknownNotifications { private get; set; }

        public Action<MessageType, AttrDic> ParseExtraInfo { private get; set; }

        public Action<MessageType, AttrDic> SerializeExtraInfo { private get; set; }

        public IRankManager RankManager;

        public Localization Localization;

        readonly HashSet<int> _filteredMessageTypes;

        public HashSet<int> FilteredMessageTypes
        {
            get
            {
                return _filteredMessageTypes;
            }
        }

        public FactoryChatMessages()
        {
            _filteredMessageTypes = new HashSet<int>();
        }

        public MessageType Create(int type, string text)
        {
            if(_filteredMessageTypes.Contains(type))
            {
                return null;
            }
            var message = new MessageType();
            message.Type = type;
            message.Text = text;
            return message;
        }

        public MessageType CreateLocalized(int type, string tid)
        {
            return Create(type, Localization.Get(tid));
        }

        public MessageType CreateWarning(int type, string text)
        {
            var message = Create(type, text);
            if(message != null)
            {
                message.IsWarning = true;
            }
            return message;
        }

        public MessageType CreateLocalizedWarning(int type, string tid)
        {
            var message = CreateLocalized(type, tid);
            if(message != null)
            {
                message.IsWarning = true;
            }
            return message;
        }

        public AttrDic SerializeMessage(MessageType message)
        {
            var msgInfo = new AttrDic();

            if(SerializeExtraInfo != null)
            {
                SerializeExtraInfo(message, msgInfo);
            }

            var data = message.MessageData;

            msgInfo.SetValue(ConnectionManager.ChatMessageUuidKey, message.Uuid);
            msgInfo.SetValue(ConnectionManager.ChatMessageUserIdKey, data.PlayerId);
            msgInfo.SetValue(ConnectionManager.ChatMessageUserNameKey, data.PlayerName);
            msgInfo.SetValue(ConnectionManager.ChatMessageTsKey, message.Timestamp);
            msgInfo.SetValue(ConnectionManager.ChatMessageTextKey, message.Text);
            msgInfo.SetValue(ConnectionManager.ChatMessageLevelKey, data.PlayerLevel);
            msgInfo.SetValue(ConnectionManager.ChatMessageAllyNameKey, data.AllianceName);
            msgInfo.SetValue(ConnectionManager.ChatMessageAllyIdKey, data.AllianceId);
            msgInfo.SetValue(ConnectionManager.ChatMessageAllyAvatarKey, data.AllianceAvatarId);
            msgInfo.SetValue(ConnectionManager.ChatMessageAllyRoleKey, data.RankInAlliance);

            return msgInfo;
        }

        public MessageType[] ParseMessage(int type, AttrDic dic)
        {
            var messages = new MessageType[0];

            switch(type)
            {
            case NotificationType.NotificationAllianceMemberAccept:
            case NotificationType.NotificationAllianceMemberKickoff:
            case NotificationType.NotificationAllianceMemberPromote:
            case NotificationType.NotificationAlliancePlayerAutoPromote:
            case NotificationType.NotificationAlliancePlayerAutoDemote:
            case NotificationType.BroadcastAllianceEdit:
            case NotificationType.BroadcastAllianceOnlineMember:
            case NotificationType.NotificationUserChatBan:
                break;

            case NotificationType.TextMessage:
                messages = ParseChatMessage(type, dic);
                break;
            
            case NotificationType.BroadcastAllianceMemberAccept:
            case NotificationType.BroadcastAllianceJoin:
                messages = ParsePlayerJoinedMessage(type, dic);
                break;

            case NotificationType.BroadcastAllianceMemberKickoff:
            case NotificationType.BroadcastAllianceMemberLeave:
                messages = ParsePlayerLeftMessage(type, dic);
                break;

            case NotificationType.BroadcastAllianceMemberPromote:
                messages = ParseMemberPromotedMessage(type, dic);
                break;

            case NotificationType.NotificationAllianceJoinRequest:
                messages = ParseJoinRequestMessage(type, dic);
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

        MessageType[] ParseChatMessage(int type, AttrDic dic)
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

            var message = Create(type, msgInfo.GetValue(ConnectionManager.ChatMessageTextKey).ToString());
            if(message == null)
            {
                return new MessageType[]{ };
            }

            var data = new MessageData();

            data.PlayerId = msgInfo.GetValue(ConnectionManager.ChatMessageUserIdKey).ToString();
            data.PlayerName = msgInfo.GetValue(ConnectionManager.ChatMessageUserNameKey).ToString();
            data.PlayerLevel = msgInfo.GetValue(ConnectionManager.ChatMessageLevelKey).ToInt();
            data.AllianceName = msgInfo.GetValue(ConnectionManager.ChatMessageAllyNameKey).ToString();
            data.AllianceId = msgInfo.GetValue(ConnectionManager.ChatMessageAllyIdKey).ToString();
            data.AllianceAvatarId = msgInfo.GetValue(ConnectionManager.ChatMessageAllyAvatarKey).ToInt();
            data.RankInAlliance = msgInfo.GetValue(ConnectionManager.ChatMessageAllyRoleKey).ToInt();
            message.Timestamp = msgInfo.GetValue(ConnectionManager.ChatMessageTsKey).ToLong();

            message.MessageData = data;

            if(ParseExtraInfo != null)
            {
                ParseExtraInfo(message, msgInfo);
            }

            return new MessageType[]{ message };
        }

        MessageType[] ParsePlayerJoinedMessage(int type, AttrDic dic)
        {   
            if(!Validate(dic, UserNameKey))
            {
                Log.e(Tag, "Received chat message of player joined type does not contain all the mandatory fields");
                return new MessageType[0];
            }

            var playerName = dic.GetValue(UserNameKey).ToString();
            var message = CreateWarning(type, string.Format(Localization.Get(SocialFrameworkStrings.ChatPlayerJoinedKey), playerName));
            if(message == null)
            {
                return new MessageType[]{ };
            }
            else
            {
                return new MessageType[] { message };
            }
        }

        MessageType[] ParsePlayerLeftMessage(int type, AttrDic dic)
        {
            if(!Validate(dic, UserNameKey))
            {
                Log.e(Tag, "Received chat message of player left type does not contain all the mandatory fields");
                return new MessageType[0];
            }

            var playerName = dic.GetValue(UserNameKey).ToString();
            var message = CreateWarning(type, string.Format(Localization.Get(SocialFrameworkStrings.ChatPlayerLeftKey), playerName));
            if(message == null)
            {
                return new MessageType[]{ };
            }
            else
            {
                return new MessageType[] { message };
            }
        }

        MessageType[] ParseMemberPromotedMessage(int type, AttrDic dic)
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
            var oldRank = dic.GetValue(OldRoleKey).ToInt();
            var newRank = dic.GetValue(NewRoleKey).ToInt();
            // TODO Move localization to RankManager?
            var messageTid = RankManager.GetRankChangeMessageTid(oldRank, newRank);

            if(string.IsNullOrEmpty(messageTid))
            {
                return new MessageType[0];
            }

            var oldRankName = Localization.Get(RankManager.GetRankNameTid(oldRank));
            var newRankName = Localization.Get(RankManager.GetRankNameTid(newRank));
            var message = CreateWarning(type, string.Format(Localization.Get(messageTid), playerName, oldRankName, newRankName));
            if(message == null)
            {
                return new MessageType[]{ };
            }

            var data = new MemberPromotionData();
            data.PlayerName = playerName;
            data.OldRank = oldRank;
            data.NewRank = newRank;
            message.MemberPromotionData = data;

            return new MessageType[] { message };
        }

        MessageType[] ParseJoinRequestMessage(int type, AttrDic dic)
        {

            var playerId = dic.GetValue(UserIdKey).ToString();
            var message = CreateWarning(type, Localization.Get(SocialFrameworkStrings.ChatJoinRequestKey));
            if(message == null)
            {
                return new MessageType[]{ };
            }

            var data = new RequestJoinData();
            data.PlayerId = playerId;
            message.RequestJoinData = data;

            if(ParseExtraInfo != null)
            {
                ParseExtraInfo(message, dic);
            }

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
