using System;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Login;
using SocialPoint.Utils;
using SocialPoint.WAMP;

namespace SocialPoint.Social
{
    public enum AllianceAction
    {
        CreateAlliance,
        ApplyToAlliance,
        JoinPublicAlliance,
        JoinPrivateAlliance,
        LeaveAlliance,
        AllianceDescriptionEdited,
        AllianceAvatarEdited,
        AllianceTypeEdited,
        AllianceRequirementEdited,
        PlayerChangedRank,
        PlayerAutoChangedRank,
        MateJoinedPlayerAlliance,
        MateLeftPlayerAlliance,
        MateChangedRank,
        UserAppliedToPlayerAlliance,
        KickedFromAlliance,
        OnPlayerAllianceInfoParsed,
        OnPlayerAllianceInfoCleared
    }

    public class JoinExtraData
    {
        public string Origin;
        public string Message;
        public long Timestamp;

        public JoinExtraData()
        {
        }

        public JoinExtraData(string origin)
        {
            Origin = origin;
            Message = string.Empty;
            Timestamp = TimeUtils.Timestamp;
        }
    }

    public class AlliancesManager : IDisposable
    {
        #region Attr keys

        const string UserIdKey = "user_id";
        const string MemberIdKey = "player_id";
        const string AllianceIdKey = "alliance_id";
        const string AvatarKey = "avatar";
        const string AllianceDescriptionKey = "description";
        const string AllianceRequirementKey = "minimum_score";
        const string AllianceTypeKey = "type";
        const string AlliancePropertiesKey = "properties";
        const string AllianceNewMemberKey = "new_member_id";
        const string AllianceDeniedMemberKey = "denied_user_id";
        const string AllianceKickedMemberKey = "kicked_user_id";
        const string AlliancePromotedMemberKey = "promoted_user_id";
        const string AllianceNewRankKey = "new_role";
        const string NotificationTypeKey = "type";
        const string OperationResultKey = "result";
        const string NotificationIdKey = "notification_id";
        const string JoinTimestampKey = "timestamp";
        const string JoinOriginKey = "origin";
        const string JoinMessageKey = "message";

        #endregion

        #region RPC methods

        const string AllianceCreateMethod = "alliance.create";
        const string AllianceEditMethod = "alliance.edit";
        const string AllianceJoinMethod = "alliance.join";
        const string AllianceLeaveMethod = "alliance.member.leave";
        const string AllianceRequestJoinMethod = "alliance.request.join";
        const string AllianceMemberAcceptMethod = "alliance.member.accept";
        const string AllianceMemberDeclineMethod = "alliance.member.decline";
        const string AllianceMemberKickMethod = "alliance.member.kickoff";
        const string AllianceMemberPromoteMethod = "alliance.member.promote";
        const string AllianceInfoMethod = "alliance.info";
        const string AllianceMemberInfoMethod = "alliance.member.info";
        const string AllianceRankingMethod = "alliance.ranking";
        const string AllianceSearchMethod = "alliance.search";
        const string NotificationReceivedMethod = "notification.received";

        #endregion

        const float RequestTimeout = 30.0f;

        public delegate void AllianceEventDelegate(AllianceAction action, AttrDic dic);

        public event AllianceEventDelegate AllianceEvent;

        public AlliancePlayerInfo AlliancePlayerInfo { get; private set; }

        public ILoginData LoginData { private get; set; }

        public AllianceDataFactory Factory { private get; set; }

        public uint MaxPendingJoinRequests { get; set; }

        public IRankManager Ranks { get; set; }

        public IAccessTypeManager AccessTypes { get; set; }

        readonly ConnectionManager _connection;

        public AlliancesManager(ConnectionManager connection)
        {
            _connection = connection;
            _connection.AlliancesManager = this;
            _connection.OnNotificationReceived += OnNotificationReceived;
            _connection.OnPendingNotification += OnPendingNotificationReceived;
        }

        public void Dispose()
        {
            _connection.OnNotificationReceived -= OnNotificationReceived;
            _connection.OnPendingNotification -= OnPendingNotificationReceived;
        }

        public AllianceBasicData GetBasicDataFromAlliance(Alliance alliance)
        {
            return Factory.CreateBasicData(alliance);
        }

        public WAMPRequest LoadAllianceInfo(string allianceId, Action<Error, Alliance> callback)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(AllianceIdKey, allianceId);
             
