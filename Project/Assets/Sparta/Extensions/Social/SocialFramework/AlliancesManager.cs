using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.WAMP;
using SocialPoint.WAMP.Subscriber;
using SocialPoint.Network;
using SocialPoint.Login;
using SocialPoint.Utils;

namespace SocialPoint.Social
{
    public enum AllianceAccessType
    {
        Open,
        Private
    }

    public enum AllianceMemberType
    {
        Lead = 1,
        CoLead,
        Soldier,
        Undefined
    }

    public enum AllianceAction
    {
        CreateAlliance,
        ApplyToAlliance,
        JoinPublicAlliance,
        JoinPrivateAlliance,
        LeaveAlliance,
        AllianceDescriptionEdited,
        AllianceIconEdited,
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

    public enum AllianceRankChange
    {
        Promotion,
        Demotion
    }

    public class JoinExtraData
    {
        public string Origin;
        public long Timestamp;
    }

    public class AlliancesManager : IDisposable
    {
        const float RequestTimeout = 30.0f;

        public delegate void AllianceEventDelegate(AllianceAction action, AttrDic dic);

        public event AllianceEventDelegate AllianceEvent;

        public AllianceMember Player;

        AlliancePlayerInfo AlliancePlayerInfo;

        readonly JsonAttrParser _parser;

        readonly ConnectionManager _connection;

        public ConnectionManager Connection
        {
            get
            {
                return _connection;
            }
        }

        public IHttpClient HttpClient { private get; set; }

        public ILoginData LoginData { private get; set; }

        public AllianceDataFactory Factory { private get; set; }

        public string ServerUrl { get; set; }

        public uint MaxPendingJoinRequests { get; set; }

