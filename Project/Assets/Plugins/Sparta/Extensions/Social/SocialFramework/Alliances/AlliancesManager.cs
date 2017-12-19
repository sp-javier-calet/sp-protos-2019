using System;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Login;
using SocialPoint.Utils;
using SocialPoint.WAMP;
using SocialPoint.Connection;
using SocialPoint.WAMP.Caller;

namespace SocialPoint.Social
{
    public enum AllianceAction
    {
        CreateAlliance,
        ApplyToAlliance,
        JoinPublicAlliance,
        JoinPrivateAlliance,
        LeaveAlliance,
        AllianceDataEdited,
        PlayerChangedRank,
        PlayerAutoChangedRank,
        MateJoinedPlayerAlliance,
        MateLeftPlayerAlliance,
        MateKickedFromPlayerAlliance,
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
            Message = string.Empty;
            Timestamp = TimeUtils.Timestamp;
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

        public const string NotificationReceivedIdKey = "notification_id";

        public const string UserIdKey = "user_id";

        const string AllianceIdKey = "alliance_id";
        const string IdKey = "id";
        public const string AvatarKey = "avatar";
        public const string AllianceDescriptionKey = "description";
        public const string AllianceMessageKey = "welcome_message";
        public const string AllianceRequirementKey = "minimum_score";
        public const string AllianceTypeKey = "type";
        public const string AlliancePropertiesKey = "properties";
        const string AllianceNewMemberKey = "new_member_id";
        const string AllianceDeniedMemberKey = "denied_user_id";
        const string AllianceKickedMemberKey = "kicked_user_id";
        const string AlliancePromotedMemberKey = "promoted_user_id";
        public const string AllianceNewRankKey = "new_role";
        public const string NotificationTypeKey = "type";
        public const string OperationResultKey = "result";
        const string NotificationIdKey = "notification_id";
        const string JoinTimestampKey = "timestamp";
        const string JoinOriginKey = "origin";
        const string JoinMessageKey = "message";

        const string AllianceRequestIdKey = "alliance_id";
        const string AllianceRequestNameKey = "alliance_name";
        const string AllianceRequestAvatarKey = "alliance_symbol";
        const string AllianceRequestTotalMembersKey = "total_members";
        const string AllianceRequestJoinTimestampKey = "join_ts";

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
        const string AllianceRankingMethod = "alliance.ranking";
        const string AllianceSearchMethod = "alliance.search";
        const string NotificationReceivedMethod = "notification.received";

        #endregion

        const float RequestTimeout = 30.0f;

        public delegate void AllianceEventDelegate(AllianceAction action, AttrDic dic);

        public event AllianceEventDelegate AllianceEvent;

        public ILoginData LoginData { private get; set; }

        public int MaxPendingJoinRequests { get; set; }

        public IRankManager Ranks { get; set; }

        public IAccessTypeManager AccessTypes { get; set; }

        AllianceDataFactory _factory;

        public AllianceDataFactory Factory
        { 
            get
            {
                return _factory;
            }
            set
            {
                _factory = value;
                _factory.PlayerFactory = _socialManager.PlayerFactory;
            }
        }


        readonly ConnectionManager _connection;
        readonly SocialManager _socialManager;
        readonly ChatManager _chatManager;

        public AlliancesManager(ConnectionManager connection, SocialManager socialManager, ChatManager chatManager)
        {
            _socialManager = socialManager;
            _socialManager.OnLocalPlayerLoaded += OnLocalPlayerLoaded;
            _socialManager.PlayerFactory.AddFactory(new AlliancePlayerBasicFactory());
            _socialManager.PlayerFactory.AddFactory(new AlliancePlayerPrivateFactory());
            _socialManager.PlayerFactory.AddFactory(new AllianceJoinRequestComponentFactory());

            _connection = connection;
            _connection.OnNotificationReceived += OnNotificationReceived;
            _connection.OnPendingNotification += OnPendingNotificationReceived;

            _chatManager = chatManager;
        }

        public void Dispose()
        {
            _socialManager.OnLocalPlayerLoaded -= OnLocalPlayerLoaded;
            _connection.OnNotificationReceived -= OnNotificationReceived;
            _connection.OnPendingNotification -= OnPendingNotificationReceived;
        }