            return _connection.Call(AllianceInfoMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
                Alliance alliance = null;
                if(Error.IsNullOrEmpty(err))
                {
                    DebugUtils.Assert(rDic.Get(OperationResultKey).IsDic);
                    var result = rDic.Get(OperationResultKey).AsDic;
                    alliance = Factory.CreateAlliance(allianceId, AccessTypes.DefaultAccessType, result);
                }
                if(callback != null)
                {
                    callback(err, alliance);
                }
            });
        }

        public WAMPRequest LoadUserInfo(string userId, Action<Error, AllianceMember> callback)
        {
            var dic = new AttrDic();
            dic.SetValue(MemberIdKey, userId);

            return _connection.Call(AllianceMemberInfoMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
                AllianceMember member = null;
                if(Error.IsNullOrEmpty(err))
                {
                    DebugUtils.Assert(rDic.Get(OperationResultKey).IsDic);
                    var result = rDic.Get(OperationResultKey).AsDic;
                    member = Factory.CreateMember(result);
                }
                if(callback != null)
                {
                    callback(err, member);
                }
            });
        }

        public WAMPRequest LoadRanking(Action<Error, AlliancesRanking> callback)
        {
            var dic = new AttrDic();
            if(AlliancePlayerInfo.IsInAlliance)
            {
                dic.SetValue(AllianceIdKey, AlliancePlayerInfo.Id);
            }

            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue("ranking_type", ""); // TODO in use?

            return _connection.Call(AllianceRankingMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
                AlliancesRanking ranking = null;
                if(Error.IsNullOrEmpty(err))
                {
                    DebugUtils.Assert(rDic.Get(OperationResultKey).IsDic);
                    var result = rDic.Get(OperationResultKey).AsDic;
                    ranking = Factory.CreateRankingData(result);
                }
                if(callback != null)
                {
                    callback(err, ranking);
                }
            });
        }

        public WAMPRequest LoadSearch(AlliancesSearch data, Action<Error, AlliancesSearchResult> callback)
        {
            var dic = Factory.SerializeSearchData(data);
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());

            return _connection.Call(AllianceSearchMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
                AlliancesSearchResult searchData = null;
                if(Error.IsNullOrEmpty(err))
                {
                    DebugUtils.Assert(rDic.Get(OperationResultKey).IsDic);
                    var result = rDic.Get(OperationResultKey).AsDic;
                    searchData = Factory.CreateSearchResultData(result);
                }
                if(callback != null)
                {
                    callback(err, searchData);
                }
            });
        }

        public WAMPRequest LeaveAlliance(Action<Error> callback, bool notifyEvent)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(AllianceIdKey, AlliancePlayerInfo.Id);

            return _connection.Call(AllianceLeaveMethod, Attr.InvalidList, dic, (err, EventArgs, kwargs) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    if(callback != null)
                    {
                        callback(err);
                    }
                    return;
                }

                ClearPlayerAllianceInfo();
                LeaveAllianceChat();

                if(callback != null)
                {
                    callback(null);
                }

                if(notifyEvent)
                {
                    NotifyAllianceEvent(AllianceAction.LeaveAlliance, kwargs);
                }
            });
        }

        public void JoinAlliance(AllianceBasicData alliance, Action<Error> callback, JoinExtraData data)
        {
            if(AccessTypes.IsPublic(alliance.AccessType))
            {
                JoinPublicAlliance(alliance, callback, data);
            }
            else if(AccessTypes.AcceptsCandidates(alliance.AccessType))
            {
                JoinPrivateAlliance(alliance, callback, data);
            }
            else
            {
                if(callback != null)
                {
                    callback(new Error("Cannot join the alliance"));
                }
            }
        }

        public WAMPRequest CreateAlliance(Alliance data, Action<Error> callback)
        {
            var dic = Factory.SerializeAlliance(data);
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());

            return _connection.Call(AllianceCreateMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    if(callback != null)
                    {
                        callback(err);
                    }
                    return;
                }

                DebugUtils.Assert(rDic.Get(OperationResultKey).IsDic);
                var result = rDic.Get(OperationResultKey).AsDic;
                Factory.OnAllianceCreated(AlliancePlayerInfo, data, result);

                UpdateChatServices(rDic);

                if(callback != null)
                {
                    callback(null);
                }

                NotifyAllianceEvent(AllianceAction.CreateAlliance, rDic);
            });
        }

        public WAMPRequest EditAlliance(Alliance current, Alliance data, Action<Error> callback)
        {
            var dic = new AttrDic();
            var dicProperties = Factory.SerializeAlliance(current, data);
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.Set(AlliancePropertiesKey, dicProperties);

            return _connection.Call(AllianceEditMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    if(callback != null)
                    {
                        callback(err);
                    }
                    return;
                }

                AlliancePlayerInfo.Avatar = data.Avatar;

                if(callback != null)
                {
                    callback(null);
                }

                if(current.Description != data.Description)
                {
                    current.Description = data.Description;
                    NotifyAllianceEvent(AllianceAction.AllianceDescriptionEdited, rDic);
                }

                if(current.Avatar != data.Avatar)
                {
                    current.Avatar = data.Avatar;
                    NotifyAllianceEvent(AllianceAction.AllianceAvatarEdited, rDic);
                }

                if(current.AccessType != data.AccessType)
                {
                    current.AccessType = data.AccessType;
                    NotifyAllianceEvent(AllianceAction.AllianceTypeEdited, rDic);
                }

                if(current.Requirement != data.Requirement)
                {
                    current.Requirement = data.Requirement;
                    NotifyAllianceEvent(AllianceAction.AllianceRequirementEdited, rDic);
                }
            });
        }

        public WAMPRequest AcceptCandidate(string candidateUid, Action<Error> callback)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(AllianceNewMemberKey, candidateUid);

            return _connection.Call(AllianceMemberAcceptMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    if(callback != null)
                    {
                        callback(err);
                    }
                    return;
                }
                if(callback != null)
                {
                    callback(null);
                }
                NotifyAllianceEvent(AllianceAction.MateChangedRank, rDic);
            });
        }

        public WAMPRequest DeclineCandidate(string candidateUid, Action<Error> callback)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(AllianceDeniedMemberKey, candidateUid);

            return _connection.Call(AllianceMemberDeclineMethod, Attr.InvalidList, dic, (err, rList, rDic) => callback(err));
        }

        public WAMPRequest KickMember(string memberUid, Action<Error> callback)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(AllianceKickedMemberKey, memberUid);
            dic.SetValue(AllianceIdKey, AlliancePlayerInfo.Id);

            return _connection.Call(AllianceMemberKickMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    if(callback != null)
                    {    
                        callback(err);
                    }
                    return;
                }

                if(callback != null)
                {
                    callback(null);
                }
                NotifyAllianceEvent(AllianceAction.KickedFromAlliance, rDic);
            });
        }

        public WAMPRequest PromoteMember(string memberUid, int rank, Action<Error> callback)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(AlliancePromotedMemberKey, memberUid);
            dic.SetValue(AllianceNewRankKey, rank);

            return _connection.Call(AllianceMemberPromoteMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    if(callback != null)
                    {
                        callback(err);
                    }
                    return;
                }

                if(callback != null)
                {
                    callback(null);
                }
                NotifyAllianceEvent(AllianceAction.MateChangedRank, rDic);
            });
        }

        public void ParseAllianceInfo(AttrDic dic)
        {
            DebugUtils.Assert(Factory != null, "AlliancesDataFactory is require to create an AlliancePlayerInfo");

            AlliancePlayerInfo = Factory.CreatePlayerInfo(MaxPendingJoinRequests, dic);
            NotifyAllianceEvent(AllianceAction.OnPlayerAllianceInfoParsed, dic);
        }

        public WAMPRequest SendNotificationAck(int typeCode, string notificationId)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(NotificationTypeKey, typeCode);
            dic.SetValue(NotificationIdKey, notificationId);

            return _connection.Call(NotificationReceivedMethod, Attr.InvalidList, dic, null);
        }

        #region Private methods

        WAMPRequest JoinPublicAlliance(AllianceBasicData alliance, Action<Error> callback, JoinExtraData data)
        {
            DebugUtils.Assert(AccessTypes.IsPublic(alliance.AccessType));

            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(AllianceIdKey, alliance.Id);
            dic.SetValue(JoinTimestampKey, data.Timestamp);
            dic.SetValue(JoinOriginKey, data.Origin);
            dic.SetValue(JoinMessageKey, data.Message);

            return _connection.Call(AllianceJoinMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    if(callback != null)
                    {
                        callback(err);
                    }
                    return;
                }

                Factory.OnAllianceJoined(AlliancePlayerInfo, alliance, data);
                UpdateChatServices(rDic);

                if(callback != null)
                {
                    callback(null);
                }
                NotifyAllianceEvent(AllianceAction.JoinPublicAlliance, rDic);
            });
        }

        void JoinPrivateAlliance(AllianceBasicData alliance, Action<Error> callback, JoinExtraData data)
        {
            DebugUtils.Assert(AccessTypes.AcceptsCandidates(alliance.AccessType));

            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(AllianceIdKey, alliance.Id);
            dic.SetValue(JoinTimestampKey, data.Timestamp);
            dic.SetValue(JoinOriginKey, data.Origin);
            dic.SetValue(JoinMessageKey, data.Message);

            _connection.Call(AllianceRequestJoinMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    if(callback != null)
                    {
                        callback(err);
                    }
                    return;
                }

                AlliancePlayerInfo.AddRequest(alliance.Id, MaxPendingJoinRequests);
                if(callback != null)
                {
                    callback(null);
                }
                NotifyAllianceEvent(AllianceAction.ApplyToAlliance, rDic);
            });
        }

        void ClearPlayerAllianceInfo()
        {
            AlliancePlayerInfo.ClearInfo();
            NotifyAllianceEvent(AllianceAction.OnPlayerAllianceInfoCleared, Attr.InvalidDic);
        }

        void OnPendingNotificationReceived(int type, string topic, AttrDic dic)
        {
            switch(type)
            {
            case NotificationTypeCode.NotificationAlliancePlayerAutoPromote:
                {
                    var notificationId = dic.GetValue(ConnectionManager.NotificationIdKey).ToString();
                    OnPlayerAutoChangedRank(dic);
                    SendNotificationAck(type, notificationId);
                    break;
                }
            case NotificationTypeCode.NotificationAlliancePlayerAutoDemote:
                {
                    var notificationId = dic.GetValue(ConnectionManager.NotificationIdKey).ToString();
                    OnPlayerAutoChangedRank(dic);
                    SendNotificationAck(type, notificationId);
                    break;
                }
            case NotificationTypeCode.BroadcastAllianceMemberPromote:
            case NotificationTypeCode.BroadcastAllianceMemberRankChange:
                {
                    OnMemberPromoted(dic);
                    break;
                }
            case NotificationTypeCode.NotificationAllianceMemberAccept:
            case NotificationTypeCode.NotificationAllianceMemberKickoff:
            case NotificationTypeCode.NotificationAllianceMemberPromote:
            case NotificationTypeCode.NotificationAllianceJoinRequest:
            case NotificationTypeCode.BroadcastAllianceMemberAccept:
            case NotificationTypeCode.BroadcastAllianceJoin:
            case NotificationTypeCode.BroadcastAllianceMemberKickoff:
            case NotificationTypeCode.BroadcastAllianceMemberLeave:
            case NotificationTypeCode.BroadcastAllianceEdit:
            case NotificationTypeCode.TextMessage:
            case NotificationTypeCode.NotificationUserChatBan:
            case NotificationTypeCode.BroadcastAllianceOnlineMember:
                {
                    break;
                }
            }
        }

        void OnNotificationReceived(int type, string topic, AttrDic dic)
        {
            switch(type)
            {
            case NotificationTypeCode.NotificationAllianceMemberAccept:
                {
                    OnRequestAccepted(dic);
                    break;
                }
            case NotificationTypeCode.NotificationAllianceMemberKickoff:
                {
                    OnKicked(dic);
                    break;
                }
            case NotificationTypeCode.NotificationAllianceMemberPromote:
                {
                    OnPromoted(dic);
                    break;
                }
            case NotificationTypeCode.NotificationAlliancePlayerAutoPromote:
                {
                    var notificationId = dic.GetValue(ConnectionManager.NotificationIdKey).ToString();
                    OnPlayerAutoChangedRank(dic);
                    SendNotificationAck(type, notificationId);
                    break;
                }
            case NotificationTypeCode.NotificationAlliancePlayerAutoDemote:
                {
                    var notificationId = dic.GetValue(ConnectionManager.NotificationIdKey).ToString();
                    OnPlayerAutoChangedRank(dic);
                    SendNotificationAck(type, notificationId);
                    break;
                }
            case NotificationTypeCode.NotificationAllianceJoinRequest:
                {
                    OnUserAppliedToPlayerAlliance(dic);
                    break;
                }
            case NotificationTypeCode.BroadcastAllianceMemberAccept:
            case NotificationTypeCode.BroadcastAllianceJoin:
                {
                    OnMemberJoined(dic);
                    break;
                }
            case NotificationTypeCode.BroadcastAllianceMemberKickoff:
            case NotificationTypeCode.BroadcastAllianceMemberLeave:
                {
                    OnMemberLeft(dic);
                    break;
                }
            case NotificationTypeCode.BroadcastAllianceEdit:
                {
                    OnAllianceEdited(dic);
                    break;
                }
            case NotificationTypeCode.BroadcastAllianceMemberPromote:
            case NotificationTypeCode.BroadcastAllianceMemberRankChange:
                {
                    OnMemberPromoted(dic);
                    break;
                }
            case NotificationTypeCode.TextMessage:
                {
                    break;
                }
            }
        }

        void OnRequestAccepted(AttrDic dic)
        {
            Factory.OnAllianceRequestAccepted(AlliancePlayerInfo, dic);

            UpdateChatServices(dic);

            NotifyAllianceEvent(AllianceAction.JoinPrivateAlliance, dic);
        }

        void OnKicked(AttrDic dic)
        {
            ClearPlayerAllianceInfo();
            LeaveAllianceChat();
            NotifyAllianceEvent(AllianceAction.KickedFromAlliance, dic);
        }

        void OnPromoted(AttrDic dic)
        {
            DebugUtils.Assert(AlliancePlayerInfo.IsInAlliance, "Trying to promote a user which is not in an alliance");
            DebugUtils.Assert(dic.Get(AllianceNewRankKey).IsValue);
            var newRank = dic.GetValue(AllianceNewRankKey).ToInt();
            AlliancePlayerInfo.Rank = newRank;
            NotifyAllianceEvent(AllianceAction.PlayerChangedRank, dic);
        }

        void OnPlayerAutoChangedRank(AttrDic dic)
        {
            DebugUtils.Assert(AlliancePlayerInfo.IsInAlliance, "User is not in an alliance");
            var newRank = dic.GetValue(AllianceNewRankKey).ToInt();
            AlliancePlayerInfo.Rank = newRank;
            NotifyAllianceEvent(AllianceAction.PlayerChangedRank, dic);
        }

        void OnUserAppliedToPlayerAlliance(AttrDic dic)
        {
            DebugUtils.Assert(AlliancePlayerInfo.IsInAlliance, "User is not in an alliance");
            DebugUtils.Assert(Ranks.HasPermission(AlliancePlayerInfo.Rank, RankPermission.Members));
            NotifyAllianceEvent(AllianceAction.UserAppliedToPlayerAlliance, dic);
        }

        void OnMemberJoined(AttrDic dic)
        {
            AlliancePlayerInfo.IncreaseTotalMembers();
            NotifyAllianceEvent(AllianceAction.MateJoinedPlayerAlliance, dic);
        }

        void OnMemberLeft(AttrDic dic)
        {
            AlliancePlayerInfo.DecreaseTotalMembers();
            NotifyAllianceEvent(AllianceAction.MateJoinedPlayerAlliance, dic);
        }

        void OnAllianceEdited(AttrDic dic)
        {
            DebugUtils.Assert(AlliancePlayerInfo.IsInAlliance, "User is not in an alliance");
            DebugUtils.Assert(dic.Get(AlliancePropertiesKey).IsDic);
            var changesDic = dic.Get(AlliancePropertiesKey).AsDic;

            if(changesDic.ContainsKey(AllianceDescriptionKey))
            {
                NotifyAllianceEvent(AllianceAction.AllianceDescriptionEdited, dic);
            }

            if(changesDic.ContainsKey(AvatarKey))
            {
                var newAvatar = changesDic.GetValue(AvatarKey).ToInt();
                AlliancePlayerInfo.Avatar = newAvatar;
                NotifyAllianceEvent(AllianceAction.AllianceAvatarEdited, dic);
            }

            if(changesDic.ContainsKey(AllianceTypeKey))
            {
                NotifyAllianceEvent(AllianceAction.AllianceTypeEdited, dic);
            }

            if(changesDic.ContainsKey(AllianceRequirementKey))
            {
                NotifyAllianceEvent(AllianceAction.AllianceRequirementEdited, dic);
            }
        }

        void OnMemberPromoted(AttrDic dic)
        {
            NotifyAllianceEvent(AllianceAction.MateChangedRank, dic);
        }

        void NotifyAllianceEvent(AllianceAction action, AttrDic dic)
        {
            if(AllianceEvent != null)
            {
                AllianceEvent(action, dic);
            }
        }

        void LeaveAllianceChat()
        {
            var chatManager = _connection.ChatManager;
            if(chatManager != null)
            {
                chatManager.DeleteSubscription(chatManager.AllianceRoom);
            }
        }

        void UpdateChatServices(AttrDic dic)
        {
            var chatManager = _connection.ChatManager;
            if(chatManager != null)
            {
                DebugUtils.Assert(dic.Get(ConnectionManager.ServicesKey).IsDic);
                var servicesDic = dic.Get(ConnectionManager.ServicesKey).AsDic;

                DebugUtils.Assert(servicesDic.Get(ConnectionManager.ChatServiceKey).IsDic);
                chatManager.ProcessChatServices(servicesDic.Get(ConnectionManager.ChatServiceKey).AsDic);
            }
        }

        #endregion
    }
}
