using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;
using Facebook.Unity;

namespace SocialPoint.Social
{
    public delegate void PlatformBridgeSessionDelegate(string session,string status,string error);

    public class UnityFacebook : BaseFacebook
    {
        private bool _connecting = false;
        private FacebookUser _user;
        private uint _loginRetries;
        private uint _maxLoginRetries = 3;
        private List<FacebookUser> _friends = new List<FacebookUser>();
        private List<string> _loginPermissions = new List<string>();
        private Dictionary<string, string> _userPermissions;
        private MonoBehaviour _behaviour;

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
                return FB.IsLoggedIn;
            }
        }

        public override bool IsConnecting
        {
            get
            {
                return _connecting;
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

        const string AppUsersValue = "app_users";
        const string AppNonUsersValue = "app_non_users";

        public uint UserPhotoSize = 100;

        public override void SendAppRequest(FacebookAppRequest req, FacebookAppRequestDelegate cbk = null)
        {
            if(!IsConnected)
            {
                if(cbk != null)
                {
                    cbk(req, new Error("Facebook is not logged in"));
                }
                return;
            }

#pragma warning disable 0618
            var filters = req.Filters;
#pragma warning restore 0618
            if(filters == null)
            {
                filters = new List<object>();
            }
            if(req.Filter == FacebookAppRequest.FilterType.AppUsers)
            {
                filters.Add(AppUsersValue);
            }
            else if(req.Filter == FacebookAppRequest.FilterType.AppNonUsers)
            {
                filters.Add(AppNonUsersValue);
            }
            if(req.FilterGroups != null)
            {
                foreach(var group in req.FilterGroups)
                {
                    filters.Add(group);
                }
            }

            FB.AppRequest
            (
                req.Message == null ? string.Empty : req.Message,
                req.To.Count > 0 ? req.To.ToArray() : null,
                filters,
                req.ExcludeIds == null ? null : req.ExcludeIds.ToArray(),
                null,
                req.AdditionalDataJson(),
                req.Title == null ? string.Empty : req.Title,
                    (IAppRequestResult response) =>
            {
                Error err = null;
                if(!string.IsNullOrEmpty(response.Error))
                {
                    err = new Error(response.Error);
                }
                if(!string.IsNullOrEmpty(response.RawResult))
                {
                        //TODO: consider using response.ResultDictionary
                    req.ResultUrl = response.RawResult;
                    
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
            if(!IsConnected)
            {
                if(cbk != null)
                {
                    cbk(post, new Error("Facebook is not logged in"));
                }
                return;
            }
            string userId = post.To;
            if(userId == "me" || userId == null)
            {
                userId = string.Empty;
            }

            FB.FeedShare
            (
                userId,
                post.Link == null ? null : post.Link,
                post.Name == null ? string.Empty : post.Name,
                post.Caption == null ? string.Empty : post.Caption,
                post.Description == null ? string.Empty : post.Description,
                post.Picture == null ? null : post.Picture,
                string.Empty,
                (IShareResult response) =>
            {
                Error err = null;
                if(!string.IsNullOrEmpty(response.Error))
                {
                    err = new Error(response.Error);
                }
                if(!string.IsNullOrEmpty(response.PostId))
                {
                    post.PostId = response.PostId;
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
            if(!IsConnected)
            {
                if(cbk != null)
                {
                    cbk(new Dictionary<string,string>(), new Error("Facebook is not logged in"));
                }
                return;
            }
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

        void DidLogin(Error err, ErrorDelegate cbk)
        {
            if(!Error.IsNullOrEmpty(err))
            {
                if(err.Code != FacebookErrors.DialogCancelled &&  _loginRetries < _maxLoginRetries)
                {
                    _loginRetries++;
                    FB.LogOut();
                    DoLogin(cbk, true);
                }
                else
                {
                    OnLoginEnd(err, cbk);
                }
            }
            else
            {
                GetLoginSessionInfo(cbk);
            }
        }

        void OnLoginEnd(Error err, ErrorDelegate cbk)
        {
            _connecting = false;
            if(IsConnected)
            {
                NotifyStateChanged();
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
            if(!IsConnected)
            {
                NotifyStateChanged();
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
            var uri = "me?fields=id,name,installed,picture.width(" + s + ").height(" + s + ")";
            FB.API(uri, HttpMethod.GET, (IGraphResult result) => {
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
                    Attr attr = parser.ParseString(result.RawResult);
                    _user = ParseUser(attr.AsDic);
                    if(_user != null)
                    {
                        _user.AccessToken = AccessToken.CurrentAccessToken.TokenString;
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
            FB.API("me/permissions", HttpMethod.GET, (IGraphResult result) => {
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
                    Attr attr = parser.ParseString(result.RawResult);
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

        public override void RefreshFriends(ErrorDelegate cbk = null)
        {
            GetLoginFriendsInfo("/me/friends", cbk);
        }

        void GetLoginFriendsInfo(string path, ErrorDelegate cbk)
        {
            var s = UserPhotoSize;
            var uri = path + "?fields=id,name,installed,picture.width(" + s + ").height(" + s + ")";

            FB.API(uri.ToString(), HttpMethod.GET, (IGraphResult result) => {
                var err = new Error(result.Error);
                if(Error.IsNullOrEmpty(err))
                {
                    JsonAttrParser parser = new JsonAttrParser();
                    Attr attr = parser.ParseString(result.RawResult);

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
            if(FB.IsInitialized)
            {
                checkHasAccess(cbk, withUi);
            }
            else
            {
                FB.Init(() => checkHasAccess(cbk, withUi));
            }
        }

        void checkHasAccess(ErrorDelegate cbk, bool withUi)
        {
            if(AccessToken.CurrentAccessToken != null)
            {
                DidLogin(null,cbk);
            }
            else
            {
                DoLogin(cbk, withUi);
            }
        }

        void DoLogin(ErrorDelegate cbk, bool withUi)
        {
            if(IsConnected)
            {
                if(cbk != null)
                {
                    cbk(null);
                }
                return;
            }
            if(!withUi && AccessToken.CurrentAccessToken == null)
            {
                if(cbk != null)
                {
                    var err = new Error(FacebookErrors.LoginNeedsUI, "Login needs ui.");
                    cbk(err);
                }
                return;
            }
            _connecting = true;

            #if UNITY_EDITOR
            //_behaviour.StartCoroutine(CheckEditorLoginFail(cbk));
            #endif
            FB.LogInWithReadPermissions(_loginPermissions, (ILoginResult response) => {
                var err = new Error(response.Error);
                if(Error.IsNullOrEmpty(err) && !FB.IsLoggedIn)
                {
                    err = new Error(FacebookErrors.DialogCancelled, "Login cancelled.");
                }
                DidLogin(err, cbk);
            });
        }

        /*
        IEnumerator CheckEditorLoginFail(ErrorDelegate cbk)
        {
            bool loaded = false;
            while(_connecting)
            {
                var token = GameObject.FindObjectOfType<EditorFacebookAccessToken>();
                if(token == null)
                {
                    if(loaded)
                    {
                        var err = new Error(FacebookErrors.DialogCancelled, "Invalid editor login access token.");
                        DidLogin(err, cbk);
                    }
                    else
                    {
                        loaded = true;
                    }
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
        */

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

            FB.API(query.Path, fbMethod, (IGraphResult response) => {
                var err = new Error(response.Error);
                if(Error.IsNullOrEmpty(err))
                {//TODO: evaluate if necessary parser or use response dict
                    JsonAttrParser parser = new JsonAttrParser();
                    Attr attr = parser.ParseString(response.RawResult);
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
            if(FB.IsLoggedIn)
            {
                FB.LogOut();
            }
            DidLogout(null, cbk);
        }

        public override string AppId
        {
            set
            {
                throw new Exception("Unity Facebook SDK does not have the option to set the app id programatically.");
            }
        }

        private IEnumerator LoadPhotoFromUrlCoroutine(string url, FacebookPhotoDelegate cbk = null)
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
            _behaviour.StartCoroutine(LoadPhotoFromUrlCoroutine(url, cbk));
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

            FB.API(userId + "/picture", HttpMethod.GET, (IGraphResult response) => {
                if(cbk != null)
                {
                    cbk(response.Texture, new Error(response.Error));
                }
            },
            dic);
        }
    }
}