        public AlliancePlayerBasic GetLocalBasicData()
        {
            return _socialManager.LocalPlayer.GetComponent<AlliancePlayerBasic>();
        }

        public AlliancePlayerPrivate GetLocalPrivateData()
        {
            return _socialManager.LocalPlayer.GetComponent<AlliancePlayerPrivate>();
        }

        public AllianceBasicData GetBasicDataFromAlliance(Alliance alliance)
        {
            return Factory.CreateBasicData(alliance);
        }

        public CallRequest LoadAllianceInfo(string allianceId, Action<Error, Alliance> callback)
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
                    alliance = Factory.CreateAlliance(allianceId, result);
                }
                if(callback != null)
                {
                    callback(err, alliance);
                }
            });
        }

        public WAMPRequest LoadRanking(AttrDic extraData, Action<Error, AlliancesRanking> callback)
        {
            AttrDic data = extraData ?? new AttrDic();
            var allianceComponent = GetLocalBasicData();
            if(allianceComponent.IsInAlliance())
            {
                data.SetValue(AllianceIdKey, allianceComponent.Id);
            }

            data.SetValue(UserIdKey, LoginData.UserId.ToString());

            return _connection.Call(AllianceRankingMethod, Attr.InvalidList, data, (err, rList, rDic) => {
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
            dic.SetValue(AllianceIdKey, GetLocalBasicData().Id);

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

        void OnAllianceCreated(Alliance data, AttrDic result)
        {
            DebugUtils.Assert(result.Get(AllianceIdKey).IsValue);
            string id = string.Empty;

            if(result.ContainsKey(AllianceIdKey))
            {
                id = result.GetValue(AllianceIdKey).ToString(); 
            }
            else
            {
                id = result.GetValue(IdKey).ToString();
            }

            var basicComponent = GetLocalBasicData();
            basicComponent.Id = id;
            basicComponent.Name = data.Name;
            basicComponent.Avatar = data.Avatar;
            basicComponent.Rank = Ranks.FounderRank;

            var privateComponent = GetLocalPrivateData();
            privateComponent.TotalMembers = 1;
            privateComponent.JoinTimestamp = TimeUtils.Timestamp;
            privateComponent.ClearRequests();
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
                OnAllianceCreated(data, result);

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

                GetLocalBasicData().Name = data.Name;
                GetLocalBasicData().Avatar = data.Avatar;

                Factory.UpdateAllianceData(current, data);

                if(callback != null)
                {
                    callback(null);
                }

                NotifyAllianceEvent(AllianceAction.AllianceDataEdited, rDic);
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
            dic.SetValue(AllianceIdKey, GetLocalBasicData().Id);

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
                NotifyAllianceEvent(AllianceAction.MateKickedFromPlayerAlliance, rDic);
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

        public void OnLocalPlayerLoaded(AttrDic dic)
        {
            GetLocalPrivateData().MaxRequests = MaxPendingJoinRequests;
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

        void OnAllianceJoined(AllianceBasicData data, JoinExtraData extra)
        {
            var basicComponent = GetLocalBasicData();
            basicComponent.Id = data.Id;
            basicComponent.Name = data.Name;
            basicComponent.Avatar = data.Avatar;
            basicComponent.Rank = Ranks.DefaultRank;

            var privateComponent = GetLocalPrivateData();
            privateComponent.TotalMembers = data.Members;
            privateComponent.JoinTimestamp = extra.Timestamp;
            privateComponent.ClearRequests();
        }

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

                OnAllianceJoined(alliance, data);
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

                GetLocalPrivateData().AddRequest(alliance.Id);
                if(callback != null)
                {
                    callback(null);
                }
                NotifyAllianceEvent(AllianceAction.ApplyToAlliance, rDic);
            });
        }

        void ClearPlayerAllianceInfo()
        {
            GetLocalBasicData().ClearInfo();
            GetLocalPrivateData().ClearInfo();
            NotifyAllianceEvent(AllianceAction.OnPlayerAllianceInfoCleared, Attr.InvalidDic);
        }

        void OnPendingNotificationReceived(int type, string topic, AttrDic dic)
        {
            switch(type)
            {
            case NotificationType.NotificationAlliancePlayerAutoPromote:
                {
                    var notificationId = dic.GetValue(NotificationReceivedIdKey).ToString();
                    OnPlayerAutoChangedRank(dic);
                    SendNotificationAck(type, notificationId);
                    break;
                }
            case NotificationType.NotificationAlliancePlayerAutoDemote:
                {
                    var notificationId = dic.GetValue(NotificationReceivedIdKey).ToString();
                    OnPlayerAutoChangedRank(dic);
                    SendNotificationAck(type, notificationId);
                    break;
                }
            case NotificationType.BroadcastAllianceMemberPromote:
            case NotificationType.BroadcastAllianceMemberRankChange:
                {
                    OnMemberPromoted(dic);
                    break;
                }
            case NotificationType.NotificationAllianceMemberAccept:
            case NotificationType.NotificationAllianceMemberKickoff:
            case NotificationType.NotificationAllianceMemberPromote:
            case NotificationType.NotificationAllianceJoinRequest:
            case NotificationType.BroadcastAllianceMemberAccept:
            case NotificationType.BroadcastAllianceJoin:
            case NotificationType.BroadcastAllianceMemberKickoff:
            case NotificationType.BroadcastAllianceMemberLeave:
            case NotificationType.BroadcastAllianceEdit:
            case NotificationType.TextMessage:
            case NotificationType.NotificationUserChatBan:
            case NotificationType.BroadcastAllianceOnlineMember:
                {
                    break;
                }
            }
        }

        void OnNotificationReceived(int type, string topic, AttrDic dic)
        {
            switch(type)
            {
            case NotificationType.NotificationAllianceMemberAccept:
                {
                    OnRequestAccepted(dic);
                    break;
                }
            case NotificationType.NotificationAllianceMemberKickoff:
                {
                    OnKicked(dic);
                    break;
                }
            case NotificationType.NotificationAllianceMemberPromote:
                {
                    OnPromoted(dic);
                    break;
                }
            case NotificationType.NotificationAlliancePlayerAutoPromote:
                {
                    var notificationId = dic.GetValue(NotificationReceivedIdKey).ToString();
                    OnPlayerAutoChangedRank(dic);
                    SendNotificationAck(type, notificationId);
                    break;
                }
            case NotificationType.NotificationAlliancePlayerAutoDemote:
                {
                    var notificationId = dic.GetValue(NotificationReceivedIdKey).ToString();
                    OnPlayerAutoChangedRank(dic);
                    SendNotificationAck(type, notificationId);
                    break;
                }
            case NotificationType.NotificationAllianceJoinRequest:
                {
                    OnUserAppliedToPlayerAlliance(dic);
                    break;
                }
            case NotificationType.BroadcastAllianceMemberAccept:
            case NotificationType.BroadcastAllianceJoin:
                {
                    OnMemberJoined(dic);
                    break;
                }
            case NotificationType.BroadcastAllianceMemberKickoff:
                {
                    OnMemberKicked(dic);
                    break;
                }
            case NotificationType.BroadcastAllianceMemberLeave:
                {
                    OnMemberLeft(dic);
                    break;
                }
            case NotificationType.BroadcastAllianceEdit:
                {
                    OnAllianceEdited(dic);
                    break;
                }
            case NotificationType.BroadcastAllianceMemberPromote:
            case NotificationType.BroadcastAllianceMemberRankChange:
                {
                    OnMemberPromoted(dic);
                    break;
                }
            case NotificationType.TextMessage:
                {
                    break;
                }
            }
        }

        void OnRequestAccepted(AttrDic dic)
        {
            DebugUtils.Assert(dic.GetValue(AllianceRequestIdKey).IsValue);
            var allianceId = dic.GetValue(AllianceRequestIdKey).ToString();

            DebugUtils.Assert(dic.GetValue(AllianceRequestNameKey).IsValue);
            var allianceName = dic.GetValue(AllianceRequestNameKey).ToString();

            DebugUtils.Assert(dic.GetValue(AllianceRequestAvatarKey).IsValue);
            var avatarId = dic.GetValue(AllianceRequestAvatarKey).ToInt();

            DebugUtils.Assert(dic.GetValue(AllianceRequestTotalMembersKey).IsValue);
            var totalMembers = dic.GetValue(AllianceRequestTotalMembersKey).ToInt();

            DebugUtils.Assert(dic.GetValue(AllianceRequestJoinTimestampKey).IsValue);
            var joinTs = dic.GetValue(AllianceRequestJoinTimestampKey).ToInt();

            var basicComponent = GetLocalBasicData();
            basicComponent.Id = allianceId;
            basicComponent.Name = allianceName;
            basicComponent.Avatar = avatarId;
            basicComponent.Rank = Ranks.DefaultRank;

            var privateComponent = GetLocalPrivateData();
            privateComponent.TotalMembers = totalMembers;
            privateComponent.JoinTimestamp = joinTs;
            privateComponent.ClearRequests();

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
            DebugUtils.Assert(GetLocalBasicData().IsInAlliance, "Trying to promote a user which is not in an alliance");
            DebugUtils.Assert(dic.Get(AllianceNewRankKey).IsValue);
            var newRank = dic.GetValue(AllianceNewRankKey).ToInt();
            GetLocalBasicData().Rank = newRank;
            NotifyAllianceEvent(AllianceAction.PlayerChangedRank, dic);
        }

        void OnPlayerAutoChangedRank(AttrDic dic)
        {
            DebugUtils.Assert(GetLocalBasicData().IsInAlliance, "User is not in an alliance");
            var newRank = dic.GetValue(AllianceNewRankKey).ToInt();
            GetLocalBasicData().Rank = newRank;
            NotifyAllianceEvent(AllianceAction.PlayerAutoChangedRank, dic);
        }

        void OnUserAppliedToPlayerAlliance(AttrDic dic)
        {
            DebugUtils.Assert(GetLocalBasicData().IsInAlliance, "User is not in an alliance");
            DebugUtils.Assert(Ranks.HasPermission(GetLocalBasicData().Rank, RankPermission.ManageCandidates));
            NotifyAllianceEvent(AllianceAction.UserAppliedToPlayerAlliance, dic);
        }

        void OnMemberJoined(AttrDic dic)
        {
            GetLocalPrivateData().IncreaseTotalMembers();
            NotifyAllianceEvent(AllianceAction.MateJoinedPlayerAlliance, dic);
        }

        void OnMemberKicked(AttrDic dic)
        {
            GetLocalPrivateData().DecreaseTotalMembers();
            NotifyAllianceEvent(AllianceAction.MateKickedFromPlayerAlliance, dic);
        }

        void OnMemberLeft(AttrDic dic)
        {
            GetLocalPrivateData().DecreaseTotalMembers();
            NotifyAllianceEvent(AllianceAction.MateLeftPlayerAlliance, dic);
        }

        void OnAllianceEdited(AttrDic dic)
        {
            DebugUtils.Assert(GetLocalBasicData().IsInAlliance, "User is not in an alliance");
            DebugUtils.Assert(dic.Get(AlliancePropertiesKey).IsDic);
            var changesDic = dic.Get(AlliancePropertiesKey).AsDic;

            if(changesDic.ContainsKey(AvatarKey))
            {
                var newAvatar = changesDic.GetValue(AvatarKey).ToInt();
                GetLocalBasicData().Avatar = newAvatar;
            }

            NotifyAllianceEvent(AllianceAction.AllianceDataEdited, dic);
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
            if(_chatManager != null)
            {
                _chatManager.DeleteSubscription(_chatManager.AllianceRoom);
            }
        }

        void UpdateChatServices(AttrDic dic)
        {
            if(_chatManager != null)
            {
                DebugUtils.Assert(dic.Get(ConnectionManager.ServicesKey).IsDic);
                var servicesDic = dic.Get(ConnectionManager.ServicesKey).AsDic;

                _chatManager.ProcessChatServices(servicesDic);
            }
        }

        #endregion
    }
}
