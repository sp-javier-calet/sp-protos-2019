using System;
using System.Runtime.Serialization;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Network;
using SocialPoint.Login;
using SocialPoint.Utils;

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
        public long Timestamp;

        public JoinExtraData()
        {
        }

        public JoinExtraData(string origin)
        {
            Origin = origin;
            Timestamp = TimeUtils.Timestamp;
        }
    }

    public class AlliancesManager : IDisposable
    {
        #region Attr keys

        const string UserIdKey = "user_id";
        // TODO Both Key and Session store the SessionId
        const string UserSessionKey = "user_key";
        const string SessionIdKey = "session_id";
        const string NameKey = "name";
        const string AllianceIdKey = "alliance_id";
        const string AvatarKey = "avatar";
        const string AllianceNameKey = "alliance_name";
        const string AllianceDescriptionKey = "description";
        const string AllianceRequirementKey = "minimum_score";
        const string AllianceTypeKey = "type";
        const string AllianceAvatarKey = "alliance_symbol";
        const string AlliancePropertiesKey = "properties";
        const string AllianceNewMemberKey = "new_member_id";
        const string AllianceDeniedMemberKey = "denied_member_id";
        const string AllianceKickedMemberKey = "kicked_user_id";
        const string AlliancePromotedMemberKey = "promoted_user_id";
        const string AllianceNewRankKey = "new_role";
        const string AllianceTotalMembersKey = "total_members";
        const string AllianceJoinTimestampKey = "join_ts";
        const string NotificationTypeKey = "type";
        const string SearchFilterKey = "filter_name";
        const string OperationResultKey = "result";
        const string NotificationIdKey = "notification_id";
        const string TimestampKey = "timestamp";
        const string OriginKey = "origin";

        #endregion

        #region Alliance endpoints

        const string AllianceEndpoint = "/api/alliance/";
        const string AllianceMemberEndpoint = "/api/alliance/member";
        const string AllianceRankingEndpoint = "/api/alliance/ranking";
        const string AllianceSearchEndpoint = "/api/alliance/search";
        const string AllianceSuggestedEndpoint = "/api/alliance/suggested";
        const string AllianceJoinSuggestedEndpoint = "/api/alliance/suggested_reward";

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
        const string NotificationReceivedMethod = "notification.received";

        #endregion

        const float RequestTimeout = 30.0f;

        public delegate void AllianceEventDelegate(AllianceAction action, AttrDic dic);

        public event AllianceEventDelegate AllianceEvent;

        public AlliancePlayerInfo AlliancePlayerInfo { get; private set; }

        public IHttpClient HttpClient { private get; set; }

        public ILoginData LoginData { private get; set; }

        public AllianceDataFactory Factory { private get; set; }

        public uint MaxPendingJoinRequests { get; set; }

        public IRankManager Ranks { get; set; }

        public IAccessTypeManager AccessTypes { get; set; }

        public string AlliancesServerUrl;

        readonly JsonAttrParser _parser;

        readonly ConnectionManager _connection;

        public AlliancesManager(ConnectionManager connection)
        {
            Ranks = new DefaultRankManager();
            AccessTypes = new DefaultAccessTypeManager();
            Factory = new AllianceDataFactory();
            AlliancePlayerInfo = Factory.CreatePlayerInfo();
            _parser = new JsonAttrParser();
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

        public IHttpConnection LoadAllianceInfo(string allianceId, Action<Error, Alliance> callback)
        {
            var req = new HttpRequest(GetUrl(AllianceEndpoint + allianceId));
            req.Timeout = RequestTimeout;
            req.AddParam(UserSessionKey, LoginData.SessionId);
            req.AddParam(UserIdKey, LoginData.UserId.ToString());
             
            return HttpClient.Send(req, response => OnAllianceInfoLoaded(response, allianceId, callback));
        }

        void OnAllianceInfoLoaded(HttpResponse resp, string allianceId, Action<Error, Alliance> callback)
        {
            AttrDic dic;
            var error = ParseResponse(resp, out dic);
            Alliance alliance = null;

            if(Error.IsNullOrEmpty(error))
            {
                alliance = Factory.CreateAlliance(allianceId, AccessTypes.DefaultAccessType, dic);
            }
            if(callback != null)
            {
                callback(error, alliance);
            }
        }

        public IHttpConnection LoadUserInfo(string userId, Action<Error, AllianceMember> callback)
        {
            var req = new HttpRequest(GetUrl(AllianceMemberEndpoint + userId));
            req.Timeout = RequestTimeout;
            req.AddParam(UserSessionKey, LoginData.SessionId);

            return HttpClient.Send(req, response => OnUserInfoLoaded(response, callback));
        }

        void OnUserInfoLoaded(HttpResponse resp, Action<Error, AllianceMember> callback)
        {
            AttrDic dic;
            var error = ParseResponse(resp, out dic);
            AllianceMember member = null;

            if(Error.IsNullOrEmpty(error))
            {
                member = Factory.CreateMember(dic);
            }
            if(callback != null)
            {
                callback(error, member);
            }
        }

        public IHttpConnection LoadRanking(Action<Error, AllianceRankingData> callback)
        {
            var req = new HttpRequest(GetUrl(AllianceRankingEndpoint));
            req.Timeout = RequestTimeout;

            if(AlliancePlayerInfo.IsInAlliance)
            {
                req.AddParam(AllianceIdKey, AlliancePlayerInfo.Id);
            }

            req.AddParam(UserIdKey, LoginData.UserId.ToString());

            return HttpClient.Send(req, response => OnRankingLoaded(response, callback));
        }

        void OnRankingLoaded(HttpResponse resp, Action<Error, AllianceRankingData> callback)
        {
            AttrDic dic;
            var error = ParseResponse(resp, out dic);
            AllianceRankingData ranking = null;

            if(Error.IsNullOrEmpty(error))
            {
                ranking = Factory.CreateRankingData(dic);
            }
            if(callback != null)
            {
                callback(error, ranking);
            }
        }

        public IHttpConnection LoadSearch(string search, Action<Error, AlliancesSearchData> callback)
        {
            var req = new HttpRequest(GetUrl(AllianceSearchEndpoint));
            req.Timeout = RequestTimeout;
            req.AddParam(SearchFilterKey, search);
            req.AddParam(UserIdKey, LoginData.UserId.ToString());

            return HttpClient.Send(req, response => OnSearchLoaded(response, callback, false));
        }

        public IHttpConnection LoadSearchSuggested(Action<Error, AlliancesSearchData> callback)
        {
            var req = new HttpRequest(GetUrl(AllianceSuggestedEndpoint));
            req.Timeout = RequestTimeout;
            req.AddParam(UserIdKey, LoginData.UserId.ToString());

            return HttpClient.Send(req, response => OnSearchLoaded(response, callback, true));
        }

        void OnSearchLoaded(HttpResponse resp, Action<Error, AlliancesSearchData> callback, bool suggested)
        {
            AttrDic dic;
            var error = ParseResponse(resp, out dic);
            AlliancesSearchData search = null;

            if(Error.IsNullOrEmpty(error))
            {
                search = Factory.CreateSearchData(dic, suggested);
            }
            if(callback != null)
            {
                callback(error, search);
            }
        }

        public IHttpConnection LoadJoinSuggestedAlliances(Action<Error, AlliancesSearchData> callback)
        {
            var req = new HttpRequest(GetUrl(AllianceJoinSuggestedEndpoint));
            req.Timeout = RequestTimeout;
            req.AddParam(UserIdKey, LoginData.UserId.ToString());
            req.AddParam(SessionIdKey, LoginData.SessionId);

            return HttpClient.Send(req, response => OnJoinSuggestedAlliancesLoaded(response, callback));
        }

        void OnJoinSuggestedAlliancesLoaded(HttpResponse resp, Action<Error, AlliancesSearchData> callback)
        {
            AttrDic dic;
            var error = ParseResponse(resp, out dic);
            AlliancesSearchData search = null;

            if(Error.IsNullOrEmpty(error))
            {
                search = Factory.CreateJoinData(dic);

            }
            if(callback != null)
            {
                callback(error, search);
            }
        }

        public void LeaveAlliance(Action<Error> callback, bool notifyEvent)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(AllianceIdKey, AlliancePlayerInfo.Id);

            _connection.Call(AllianceLeaveMethod, Attr.InvalidList, dic, (err, EventArgs, kwargs) => {
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

        public void CreateAlliance(AlliancesCreateData data, Action<Error> callback)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(NameKey, data.Name);
            dic.SetValue(AllianceDescriptionKey, data.Description);
            dic.SetValue(AllianceRequirementKey, data.Requirement);
            dic.SetValue(AllianceTypeKey, data.AccessType);
            dic.SetValue(AvatarKey, data.Avatar);

            _connection.Call(AllianceCreateMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
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

                DebugUtils.Assert(rDic.Get(AllianceIdKey).IsValue);
                var id = result.GetValue(AllianceIdKey).ToString();

                AlliancePlayerInfo.Id = id;
                AlliancePlayerInfo.Avatar = data.Avatar;
                AlliancePlayerInfo.Name = data.Name;
                AlliancePlayerInfo.Rank = Ranks.FounderRank;
                AlliancePlayerInfo.TotalMembers = 1;
                AlliancePlayerInfo.JoinTimestamp = TimeUtils.Timestamp;
                AlliancePlayerInfo.ClearRequests();

                UpdateChatServices(rDic);

                if(callback != null)
                {
                    callback(null);
                }

                NotifyAllianceEvent(AllianceAction.CreateAlliance, rDic);
            });
        }

        public void EditAlliance(Alliance current, AlliancesCreateData data, Action<Error> callback)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            var dicProperties = new AttrDic();

            if(current.Description != data.Description)
            {
                dicProperties.SetValue(AllianceDescriptionKey, data.Description);
            }

            if(current.Requirement != data.Requirement)
            {
                dicProperties.SetValue(AllianceRequirementKey, data.Requirement);
            }

            if(current.AccessType != data.AccessType)
            {
                dicProperties.SetValue(AllianceTypeKey, data.AccessType);
            }

            if(current.Avatar != data.Avatar)
            {
                dicProperties.SetValue(AvatarKey, data.Avatar);
            }

            dic.Set(AlliancePropertiesKey, dicProperties);

            _connection.Call(AllianceEditMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
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
                    NotifyAllianceEvent(AllianceAction.AllianceAvatarEdited, rDic); // TODO Change name?
                }

                if(current.AccessType != data.AccessType)
                {
                    current.AccessType = data.AccessType;
                    NotifyAllianceEvent(AllianceAction.AllianceTypeEdited, rDic);
                }

                if(current.Requirement != data.Requirement) // TODO use same name in both classes
                {
                    current.Requirement = data.Requirement;
                    NotifyAllianceEvent(AllianceAction.AllianceRequirementEdited, rDic);
                }
            });
        }

        public void AcceptCandidate(string candidateUid, Action<Error> callback)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(AllianceNewMemberKey, long.Parse(candidateUid));

            _connection.Call(AllianceMemberAcceptMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
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

        public void DeclineCandidate(string candidateUid, Action<Error> callback)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(AllianceDeniedMemberKey, long.Parse(candidateUid));

            _connection.Call(AllianceMemberAcceptMethod, Attr.InvalidList, dic, (err, rList, rDic) => callback(err));
        }

        public void KickMember(string memberUid, Action<Error> callback)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(AllianceKickedMemberKey, long.Parse(memberUid));
            dic.SetValue(AllianceIdKey, AlliancePlayerInfo.Id);

            _connection.Call(AllianceMemberKickMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
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

        public void PromoteMember(string memberUid, int rank, Action<Error> callback)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(AlliancePromotedMemberKey, long.Parse(memberUid));
            dic.SetValue(AllianceNewRankKey, rank);

            _connection.Call(AllianceMemberPromoteMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    if(callback != null)
                    {
                        callback(err);
                    }
                    return;
                }

                AlliancePlayerInfo.Rank = Ranks.GetPromotedTo(rank); 

                if(callback != null)
                {
                    callback(null);
                }
                NotifyAllianceEvent(AllianceAction.MateChangedRank, rDic);
            });
        }

        public void ParseAllianceInfo(AttrDic dic)
        {
            AlliancePlayerInfo = Factory.CreatePlayerInfo(MaxPendingJoinRequests, dic);
            NotifyAllianceEvent(AllianceAction.OnPlayerAllianceInfoParsed, dic);
        }

        public void SendNotificationAck(int typeCode, string notificationId)
        {
            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(NotificationTypeKey, typeCode);
            dic.SetValue(NotificationIdKey, notificationId);

            _connection.Call(NotificationReceivedMethod, Attr.InvalidList, dic, null);
        }

        #region Private methods

        string GetUrl(string suffix)
        {
            return (string.IsNullOrEmpty(AlliancesServerUrl) ? LoginData.BaseUrl : AlliancesServerUrl) + suffix;
        }

        void JoinPublicAlliance(AllianceBasicData alliance, Action<Error> callback, JoinExtraData data)
        {
            DebugUtils.Assert(AccessTypes.IsPublic(alliance.AccessType));

            var dic = new AttrDic();
            dic.SetValue(UserIdKey, LoginData.UserId.ToString());
            dic.SetValue(AllianceIdKey, alliance.Id);
            dic.SetValue(TimestampKey, data.Timestamp);
            dic.SetValue(OriginKey, data.Origin);

            long joinTs = data.Timestamp;

            _connection.Call(AllianceJoinMethod, Attr.InvalidList, dic, (err, rList, rDic) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    if(callback != null)
                    {
                        callback(err);
                    }
                    return;
                }

                AlliancePlayerInfo.Id = alliance.Id;
                AlliancePlayerInfo.Name = alliance.Name;
                AlliancePlayerInfo.Avatar = alliance.Avatar;
                AlliancePlayerInfo.Rank = Ranks.DefaultRank;
                AlliancePlayerInfo.TotalMembers = alliance.Members;
                AlliancePlayerInfo.JoinTimestamp = joinTs;
                AlliancePlayerInfo.ClearRequests();

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
            dic.SetValue(TimestampKey, data.Timestamp);
            dic.SetValue(OriginKey, data.Origin);

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
            DebugUtils.Assert(dic.GetValue(AllianceIdKey).IsValue);
            var allianceId = dic.GetValue(AllianceIdKey).ToString();

            DebugUtils.Assert(dic.GetValue(AllianceNameKey).IsValue);
            var allianceName = dic.GetValue(AllianceNameKey).ToString();

            DebugUtils.Assert(dic.GetValue(AllianceAvatarKey).IsValue);
            var avatarId = dic.GetValue(AllianceAvatarKey).ToInt();

            var totalMembers = dic.GetValue(AllianceTotalMembersKey).ToInt();
            var joinTs = dic.GetValue(AllianceJoinTimestampKey).ToInt();

            AlliancePlayerInfo.Id = allianceId;
            AlliancePlayerInfo.Name = allianceName;
            AlliancePlayerInfo.Avatar = avatarId;
            AlliancePlayerInfo.Rank = Ranks.DefaultRank;
            AlliancePlayerInfo.TotalMembers = totalMembers;
            AlliancePlayerInfo.JoinTimestamp = joinTs;
            AlliancePlayerInfo.ClearRequests();

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
            DebugUtils.Assert(Ranks.HasMemberManagementPermission(AlliancePlayerInfo.Rank));
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

        Error ParseResponse(HttpResponse response, out AttrDic dic)
        {
            dic = null;
            var error = response.Error;
            if(Error.IsNullOrEmpty(error))
            {   
                try
                {
                    dic = _parser.Parse(response.Body).AsDic;
                }
                catch(SerializationException e)
                {
                    error = new Error("Serialization error. " + e.Message);
                }
            }

            return error;
        }

        #endregion
    }
}