        public AlliancesManager(ConnectionManager connection)
        {
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

        public IHttpConnection LoadAllianceInfo(string allianceId, Action<Alliance> onSuccess, Action<Error> onFailure)
        {
            var url = LoginData.BaseUrl + "/api/alliance/" + allianceId;
            var req = new HttpRequest(url);
            req.Timeout = RequestTimeout;
            req.AddParam("user_key", LoginData.SessionId);
            req.AddParam("user_id", LoginData.UserId.ToString());
             
            return HttpClient.Send(req, response => OnAllianceInfoLoaded(response, allianceId, onSuccess, onFailure));
        }

        void OnAllianceInfoLoaded(HttpResponse resp, string allianceId, Action<Alliance> onSuccess, Action<Error> onFailure)
        {
            var error = resp.Error;
            var success = Error.IsNullOrEmpty(error);
            AttrDic dic = null;

            if(success)
            {
                try
                {
                    dic = _parser.Parse(resp.Body).AsDic;
                }
                catch(SerializationException e)
                {
                    error = new Error("Serialization error. " + e.Message);
                    success = false;
                }
            }

            if(success)
            {
                onSuccess(Factory.CreateAlliance(allianceId, dic));
            }
            else
            {
                onFailure(error);
            }
        }

        public IHttpConnection LoadUserInfo(string userId, Action<AllianceMember> onSuccess, Action<Error> onFailure)
        {
            var url = LoginData.BaseUrl + "/api/alliance/member" + userId;
            var req = new HttpRequest(url);
            req.Timeout = RequestTimeout;
            req.AddParam("user_key", LoginData.SessionId);

            return HttpClient.Send(req, response => OnUserInfoLoaded(response, onSuccess, onFailure));
        }

        void OnUserInfoLoaded(HttpResponse resp, Action<AllianceMember> onSuccess, Action<Error> onFailure)
        {
            var error = resp.Error;
            var success = Error.IsNullOrEmpty(error);
            AttrDic dic = null;

            if(success)
            {
                try
                {
                    dic = _parser.Parse(resp.Body).AsDic;
                }
                catch(SerializationException e)
                {
                    error = new Error("Serialization error. " + e.Message);
                    success = false;
                }
            }

            if(success)
            {
                onSuccess(Factory.CreateMember(dic));
            }
            else
            {
                onFailure(error);
            }
        }

        public IHttpConnection LoadRanking(Action<AllianceRankingData> onSuccess, Action<Error> onFailure)
        {
            var url = LoginData.BaseUrl + "/api/alliance/ranking";
            var req = new HttpRequest(url);
            req.Timeout = RequestTimeout;

            if(Player.IsInAlliance)
            {
                req.AddParam("alliance_id", Player.Id);
            }

            req.AddParam("user_id", LoginData.UserId.ToString());

            return HttpClient.Send(req, response => OnRankingLoaded(response, onSuccess, onFailure));
        }

        void OnRankingLoaded(HttpResponse resp, Action<AllianceRankingData> onSuccess, Action<Error> onFailure)
        {
            var error = resp.Error;
            var success = Error.IsNullOrEmpty(error);
            AttrDic dic = null;

            if(success)
            {
                try
                {
                    dic = _parser.Parse(resp.Body).AsDic;
                }
                catch(SerializationException e)
                {
                    error = new Error("Serialization error. " + e.Message);
                    success = false;
                }
            }

            if(success)
            {
                onSuccess(Factory.CreateRankingData(dic));
            }
            else
            {
                onFailure(error);
            }
        }

        public IHttpConnection LoadSearch(string search, Action<AlliancesSearchData> onSuccess, Action<Error> onFailure)
        {
            var url = LoginData.BaseUrl + "/api/alliance/search";
            var req = new HttpRequest(url);
            req.Timeout = RequestTimeout;
            req.AddParam("filter_name", search);
            req.AddParam("user_id", LoginData.UserId.ToString());

            return HttpClient.Send(req, response => OnSearchLoaded(response, onSuccess, onFailure, false));
        }

        public IHttpConnection LoadSearchSuggested(Action<AlliancesSearchData> onSuccess, Action<Error> onFailure)
        {
            var url = LoginData.BaseUrl + "/api/alliance/suggested";
            var req = new HttpRequest(url);
            req.Timeout = RequestTimeout;
            req.AddParam("user_id", LoginData.UserId.ToString());

            return HttpClient.Send(req, response => OnSearchLoaded(response, onSuccess, onFailure, true));
        }

        void OnSearchLoaded(HttpResponse resp, Action<AlliancesSearchData> onSuccess, Action<Error> onFailure, bool suggested)
        {
            var error = resp.Error;
            var success = Error.IsNullOrEmpty(error);
            AttrDic dic = null;

            if(success)
            {   
                try
                {
                    dic = _parser.Parse(resp.Body).AsDic;
                }
                catch(SerializationException e)
                {
                    error = new Error("Serialization error. " + e.Message);
                    success = false;
                }
            }

            if(success)
            {
                onSuccess(Factory.CreateSearchData(dic, suggested));
            }
            else
            {
                onFailure(error);
            }
        }

        public IHttpConnection LoadJoinSuggestedAlliances(Action<AlliancesSearchData> onSuccess, Action<Error> onFailure)
        {
            var url = LoginData.BaseUrl + "/api/alliance/suggested_reward";
            var req = new HttpRequest(url);
            req.Timeout = RequestTimeout;
            req.AddParam("user_id", LoginData.UserId.ToString());
            req.AddParam("session_id", LoginData.SessionId);

            return HttpClient.Send(req, response => OnJoinSuggestedAlliancesLoaded(response, onSuccess, onFailure));
        }

        void OnJoinSuggestedAlliancesLoaded(HttpResponse resp, Action<AlliancesSearchData> onSuccess, Action<Error> onFailure)
        {
            var error = resp.Error;
            var success = Error.IsNullOrEmpty(error);
            AttrDic dic = null;

            if(success)
            {   
                try
                {
                    dic = _parser.Parse(resp.Body).AsDic;
                }
                catch(SerializationException e)
                {
                    error = new Error("Serialization error. " + e.Message);
                    success = false;
                }
            }

            if(success)
            {
                onSuccess(Factory.CreateJoinData(dic));
            }
            else
            {
                onFailure(error);
            }
        }

        public void LeaveAlliance(Action<Error> callback, bool notifyEvent)
        {
            var dic = new AttrDic();
            dic.SetValue("user_id", LoginData.UserId.ToString());
            dic.SetValue("alliance_id", Player.Id);

            Connection.Call("alliance.member.leave", Attr.InvalidList, dic, (err, EventArgs, kwargs) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    callback(err);
                    return;
                }

                ClearPlayerAllianceInfo();

                var chatManager = Connection.ChatManager;
                if(chatManager != null)
                {
                    chatManager.DeleteSubscription(chatManager.AllianceRoom);
                }

                callback(null);

                if(notifyEvent && AllianceEvent != null)
                {
                    AllianceEvent(AllianceAction.LeaveAlliance, kwargs);
                }
            });
        }

