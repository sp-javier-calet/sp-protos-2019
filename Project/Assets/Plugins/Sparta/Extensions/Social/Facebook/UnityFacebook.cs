using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;
using Facebook.Unity;

namespace SocialPoint.Social
{
    public delegate void PlatformBridgeSessionDelegate(string session, string status, string error);

    public sealed class UnityFacebook : BaseFacebook
    {
        const uint kMaxLoginRetries = 3;

        bool _connecting;
        FacebookUser _user;
        uint _loginRetries;
        readonly List<FacebookUser> _friends;
        readonly List<string> _loginPermissions;
        List<string> _userPermissions;
        ICoroutineRunner _runner;

        public UnityFacebook(ICoroutineRunner runner)
        {
            _friends = new List<FacebookUser>();
            _loginPermissions = new List<string>();
            _runner = runner;
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
                return FB.IsLoggedIn && !_connecting;
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
            filters = filters ?? new List<object>();

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
                for(int i = 0, reqFilterGroupsCount = req.FilterGroups.Count; i < reqFilterGroupsCount; i++)
                {
                    var group = req.FilterGroups[i];
                    filters.Add(group);
                }
            }

            FB.AppRequest
            (
                req.Message ?? string.Empty,
                req.ArrayTo,
                filters,
                req.ArrayExcludeIds,
                null,
                req.AdditionalDataJson(),
                req.Title ?? string.Empty,
                response => {
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

        public override void PostOnWallWithDialog(FacebookWallPost pub, FacebookWallPostDelegate cbk = null)
        {
            if(!IsConnected)
            {
                if(cbk != null)
                {
                    cbk(pub, new Error("Facebook is not logged in"));
                }
                return;
            }
            string userId = pub.To;
            if(userId == "me" || userId == null)
            {
                userId = string.Empty;
            }

            FB.FeedShare
            (
                userId,
                pub.Link,
                pub.Name ?? string.Empty,
                pub.Caption ?? string.Empty,
                pub.Description ?? string.Empty,
                pub.Picture,
                string.Empty,
                response => {
                    Error err = null;
                    if(!string.IsNullOrEmpty(response.Error))
                    {
                        err = new Error(response.Error);
                    }
                    if(!string.IsNullOrEmpty(response.PostId))
                    {
                        pub.PostId = response.PostId;
                    }

                    if(cbk != null)
                    {
                        cbk(pub, err);
                    }
                }
            );
        }

        public override bool HasPermissions(IList<string> permissions)
        {
            for(int i = 0, permissionsCount = permissions.Count; i < permissionsCount; i++)
            {
                var perm = permissions[i];
                if(_userPermissions == null || !_userPermissions.Contains(perm))
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
                    cbk(new List<string>(), new Error("Facebook is not logged in"));
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
                if(err.Code != FacebookErrors.DialogCancelled && _loginRetries < kMaxLoginRetries)
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
            NotifyStateChanged();
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

        static FacebookUser ParseUser(AttrDic dicUser)
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
            FB.API(uri, HttpMethod.GET, result => {
                if(!string.IsNullOrEmpty(result.Error))
                {
                    OnLoginEnd(new Error(result.Error), cbk);
                }
                else
                {
                    var parser = new JsonAttrParser();
                    Attr attr = parser.ParseString(result.RawResult);
                    _user = ParseUser(attr.AsDic);
                    if(_user != null)
                    {
                        _user.AccessToken = AccessToken.CurrentAccessToken.TokenString;
                        _userPermissions = new List<string>(AccessToken.CurrentAccessToken.Permissions);
                        GetLoginFriendsInfo("/me/friends", err => {
                            if(!Error.IsNullOrEmpty(err))
                            {
                                OnLoginEnd(err, cbk);
                            }
                            else
                            {
                                GetLoginFriendsInfo("/me/invitable_friends", err2 => OnLoginEnd(err2, cbk));
                            }
                        });
                    }
                    else
                    {
                        OnLoginEnd(new Error("Could not read the user json"), cbk);
                    }
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
            FB.API(uri, HttpMethod.GET, result => {
                var err = new Error(result.Error);
                if(Error.IsNullOrEmpty(err))
                {
                    var parser = new JsonAttrParser();
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

                    InitializedFriends = true;
                }
                if(cbk != null)
                {
                    cbk(err);
                }
            });
        }

        public override void Login(ErrorDelegate cbk, bool withUi = true)
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
                DidLogin(null, cbk);
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

            FB.LogInWithReadPermissions(_loginPermissions, response => {
                var err = new Error(response.Error);
                if(Error.IsNullOrEmpty(err) && !FB.IsLoggedIn)
                {
                    err = new Error(FacebookErrors.DialogCancelled, "Login cancelled.");
                }
                DidLogin(err, cbk);
            });
        }

        public override void QueryGraph(FacebookGraphQuery req, FacebookGraphQueryDelegate cbk)
        {
            Dictionary<string, string> dic = req.FlatParams;

            HttpMethod fbMethod;
            switch(req.Method)
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

            FB.API(req.Path, fbMethod, response => {
                var err = new Error(response.Error);
                if(Error.IsNullOrEmpty(err))
                {//TODO: evaluate if necessary parser or use response dict
                    var parser = new JsonAttrParser();
                    Attr attr = parser.ParseString(response.RawResult);
                    req.Response = attr.AsDic;
                }

                if(cbk != null)
                {
                    cbk(req, err);
                }
            },
                dic);
        }

        public override void Logout(ErrorDelegate cbk)
        {
            if(FB.IsLoggedIn)
            {
                FB.LogOut();
            }
            DidLogout(null, cbk);
            InitializedFriends = false;
            _connecting = false;
        }

        public override string AppId
        {
            get
            {
                return FB.AppId;
            }
            set
            {
                throw new Exception("Unity Facebook SDK does not have the option to set the app id programatically.");
            }
        }

        public override string ApiVersion 
        { 
            get
            {
                return FB.GraphApiVersion;
            }
            set
            {
                FB.GraphApiVersion = value;
            }
        }

        void LoadPhotoFromUrl(string url, FacebookPhotoDelegate cbk = null)
        {
            _runner.DownloadTexture(url, (tex, err) => {
                if(cbk != null)
                {
                    cbk(tex, err);
                }
            });
        }

        public override void LoadPhoto(string userId, FacebookPhotoDelegate cbk)
        {
            if(_user != null && userId == _user.UserId && !string.IsNullOrEmpty(_user.PhotoUrl))
            {
                LoadPhotoFromUrl(_user.PhotoUrl, cbk);
            }
            if(_friends != null)
            {
                for(int i = 0; i < _friends.Count; ++i)
                {
                    var user = _friends[i];
                    if(_user != null && userId == user.UserId && !string.IsNullOrEmpty(user.PhotoUrl))
                    {
                        LoadPhotoFromUrl(user.PhotoUrl, cbk);
                        return;
                    }
                }
            }

            var dic = new Dictionary<string, string>();
            dic.Add("redirect", "1");
            dic.Add("type", "square");
            dic.Add("width", UserPhotoSize.ToString());
            dic.Add("height", UserPhotoSize.ToString());

            FB.API(userId + "/picture", HttpMethod.GET, response => {
                if(cbk != null)
                {
                    cbk(response.Texture, new Error(response.Error));
                }
            },
                dic);
        }
    }
}