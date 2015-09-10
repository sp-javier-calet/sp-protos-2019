using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;
using Facebook;

namespace SocialPoint.Social
{
    public delegate void PlatformBridgeSessionDelegate(string session,string status,string error);

    public class UnityFacebook : BaseFacebook
    {
        static string GraphApiVersion = "v2.0";

        public enum States
        {
            LoggedIn,
            LoggingIn,
            LoggedOut,
            LoggingOut,
            LoginCancelled,
            Error
        }

        private States _state = States.LoggedOut;

        public States State
        {
            get
            {
                return _state;
            }
            private set
            {
                if(_state != value)
                {
                    _state = value;
                    NotifyStateChanged();
                }
            }
        }

        private FacebookUser _user;
        private uint _loginRetries;
        private uint _maxLoginRetries = 3;
        private List<FacebookUser> _friends = new List<FacebookUser>();
        private List<string> _loginPermissions = new List<string>();
        private Dictionary<string, string> _userPermissions;
        private MonoBehaviour _behaviour;

        private event ErrorDelegate _eventCallback;

        public UnityFacebook(MonoBehaviour behaviour)
        {
            _behaviour = behaviour;
        }

        public override List<string> LoginPermissions
        {
            get
            {
                return _loginPermissions;
            }
        }

        public override bool IsConnected
        {
            get
            {
                return State == States.LoggedIn;
            }
        }

        public override bool IsConnecting
        {
            get
            {
                return State == States.LoggingIn || State == States.LoggingOut;
            }
        }

        public override bool HasError
        {
            get
            {
                return State == States.Error;
            }
        }

        public override List<FacebookUser> Friends
        {
            get
            {
                return _friends;
            }
        }

        public override FacebookUser User
        {
            get
            {
                return _user;
            }
        }

        public override void SendAppRequest(FacebookAppRequest req, FacebookAppRequestDelegate cbk = null)
        {
            FB.AppRequest
            (
                req.Message,
                req.To.Count > 0 ? req.To.ToArray() : null,
                req.Filters,
                req.ExcludeIds,
                null,
                req.AdditionalDataToString(),
                req.Title,
                (FBResult response) =>
            {
                Error err = null;
                if(!string.IsNullOrEmpty(response.Error))
                {
                    err = new Error(response.Error);
                }
                if(!string.IsNullOrEmpty(response.Text))
                {
                    req.ResultUrl = response.Text;
                    
                    if(req.RequestCancelled)
                    {
                        err = new Error(FacebookErrors.DialogCancelled);
                    }
                }
                if(cbk != null)
                {
                    cbk(req, err);
                }
            }
            );
        }

        public override void PostOnWallWithDialog(FacebookWallPost post, FacebookWallPostDelegate cbk = null)
        {
            string userId = post.To;
            if(userId == "me")
            {
                userId = string.Empty;
            }

            FB.Feed
            (
                userId,
                post.Link,
                post.Name,
                post.Caption,
                post.Description,
                post.Picture,
                string.Empty,
                post.GetActionsJson(),
                string.Empty,
                string.Empty,
                null,
                (FBResult response) =>
            {
                Error err = null;
                if(!string.IsNullOrEmpty(response.Error))
                {
                    err = new Error(response.Error);
                }
                if(!string.IsNullOrEmpty(response.Text))
                {
                    JsonAttrParser parser = new JsonAttrParser();
                    Attr attr = parser.ParseString(response.Text);
                    post.PostId = attr.AsDic.GetValue("post_id").ToString();
                }

                if(cbk != null)
                {
                    cbk(post, err);
                }
            }
            );
        }

        bool HasPermissions(List<string> permissions)
        {
            foreach(var perm in permissions)
            {
                if(_userPermissions == null || !_userPermissions.ContainsKey(perm))
                {
                    return false;
                }
            }
            return true;
        }
       
        public override void AskForPermissions(List<string> permissions, FacebookPermissionsDelegate cbk = null)
        {
            var allPermissions = _userPermissions;
            Error err = null;
            if(!HasPermissions(permissions))
            {
                err = new Error("User does not have the requested permissions.");
            }
            if(cbk != null)
            {
                cbk(allPermissions, err);
            }
        }

        void SessionCompletionHandler(Error err)
        {
            if(State == States.LoggingIn)
            {
                DidLogin(err, _eventCallback);
            }
            else if(State == States.LoggingOut)
            {
                DidLogout(err, _eventCallback);
            }
            _eventCallback = null;
        }

        void DidLogin(Error err, ErrorDelegate cbk)
        {
            if(!Error.IsNullOrEmpty(err))
            {
                if(err.Code != FacebookErrors.DialogCancelled &&  _loginRetries < _maxLoginRetries)
                {
                    _loginRetries++;
                    FB.Logout();
                    DoLogin(cbk, true);
                    return;
                }
                else
                {
                    State = States.Error;
                }
                if(cbk != null)
                {
                    cbk(err);
                }
            }
            else
            {
                GetLoginSessionInfo(cbk);
            }
        }