        public void JoinAlliance(AllianceBasicData alliance, Action<Error> callback, JoinExtraData data)
        {
            switch(alliance.AccessType)
            {
            case AllianceAccessType.Open:
                JoinPublicAlliance(alliance, callback, data);
                break;
            case AllianceAccessType.Private:
                JoinPrivateAlliance(alliance, callback, data);
                break;
            }
        }

        public void CreateAlliance(AlliancesCreateData data, Action<Error> callback)
        {
            var dic = new AttrDic();
            dic.SetValue("user_id", LoginData.UserId.ToString());
            dic.SetValue("name", data.Name);
            dic.SetValue("description", data.Description);
            dic.SetValue("minimum_score", data.RequirementValue);
            dic.SetValue("type", data.AccessType != AllianceAccessType.Open ? 1 : 0);
            dic.SetValue("avatar", data.AvatarId);

            Connection.Call("alliance.create", Attr.InvalidList, dic, (err, rList, rDic) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    callback(err);
                    return;
                }

                var result = rDic.Get("result").AsDic;
                var id = result.GetValue("alliance_id").ToString();
                AlliancePlayerInfo.Id = id;
                AlliancePlayerInfo.AvatarId = data.AvatarId;
                AlliancePlayerInfo.Name = data.Name;
                AlliancePlayerInfo.MemberType = AllianceMemberType.Lead;
                AlliancePlayerInfo.TotalMembers = 1;
                AlliancePlayerInfo.JoinTimestamp = TimeUtils.Timestamp;
                AlliancePlayerInfo.ClearRequests();

                var chatManager = Connection.ChatManager;
                if(chatManager != null)
                {
                    var servicesDic = rDic.Get(ConnectionManager.ServicesKey).AsDic;
                    chatManager.ProcessChatServices(servicesDic.Get(ConnectionManager.ChatServiceKey).AsDic);
                }

                callback(null);

                AllianceEvent(AllianceAction.CreateAlliance, rDic);
            });
        }

        public void EditAlliance(Alliance current, AlliancesCreateData data, Action<Error> callback)
        {
            var dic = new AttrDic();
            dic.SetValue("user_id", LoginData.UserId.ToString());
            var dicProperties = new AttrDic();

            if(current.Description != data.Description)
            {
                dicProperties.SetValue("description", data.Description);
            }

            if(current.MinScoreToJoin != data.RequirementValue)
            {
                dicProperties.SetValue("minimum_score", data.RequirementValue);
            }

            if(current.AccessType != data.AccessType)
            {
                dicProperties.SetValue("type", data.AccessType != AllianceAccessType.Open ? 1 : 0);
            }

            if(current.AvatarId != data.AvatarId)
            {
                dicProperties.SetValue("avatar", data.AvatarId);
            }

            dic.Set("properties", dicProperties);

            Connection.Call("alliance.edit", Attr.InvalidList, dic, (err, rList, rDic) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    callback(err);
                    return;
                }

                AlliancePlayerInfo.AvatarId = data.AvatarId;

                callback(null);

                if(current.Description != data.Description)
                {
                    current.Description = data.Description;
                    AllianceEvent(AllianceAction.AllianceDescriptionEdited, rDic); // TODO Add trigger action for event
                }

                if(current.AvatarId != data.AvatarId)
                {
                    current.AvatarId = data.AvatarId;
                    AllianceEvent(AllianceAction.AllianceIconEdited, rDic); // TODO Change name?

                }

                if(current.AccessType != data.AccessType)
                {
                    current.AccessType = data.AccessType;
                    AllianceEvent(AllianceAction.AllianceTypeEdited, rDic);
                }

                if(current.MinScoreToJoin != data.RequirementValue) // TODO use same name in both classes
                {
                    current.MinScoreToJoin = data.RequirementValue;
                    AllianceEvent(AllianceAction.AllianceRequirementEdited, rDic);
                }
            });
        }

        public void AcceptCandidate(string candidateUid, Action<Error> callback)
        {
            var dic = new AttrDic();
            dic.SetValue("user_id", LoginData.UserId.ToString());
            dic.SetValue("new_member_id", long.Parse(candidateUid));

            Connection.Call("alliance.member.accept", Attr.InvalidList, dic, (err, rList, rDic) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    callback(err);
                    return;
                }

                callback(null);
                AllianceEvent(AllianceAction.MateChangedRank, rDic);
            });
        }

        public void DeclineCandidate(string candidateUid, Action<Error> callback)
        {
            var dic = new AttrDic();
            dic.SetValue("user_id", LoginData.UserId.ToString());
            dic.SetValue("denied_user_id", long.Parse(candidateUid));

            Connection.Call("alliance.member.decline", Attr.InvalidList, dic, (err, rList, rDic) => {
                callback(err);
            });
        }

        public void KickMember(string memberUid, Action<Error> callback)
        {
            var dic = new AttrDic();
            dic.SetValue("user_id", LoginData.UserId.ToString());
            dic.SetValue("kicked_user_id", long.Parse(memberUid));
            dic.SetValue("alliance_id", AlliancePlayerInfo.Id);

            Connection.Call("alliance.member.kickoff", Attr.InvalidList, dic, (err, rList, rDic) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    callback(err);
                    return;
                }

                callback(null);
                AllianceEvent(AllianceAction.MateChangedRank, rDic);
            });
        }

        public void PromoteMember(string memberUid, AllianceMemberType newType, Action<Error> callback)
        {
            var dic = new AttrDic();
            dic.SetValue("user_id", LoginData.UserId.ToString());
            dic.SetValue("promoted_user_id", long.Parse(memberUid));
            dic.SetValue("new_role", AllianceUtils.GetIndexForMemberType(newType));

            Connection.Call("alliance.member.promote", Attr.InvalidList, dic, (err, rList, rDic) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    callback(err);
                    return;
                }

                if(newType == AllianceMemberType.Lead)
                {
                    AlliancePlayerInfo.MemberType = AllianceMemberType.CoLead;
                }

                callback(null);
                AllianceEvent(AllianceAction.MateChangedRank, rDic);
            });
        }

        public void ParseAllianceInfo(AttrDic dic)
        {
            AlliancePlayerInfo = Factory.CreatePlayerInfo(MaxPendingJoinRequests, dic);
            AllianceEvent(AllianceAction.OnPlayerAllianceInfoParsed, dic);
        }

        public void SendNotificationAck(int typeCode, string notificationId)
        {
            var dic = new AttrDic();
            dic.SetValue("user_id", LoginData.UserId.ToString()); // TODO stoll? long or string?
            dic.SetValue("type", typeCode);
            dic.SetValue("notification_id", notificationId);

            Connection.Call("notification.received", Attr.InvalidList, dic, null);
        }


        #region Private methods

        public string AlliancesServerUrl;
        // FIXME top

        string GetUrl(string suffix)
        {
            return (string.IsNullOrEmpty(AlliancesServerUrl) ? LoginData.BaseUrl : AlliancesServerUrl) + suffix;
        }

        void JoinPublicAlliance(AllianceBasicData alliance, Action<Error> callback, JoinExtraData data)
        {
            var dic = new AttrDic();
            dic.SetValue("userid", LoginData.UserId.ToString());
            dic.SetValue("alliance_id", alliance.Id);
            dic.SetValue("timestamp", data.Timestamp);
            dic.SetValue("origin", data.Origin);

            long joinTs = data.Timestamp;

            Connection.Call("alliance.join", Attr.InvalidList, dic, (err, rList, rDic) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    callback(err);
                    return;
                }

                AlliancePlayerInfo.ClearRequests();
                AlliancePlayerInfo.Id = alliance.Id;
                AlliancePlayerInfo.Name = alliance.Name;
                AlliancePlayerInfo.AvatarId = alliance.AvatarId;
                AlliancePlayerInfo.MemberType = AllianceMemberType.Soldier;
                AlliancePlayerInfo.TotalMembers = alliance.MemberCount;
                AlliancePlayerInfo.JoinTimestamp = joinTs;

                var chatManager = Connection.ChatManager;
                if(chatManager != null)
                {
                    var servicesDic = rDic.Get(ConnectionManager.ServicesKey).AsDic;
                    chatManager.ProcessChatServices(servicesDic.Get(ConnectionManager.ChatServiceKey).AsDic);
                }

                callback(null);
                AllianceEvent(AllianceAction.JoinPublicAlliance, rDic);
            });
        }

        void JoinPrivateAlliance(AllianceBasicData alliance, Action<Error> callback, JoinExtraData data)
        {
            var dic = new AttrDic();
            dic.SetValue("user_id", LoginData.UserId.ToString());
            dic.SetValue("alliance_id", alliance.Id);
            dic.SetValue("timestamp", data.Timestamp);
            dic.SetValue("origin", data.Origin);

            Connection.Call("alliance.request.join", Attr.InvalidList, dic, (err, rList, rDic) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    callback(err);
                    return;
                }

                AlliancePlayerInfo.AddRequest(alliance.Id, MaxPendingJoinRequests);
                callback(null);
                AllianceEvent(AllianceAction.ApplyToAlliance, rDic);
            });
        }

        void ClearPlayerAllianceInfo()
        {
            AlliancePlayerInfo.ClearInfo();
            AllianceEvent(AllianceAction.OnPlayerAllianceInfoCleared, Attr.InvalidDic);
        }

        void OnPendingNotificationReceived(int type, string topic, AttrDic dic)
        {
            switch(type)
            {
            case NotificationTypeCode.NotificationAlliancePlayerAutoPromote:
                {
                    var notificationId = dic.GetValue(ConnectionManager.NotificationIdKey).ToString();
                    OnPlayerAutoChangedRank(dic, AllianceRankChange.Promotion);
                    SendNotificationAck(type, notificationId);
                    break;
                }
            case NotificationTypeCode.NotificationAlliancePlayerAutoDemote:
                {
                    var notificationId = dic.GetValue(ConnectionManager.NotificationIdKey).ToString();
                    OnPlayerAutoChangedRank(dic, AllianceRankChange.Demotion);
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
                    OnPlayerAutoChangedRank(dic, AllianceRankChange.Promotion);
                    SendNotificationAck(type, notificationId);
                    break;
                }
            case NotificationTypeCode.NotificationAlliancePlayerAutoDemote:
                {
                    var notificationId = dic.GetValue(ConnectionManager.NotificationIdKey).ToString();
                    OnPlayerAutoChangedRank(dic, AllianceRankChange.Demotion);
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
            var allianceId = dic.GetValue("alliance_id").ToString();
            var allianceName = dic.GetValue("alliance_name").ToString();
            var avatarId = dic.GetValue("alliance_symbol").ToInt(); // Symbol now, really? -.-
            var totalMembers = dic.GetValue("total_members").ToInt();
            var joinTs = dic.GetValue("join_ts").ToInt();

            // TODO Check duplicated code
            AlliancePlayerInfo.ClearRequests();
            AlliancePlayerInfo.Id = allianceId;
            AlliancePlayerInfo.Name = allianceName;
            AlliancePlayerInfo.AvatarId = avatarId;
            AlliancePlayerInfo.MemberType = AllianceMemberType.Soldier;
            AlliancePlayerInfo.TotalMembers = totalMembers;
            AlliancePlayerInfo.JoinTimestamp = joinTs;

            var chatManager = Connection.ChatManager;
            if(chatManager != null)
            {
                var servicesDic = dic.Get(ConnectionManager.ServicesKey).AsDic;
                chatManager.ProcessChatServices(servicesDic.Get(ConnectionManager.ChatServiceKey).AsDic);
            }

            AllianceEvent(AllianceAction.JoinPrivateAlliance, dic);
        }

        void OnKicked(AttrDic dic)
        {
            ClearPlayerAllianceInfo();
            var chatManager = Connection.ChatManager;
            if(chatManager != null)
            {
                chatManager.DeleteSubscription(chatManager.AllianceRoom);
            }
            AllianceEvent(AllianceAction.KickedFromAlliance, dic);
        }

        void OnPromoted(AttrDic dic)
        {
            var newRole = dic.GetValue("new_role").ToInt();
            AlliancePlayerInfo.MemberType = AllianceUtils.GetMemberTypeFromIndex(newRole);
            AllianceEvent(AllianceAction.PlayerChangedRank, dic);
        }

        void OnPlayerAutoChangedRank(AttrDic dic, AllianceRankChange rankChange)
        {
            var newRole = dic.GetValue("new_role").ToInt();
            AlliancePlayerInfo.MemberType = AllianceUtils.GetMemberTypeFromIndex(newRole);
            AllianceEvent(AllianceAction.PlayerChangedRank, dic);
        }

        void OnUserAppliedToPlayerAlliance(AttrDic dic)
        {
            DebugUtils.Assert(!AlliancePlayerInfo.IsInAlliance, "User is not in an alliance");
            AllianceEvent(AllianceAction.UserAppliedToPlayerAlliance, dic);
        }

        void OnMemberJoined(AttrDic dic)
        {
            AlliancePlayerInfo.IncreaseTotalMembers();
            AllianceEvent(AllianceAction.MateJoinedPlayerAlliance, dic);
        }

        void OnMemberLeft(AttrDic dic)
        {
            AlliancePlayerInfo.DecreaseTotalMembers();
            AllianceEvent(AllianceAction.MateJoinedPlayerAlliance, dic);
        }

        void OnAllianceEdited(AttrDic dic)
        {
            var changesDic = dic.Get("properties").AsDic;

            if(changesDic.ContainsKey("description"))
            {
                AllianceEvent(AllianceAction.AllianceDescriptionEdited, dic);
            }

            if(changesDic.ContainsKey("avatar"))
            {
                var newAvatar = changesDic.GetValue("avatar").ToInt();
                AlliancePlayerInfo.AvatarId = newAvatar;
                AllianceEvent(AllianceAction.AllianceIconEdited, dic);
            }

            if(changesDic.ContainsKey("type"))
            {
                AllianceEvent(AllianceAction.AllianceTypeEdited, dic);
            }

            if(changesDic.ContainsKey("minimum_score"))
            {
                AllianceEvent(AllianceAction.AllianceRequirementEdited, dic);
            }
        }

        void OnMemberPromoted(AttrDic dic)
        {
            AllianceEvent(AllianceAction.MateChangedRank, dic);
        }

        #endregion
    }
}
