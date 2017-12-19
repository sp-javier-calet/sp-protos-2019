using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Locale;
using SocialPoint.Connection;

namespace SocialPoint.Social
{
    public class FactoryChatMessages<MessageType> 
        where MessageType : class, IChatMessage, new()
    {
        const string Tag = "SocialFramework";

        #region Attr keys

        const string UserIdKey = "user_id";
        const string UserNameKey = "user_name";
        const string KickedUserNameKey = "kicked_user_name";
        const string AcceptedUserNameKey = "accepted_user_name";
        const string AdminUserNameKey = "admin_user_name";
        const string OldRoleKey = "old_role";
        const string NewRoleKey = "new_role";
        const string RankPromotionKey = "promotion";
        const string RankDemotionKey = "demotion";

        public const string ChatMessageUuidKey = "id";
        public const string ChatMessageUserIdKey = "uid";
        public const string ChatMessageUserNameKey = "uname";
        public const string ChatMessageTsKey = "ts";
        public const string ChatMessageTextKey = "msg";
        public const string ChatMessageLevelKey = "lvl";
        public const string ChatMessageAllyNameKey = "ally_name";
        public const string ChatMessageAllyIdKey = "ally_id";
        public const string ChatMessageAllyAvatarKey = "ally_avatar";
        public const string ChatMessageAllyRoleKey = "ally_role";

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

            msgInfo.SetValue(ChatMessageUuidKey, message.Uuid);
            msgInfo.SetValue(ChatMessageUserIdKey, data.PlayerId);
            msgInfo.SetValue(ChatMessageUserNameKey, data.PlayerName);
            msgInfo.SetValue(ChatMessageTsKey, message.Timestamp);
            msgInfo.SetValue(ChatMessageTextKey, message.Text);
            msgInfo.SetValue(ChatMessageLevelKey, data.PlayerLevel);
            msgInfo.SetValue(ChatMessageAllyNameKey, data.AllianceName);
            msgInfo.SetValue(ChatMessageAllyIdKey, data.AllianceId);
            msgInfo.SetValue(ChatMessageAllyAvatarKey, data.AllianceAvatarId);
            msgInfo.SetValue(ChatMessageAllyRoleKey, data.RankInAlliance);

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
                messages = ParsePlayerAcceptedMessage(type, dic);
                break;

            case NotificationType.BroadcastAllianceJoin:
                messages = ParsePlayerJoinedMessage(type, dic);
                break;

            case NotificationType.BroadcastAllianceMemberKickoff:
                messages = ParsePlayerKickedMessage(type, dic);
                break;

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
            if(!dic.ContainsKey(ChatManager.ChatMessageInfoKey))
            {
                Log.e(Tag, "Received chat message of text type does not contain the main parent");
                return new MessageType[0];
            }

            var msgInfo = dic.Get(ChatManager.ChatMessageInfoKey).AsDic;
            if(!Validate(msgInfo,
                ChatMessageUserIdKey,
                ChatMessageUserNameKey,
                ChatMessageTsKey,
                ChatMessageTextKey,
                ChatMessageLevelKey,
                ChatMessageAllyNameKey,
                ChatMessageAllyIdKey,
                ChatMessageAllyAvatarKey,
                ChatMessageAllyRoleKey
            ))
            {
                Log.e(Tag, "Received chat message of text type does not contain all the mandatory fields");
                return new MessageType[0];
            }

            var message = Create(type, msgInfo.GetValue(ChatMessageTextKey).ToString());
            if(message == null)
            {
                return new MessageType[]{ };
            }

            var data = new MessageData();

            data.PlayerId = msgInfo.GetValue(ChatMessageUserIdKey).ToString();
            data.PlayerName = msgInfo.GetValue(ChatMessageUserNameKey).ToString();
            data.PlayerLevel = msgInfo.GetValue(ChatMessageLevelKey).ToInt();
            data.AllianceName = msgInfo.GetValue(ChatMessageAllyNameKey).ToString();
            data.AllianceId = msgInfo.GetValue(ChatMessageAllyIdKey).ToString();
            data.AllianceAvatarId = msgInfo.GetValue(ChatMessageAllyAvatarKey).ToInt();
            data.RankInAlliance = msgInfo.GetValue(ChatMessageAllyRoleKey).ToInt();
            message.Timestamp = msgInfo.GetValue(ChatMessageTsKey).ToLong();

            message.MessageData = data;

            if(ParseExtraInfo != null)
            {
                ParseExtraInfo(message, msgInfo);
            }

            return new []{ message };
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
            message.Timestamp = dic.GetValue(ChatMessageTsKey).ToLong();
            return new [] { message };
        }

        MessageType[] ParsePlayerAcceptedMessage(int type, AttrDic dic)
        {   
            if(!Validate(dic, UserNameKey, AcceptedUserNameKey, UserIdKey))
            {
                Log.e(Tag, "Received chat message of player accepted message type does not contain all the mandatory fields");
                return new MessageType[0];
            }

            var nameUserAction = dic.GetValue(UserNameKey).ToString();
            var nameUserAccepted = dic.GetValue(AcceptedUserNameKey).ToString();
            string userAcceptedId = dic.GetValue(UserIdKey).ToString();
            var message = CreateWarning(type, string.Format(Localization.Get(SocialFrameworkStrings.ChatPlayerAcceptedKey), nameUserAction, nameUserAccepted));
            if(message == null)
            {
                return new MessageType[]{ };
            }
            message.RequestJoinData = new RequestJoinData();
            message.RequestJoinData.PlayerId = userAcceptedId;
            message.Timestamp = dic.GetValue(ChatMessageTsKey).ToLong();
            return new [] { message };
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
            message.Timestamp = dic.GetValue(ChatMessageTsKey).ToLong();
            return new [] { message };
        }

        MessageType[] ParsePlayerKickedMessage(int type, AttrDic dic)
        {
            //AdminUserNameKey is not mandatory
            if(!Validate(dic, KickedUserNameKey))
            {
                Log.e(Tag, "Received chat message of player left type does not contain all the mandatory fields");
                return new MessageType[0];
            }

            var kickedPlayerName = dic.GetValue(KickedUserNameKey).ToString();
            var adminPlayerName = dic.GetValue(AdminUserNameKey).ToString();
            var message = CreateWarning(type, string.Format(Localization.Get(SocialFrameworkStrings.ChatPlayerKickedKey), kickedPlayerName, adminPlayerName));
            if(message == null)
            {
                return new MessageType[]{ };
            }
            message.Timestamp = dic.GetValue(ChatMessageTsKey).ToLong();
            return new [] { message };
        }

        MessageType[] ParseMemberPromotedMessage(int type, AttrDic dic)
        {
            if(!Validate(dic, UserNameKey, OldRoleKey, NewRoleKey))
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

            message.Timestamp = dic.GetValue(ChatMessageTsKey).ToLong();

            var data = new MemberPromotionData();
            data.PlayerName = playerName;
            data.OldRank = oldRank;
            data.NewRank = newRank;
            message.MemberPromotionData = data;

            return new [] { message };
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
            message.Timestamp = dic.GetValue(ChatMessageTsKey).ToLong();

            if(ParseExtraInfo != null)
            {
                ParseExtraInfo(message, dic);
            }

            return new [] { message };
        }

        static bool Validate(AttrDic dic, params string[] requiredValues)
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