        void OnLoginEnd(Error err, ErrorDelegate cbk)
        {
            if(!Error.IsNullOrEmpty(err))
            {
                State = States.Error;
            }
            else
            {
                State = States.LoggedIn;
            }

            if(cbk != null)
            {
                cbk(err);
            }
        }

        void DidLogout(Error err, ErrorDelegate cbk)
        {
            _user = new FacebookUser();
            _friends.Clear();
            if(!Error.IsNullOrEmpty(err))
            {
                State = States.Error;
            }
            else
            {
                State = States.LoggedOut;
            }
            if(cbk != null)
            {
                cbk(err);
            }
        }

        FacebookUser ParseUser(AttrDic dicUser)
        {
            string userId = dicUser.GetValue("id").ToString();
            string name = dicUser.GetValue("name").ToString();
            if(userId != null && name != null)
            {
                bool usesApp = dicUser.GetValue("installed").ToBool();
                var user = new FacebookUser(userId, name, usesApp);
                var picture = dicUser.Get("picture").AsDic.Get("data").AsDic;
                if(!picture.GetValue("is_silhouette").ToBool())
                {
                    user.PhotoUrl = picture.GetValue("url").ToString();
                }
                return user;
            }
            return null;
        }

        void GetLoginSessionInfo(ErrorDelegate cbk)
        {
            var s = UserPhotoSize;
            var uri = GraphApiVersion + "/me";
            uri += "?fields=id,name,installed,picture.width(" + s + ").height(" + s + ")";
            FB.API(uri, HttpMethod.GET, (FBResult result) => {
                if(!string.IsNullOrEmpty(result.Error))
                {
                    if(cbk != null)
                    {
                        cbk(new Error(result.Error));
                    }
                }
                else
                {
                    JsonAttrParser parser = new JsonAttrParser();
                    Attr attr = parser.ParseString(result.Text);
                    _user = ParseUser(attr.AsDic);
                    if(_user != null)
                    {
                        _user.AccessToken = FB.AccessToken;
                        GetLoginPermissions(cbk);
                    }
                    else
                    {
                        if(cbk != null)
                        {
                            cbk(new Error("Could not read the user json"));
                        }
                    }

                }
            });
        }

        void GetLoginPermissions(ErrorDelegate cbk)
        {
            FB.API(GraphApiVersion + "/me/permissions", HttpMethod.GET, (FBResult result) => {
                if(!string.IsNullOrEmpty(result.Error))
                {
                    if(cbk != null)
                    {
                        cbk(new Error(result.Error));
                    }
                }
                else
                {
                    _userPermissions = new Dictionary<string, string>();
                    JsonAttrParser parser = new JsonAttrParser();
                    Attr attr = parser.ParseString(result.Text);
                    var perm = attr.AsDic.Get("data").AsList;
                    for(int k = 0; k < perm.Count; k++)
                    {
                        AttrDic dicPerm = perm[k].AsDic;
                        string permission = dicPerm.GetValue("permission").ToString();
                        string status = dicPerm.GetValue("status").ToString();
                        if(!string.IsNullOrEmpty(permission) && !string.IsNullOrEmpty(status))
                        {
                            _userPermissions.Add(permission, status);
                        }
                    }
                    GetLoginFriendsInfo("/me/friends", (err) => {
                        if(!Error.IsNullOrEmpty(err))
                        {
                            if(cbk != null)
                            {
                                cbk(err);
                            }
                        }
                        else
                        {
                            GetLoginFriendsInfo("/me/invitable_friends", (err2) => {
                                if(cbk != null)
                                {
                                    cbk(null);
                                }
                            });
                        }
                    });
                }
            });
        }

        void GetLoginFriendsInfo(string path, ErrorDelegate cbk)
        {

            var s = UserPhotoSize;
            var uri = GraphApiVersion + path;
            uri += "?fields=id,name,installed,picture.width(" + s + ").height(" + s + ")";

            FB.API(uri.ToString(), HttpMethod.GET, (FBResult result) => {
                var err = new Error(result.Error);
                if(Error.IsNullOrEmpty(err))
                {
                    JsonAttrParser parser = new JsonAttrParser();
                    Attr attr = parser.ParseString(result.Text);

                    AttrList users = attr.AsDic.Get("data").AsList;
                    for(int k = 0; k < users.Count; k++)
                    {
                        var user = ParseUser(users[k].AsDic);
                        if(user != null && !_friends.Contains(user))
                        {
                            _friends.Add(user);
                        }
                    }
                }
                if(cbk != null)
                {
                    cbk(err);
                }
            });
        }

