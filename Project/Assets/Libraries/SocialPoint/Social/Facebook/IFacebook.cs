using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public delegate void FacebookAppRequestDelegate(FacebookAppRequest appReq, Error err);
    
    public delegate void FacebookWallPostDelegate(FacebookWallPost post, Error err);
    
    public delegate void FacebookGraphQueryDelegate(FacebookGraphQuery graph, Error err);
    
    public delegate void FacebookPhotoDelegate(Texture2D photo, Error err);
    
    public delegate void FacebookPermissionsDelegate(IDictionary<string, string> permissions, Error err);
    
    public delegate bool FacebookFriendFilterDelegate(FacebookUser fbUser);
    
    public delegate void FacebookStateChangeDelegate();

    public class FacebookUser
    {
        public string UserId { get; private set; }
        
        public string Name { get; private set; }
        
        public string AccessToken { get; set; }
        
        public string PhotoUrl { get; set; }
        
        public bool UsesApp { get; private set; }
        
        public FacebookUser(string id = "", string name = "", bool usesApp = false)
        {
            UserId = id;
            Name = name;
            UsesApp = usesApp;
        }
        
        public static bool operator ==(FacebookUser lu, FacebookUser ru)
        {
            if(System.Object.ReferenceEquals(lu, null))
            {
                if(System.Object.ReferenceEquals(ru, null))
                {
                    return true;
                }
                return false;
            }
            else if(System.Object.ReferenceEquals(ru, null))
            {
                return false;
            }
            
            return (lu.UserId == ru.UserId);
        }
        
        public static bool operator !=(FacebookUser lu, FacebookUser ru)
        {
            return !(lu == ru);
        }
        
        public override bool Equals(System.Object obj)
        {
            if(obj == null)
            {
                return false;
            }
            
            var p = obj as FacebookUser;
            if((System.Object)p == null)
            {
                return false;
            }
            
            return this == p;
        }
        
        public override int GetHashCode()
        {
            return UserId.GetHashCode();
        }
        
        public override string ToString()
        {
            return string.Format("[FacebookUser: UserId={0}, Name={1}, AccessToken={2}, UsesApp={3}]", UserId, Name, AccessToken, UsesApp);
        }
        
    }
    
    public class FacebookRequest
    {
        public AttrDic AdditionalData { get; private set; }
        
        public FacebookRequest()
        {
            AdditionalData = new AttrDic();
        }
        
        public string AdditionalDataJson()
        {
            return new JsonAttrSerializer().SerializeString(AdditionalData);
        }
    }

    public class FacebookAppRequestFilterGroup
    {
        public string Name;
        public List<string> UserIds;
    }
    
    public class FacebookAppRequest : FacebookRequest
    {
        public enum FilterType
        {
            Everybody,
            AppUsers,
            AppNonUsers
        };

        public string RequestId { get; set; }
        
        public bool RequestCancelled { get; set; }
        
        public List<string> To { get; set; }
        
        public List<string> Processed { get; set; }
        
        public string Message { get; set; }
        
        public string Title { get; set; }
        
        public AttrDic ResultParams { get; set; }
        
        public bool FrictionLess { get; set; }
        
        public string ActionType { get; private set; }
        
        public string ObjectId { get; private set; }
        
        public List<string> ExcludeIds { get; set; }

        public FilterType Filter { get; set; }

        public List<FacebookAppRequestFilterGroup> FilterGroups { get; set; }

        [Obsolete("Use Filter and FilterGroups")]
        public List<object> Filters{ get; set; }       
        
        const string RequestIdResultParam = "request";
        const string RequestCancelledParam = "cancelled";
        const string RequestErrorMesageParam = "error_code";
        const string ToSeparator = ",";
        const string ActionTypeSend = "send";
        const string ActionTypeAskFor = "askfor";
        
        public FacebookAppRequest()
        {
            FrictionLess = false;
            To = new List<string>();
            Processed = new List<string>();
            ExcludeIds = null;
        }
        
        public FacebookAppRequest(string message) : this()
        {
            Message = message;
        }
        
        public void SetTo(string to)
        {
            To.Clear();
            To.AddRange(to.Split(ToSeparator.ToArray()));
        }
        
        public string GetTo()
        {
            return string.Join(ToSeparator, To.ToArray());
        }
        
        public string ResultUrl
        {
            set
            {
                Result = new JsonAttrParser().ParseString(value).AsDic;
            }
        }
        
        public byte[] ResultData
        {
            set
            {
                ResultUrl = Encoding.UTF8.GetString(value);
            }
        }

        const int DialogButtonCancelled = 4201;
        
        public AttrDic Result
        {
            set
            {
                RequestCancelled = false;
                ResultParams = value;
                if(ResultParams.Get(RequestIdResultParam) != null)
                {
                    RequestId = ResultParams.GetValue(RequestIdResultParam).ToString();
                }
                RequestCancelled = ResultParams.GetValue(RequestCancelledParam).ToBool()
                    || ResultParams.GetValue(RequestErrorMesageParam).ToLong() == DialogButtonCancelled;
            }
        }
        
        public void SetSendObject(string objectId)
        {
            ActionType = ActionTypeSend;
            ObjectId = objectId;
        }
        
        public void SetAskForObject(string objectId)
        {
            ActionType = ActionTypeAskFor;
            ObjectId = objectId;
        }
        public override string ToString()
        {
            return string.Format("[FacebookAppRequest: RequestId={0}, Cancelled={1}, To={2}, Processed={3}, " +
                "Message={4}, Title={5}, ResultParams={6}, FrictionLess={7}, ActionType={8}, ObjectId={9}, ExcludeIds={10}, " +
                "Filter={11}]", RequestId, RequestCancelled, To, Processed,
                 Message, Title, ResultParams, FrictionLess, ActionType, ObjectId, ExcludeIds, Filter);
        }
    }
    
    public struct FacebookWallPostAction
    {
        public string Name;
        public string Link;
        
        public FacebookWallPostAction(string name, string link)
        {
            Name = name;
            Link = link;
        }

        public override string ToString()
        {
            return string.Format("[FacebookWallPostAction: Name={0}, Link={1}]", Name, Link);
        }
    }
    
    public class FacebookWallPost : FacebookRequest
    {
        public string To { get; set; }
        
        public string Picture { get; set; }
        
        public string Link { get; set; }
        
        public string Name { get; set; }
        
        public string Caption { get; set; }
        
        public string Message { get; set; }
        
        public string Description { get; set; }
        
        public string PostId { get; set; }
        
        public List<FacebookWallPostAction> Actions { get; private set; }
        
        public FacebookWallPost(string name = "", string message = "")
        {
            Actions = new List<FacebookWallPostAction>();
            Name = name;
            Message = message;
        }
        
        public void AddAction(string name, string link)
        {
            Actions.Add(new FacebookWallPostAction(name, link));
        }
        
        public string GetActionsJson()
        {
            if(Actions.Count == 0)
            {
                return string.Empty;
            }
            
            var list = new AttrList();
            for(int k = 0; k < Actions.Count; k++)
            {
                FacebookWallPostAction data = Actions[k];
                AttrDic dic = new AttrDic();
                dic.Set("name", new AttrString(data.Name));
                dic.Set("link", new AttrString(data.Link));
                list.Add(dic);
            }
            
            return new JsonAttrSerializer().SerializeString(list);
        }

        public override string ToString()
        {
            var actions = new string[Actions.Count];
            var i = 0;
            foreach(var action in Actions)
            {
                actions[i++] = action.ToString();
            }
            return string.Format("[FacebookWallPost: To={0}, Picture={1}, Link={2}, Name={3}, Caption={4}, " +
                "Message={5}, Description={6}, PostId={7}, Actions={8}]", To, Picture, Link, Name, Caption,
                 Message, Description, PostId, string.Join(", ", actions));
        }
    }
    
    public class FacebookGraphQuery : FacebookRequest
    {
        
        public enum MethodType
        {
            GET,    
            POST,
            DELETE
        }
        
        public string Path { get; set; }
        
        public MethodType Method { get; set; }
        
        public AttrDic Response { get; set; }
        
        public AttrDic Params { get; set; }
        
        public Dictionary<string, string> FlatParams
        {
            get
            {
                var data = new UrlQueryAttrSerializer().Serialize(Params);
                return StringUtils.QueryToDictionary(data.ToString());
            }
        }
        
        public FacebookGraphQuery()
        {
            Response = new AttrDic();
            Params = new AttrDic();
        }
        
        public FacebookGraphQuery(string path, MethodType method) : this()
        {
            Path = path;
            Method = method;
        }
        
        public void AddParam(string key, string value)
        {
            Params.SetValue(key, value);
        }
        
        public bool HasResponse()
        {
            return Response.Count > 0;
        }

        public override string ToString()
        {
            return string.Format("[FacebookGraphQuery: Path={0}, Method={1}, Response={2}," +
                "Params={3}]", Path, Method, Response, Params);
        }
    }

    public static class FacebookErrors
    {
        public const int LoginNeedsUI = 100;
        public const int DialogCancelled = 101;
    }

    public interface IFacebook
    {
        event FacebookStateChangeDelegate StateChangeEvent;
       
        string GetFriendsIds(FacebookFriendFilterDelegate includeFriend = null);
        void SendAppRequest(FacebookAppRequest req, FacebookAppRequestDelegate cbk = null);
        void PostOnWall(FacebookWallPost post, FacebookWallPostDelegate cbk = null);
        void PostOnWallWithDialog(FacebookWallPost pub, FacebookWallPostDelegate cbk = null);
        void QueryGraph(FacebookGraphQuery req, FacebookGraphQueryDelegate cbk);
        void Login(ErrorDelegate cbk, bool withUi = true);
        void Logout(ErrorDelegate cbk);
        void LoadPhoto(string userId, FacebookPhotoDelegate cbk);
        void AskForPermission(string permission, FacebookPermissionsDelegate cbk = null);
        void AskForPermissions(List<string> permissions, FacebookPermissionsDelegate cbk = null);
        void RefreshFriends(ErrorDelegate cbk = null);

        bool IsConnected{ get; }
        bool IsConnecting{ get; }
        string AppId{ set; }
        FacebookUser User{ get; }
        List<FacebookUser> Friends{ get; }
        List<string> LoginPermissions{ get; }

    }
}