        public override void Login(ErrorDelegate cbk = null, bool withUi = true)
        {
            if(State == States.LoggedIn)
            {
                if(cbk != null)
                {
                    cbk(null);
                }
                return;
            }
            else if(State != States.LoggedOut && State != States.Error)
            {
                if(cbk != null)
                {
                    Error err = null;
                    if(State == States.LoggingIn)
                    {
                        err = new Error("Currently logging in.");
                    }
                    if(State == States.LoggingOut)
                    {
                        err = new Error("Currently logging out.");
                    }
                    cbk(err);
                }
                return;
            }

            State = States.LoggingIn;

            if(!string.IsNullOrEmpty(FB.AppId))
            {
                DoLogin(cbk, withUi);
            }
            else
            {
                FB.Init(() => DoLogin(cbk, withUi));
            }
        }

        void DoLogin(ErrorDelegate cbk, bool withUi)
        {
            if(!withUi && FB.AccessToken == null)
            {
                if(cbk != null)
                {
                    var err = new Error(FacebookErrors.LoginNeedsUI, "Login needs ui.");
                    cbk(err);
                }
                return;
            }

            _eventCallback += (epet) => OnLoginEnd(epet, cbk);

            FB.Login(string.Join(",", _loginPermissions.ToArray()), (FBResult response) => {
                var err = new Error(response.Error);
                if(Error.IsNullOrEmpty(err) && !FB.IsLoggedIn)
                {
                    err = new Error(FacebookErrors.DialogCancelled, "Login cancelled.");
                }
                SessionCompletionHandler(err);
            });
        }

        public override void QueryGraph(FacebookGraphQuery query, FacebookGraphQueryDelegate cbk = null)
        {
            Dictionary<string, string> dic = query.FlatParams;

            HttpMethod fbMethod;
            switch(query.Method)
            {
            case FacebookGraphQuery.MethodType.POST:
                fbMethod = HttpMethod.POST;
                break;
            case FacebookGraphQuery.MethodType.DELETE:
                fbMethod = HttpMethod.DELETE;
                break;
            default:
                fbMethod = HttpMethod.GET;
                break;
            }

            FB.API(GraphApiVersion + "/" + query.Path, fbMethod, (FBResult response) => {
                var err = new Error(response.Error);
                if(Error.IsNullOrEmpty(err))
                {
                    JsonAttrParser parser = new JsonAttrParser();
                    Attr attr = parser.ParseString(response.Text);
                    query.Response = attr.AsDic;
                }

                if(cbk != null)
                {
                    cbk(query, err);
                }
            },
            dic);
        }

        public override void Logout(ErrorDelegate cbk = null)
        {
            if(State == States.LoggedOut)
            {
                if(cbk != null)
                {
                    cbk(null);
                }
                return;
            }
            else if(State != States.LoggedIn && State != States.Error)
            {
                if(cbk != null)
                {
                    Error err = null;
                    if(State == States.LoggingIn)
                    {
                        err = new Error("Currently logging in.");
                    }
                    if(State == States.LoggingOut)
                    {
                        err = new Error("Currently logging out.");
                    }
                    cbk(err);
                }
                return;
            }
            State = States.LoggingOut;
            _eventCallback = cbk;
            FB.Logout();
            SessionCompletionHandler(null);
        }

        public override string AppId
        {
            set
            {
                throw new Exception("Unity Facebook SDK does not have the option to set the app id programatically.");
            }
        }

        private IEnumerator LoadPhotoFromUrlCorroutine(string url, FacebookPhotoDelegate cbk = null)
        {
            var www = new WWW(url);
            yield return www;
            if(cbk != null)
            {
                cbk(www.texture, new Error(www.error));
            }
        }

        private void LoadPhotoFromUrl(string url, FacebookPhotoDelegate cbk = null)
        {
            _behaviour.StartCoroutine(LoadPhotoFromUrlCorroutine(url, cbk));
        }

        public override void LoadPhoto(string userId, FacebookPhotoDelegate cbk = null)
        {
            if(_user != null && userId == _user.UserId && !string.IsNullOrEmpty(_user.PhotoUrl))
            {
                LoadPhotoFromUrl(_user.PhotoUrl, cbk);
            }
            if(_friends != null)
            {
                for(int i=0; i<_friends.Count; ++i)
                {
                    var user = _friends[i];
                    if(_user != null && userId == user.UserId && !string.IsNullOrEmpty(user.PhotoUrl))
                    {
                        LoadPhotoFromUrl(user.PhotoUrl, cbk);
                        return;
                    }
                }
            }

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("redirect", "1");
            dic.Add("type", "square");
            dic.Add("width", UserPhotoSize.ToString());
            dic.Add("height", UserPhotoSize.ToString());

            FB.API(GraphApiVersion + "/" + userId + "/picture", HttpMethod.GET, (FBResult response) => {
                if(cbk != null)
                {
                    cbk(response.Texture, new Error(response.Error));
                }
            },
            dic);
        }
    }
}
