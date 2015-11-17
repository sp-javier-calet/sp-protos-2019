using System;
using System.Collections.Generic;
using System.Linq;

using SocialPoint.Attributes;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.Utils;
using SocialPoint.Base;

namespace SocialPoint.Login
{
    public delegate void TrackEventDelegate(string eventName, AttrDic data = null, ErrorDelegate del = null);        

    public class SocialPointLogin : ILogin
    {
        private const string BaseUri = "{0}/{1}";
        // UserId, DeviceId
        private const string LoginUri = "user/login";
        private const string LinkUri = "user/link";
        private const string LinkConfirmUri = "user/link/confirm";
        private const string UserMappingUri = "user/link/mapping";
        private const string AppRequestsUri = "requests";
        private const char UriSeparator = '/';

        private const string SecurityTokenStorageKey = "SocialPointLoginClientToken";
        private const string UserIdStorageKey = "SocialPointLoginUserId";
        private const string UserHasRegisteredStorageKey = "SocialPointLoginHasRegistered";

        private const string HttpParamSessionId = "session_id";
        private const string HttpParamDeviceModel = "device_model";
        private const string HttpParamSecurityToken = "security_token";
        private const string HttpParamClientVersion = "client_version";
        private const string HttpParamPlatform = "platform";
        private const string HttpParamClientLanguage = "client_language";
        private const string HttpParamDeviceLanguage = "device_language";
        private const string HttpParamUserIds = "ids";
        private const string HttpParamSocialPointUserIds = "sp";
        private const string HttpParamAppRequestUserIds = "to";
        private const string HttpParamAppRequestType = "type";
        private const string HttpParamTimestamp = "ts";
        private const string HttpParamPlatformVersion = "device_os";
        private const string HttpParamDeviceAid = "device_adid";
        private const string HttpParamDeviceAidEnabled = "device_adid_enabled";
        private const string HttpParamDeviceRooted = "device_rooted";     
        private const string HttpParamClientBuild = "client_build";
        private const string HttpParamClientAppId = "client_appid";
        private const string HttpParamLinkConfirmToken = "confirm_link_token";
        private const string HttpParamLinkDecision = "decision";
        private const string HttpParamLinkType = "provider_type";
        private const string HttpParamRequestIds = "request_ids";
        private const string HttpParamPrivilegeToken = "privileged_session_token";

        private const string AttrKeySessionId = "session_id";
        private const string AttrKeyLinksData = "linked_accounts";
        private const string AttrKeyUserId = "user_id";
        private const string AttrKeyLinkProvider = "provider_type";
        private const string AttrKeyLinkExternalId = "external_id";
        private const string AttrKeyConfirmLinkToken = "confirm_link_token";
        private const string AttrKeyLoginData = "login_data";
        private const string AttrKeyGameData = "game_data";
        private const string AttrKeyGenericData = "generic_data";
        public const string AttrKeyData = "data";
        private const string AttrKeyEventError = "error";
        private const string AttrKeyEventLogin = "login";
        private const string AttrKeyEventErrorType = "error_type";
        private const string AttrKeyEventErrorCode = "error_code";
        private const string AttrKeyEventErrorMessage = "error_desc";
        private const string AttrKeyEventErrorHttpCode = "error_code";
        private const string AttrKeyEventErrorData = "data";
        public const string AttrKeyHttpCode = "http_code";
        public const string AttrKeySignature = "signature";
                
        private const string EventNameLoading = "game.loading";
        private const string EventNameError = "errors.login_error";

        private const string SignatureSeparator = ":";
        private const string SignatureCodeSeparator = "-";

        private const int InvalidSecurityTokenError = 480;
        private const int InvalidSessionError = 482;
        private const int InvalidLinkDataError = 483;
        private const int InvalidProviderTokenError = 484;
        private const int InvalidPrivilegeTokenError = 486;
        private const int MaintenanceMode = 503;
        private const int LooseToLinkedError = 264;
        private const int LinkedToLooseError = 265;
        private const int LinkedToSameError = 266;
        private const int LinkedToLinkedError = 267;
        private const int ForceUpgradeError = 485;

        public const int DefaultMaxSecurityTokenErrorRetries = 5;
        public const int DefaultMaxConnectivityErrorRetries = 0;
        public const bool DefaultEnableLinkConfirmRetries = false;
        public const float DefaultTimeout = 30.0f;
        public const float DefaultActivityTimeout = 15.0f;
        public const bool DefaultAutoUpdateFriends = true;
        public const uint DefaultAutoUpdateFriendsPhotoSize = 0;
        public const uint DefaultUserMappingsBlock = 50;

        private LocalUser _user;

        public LocalUser User
        {
            get
            {
                return _user;
            }
            private set
            {
                _user = value;
            }
        }

        public List<User> Friends { get; private set; }

        public IDeviceInfo DeviceInfo { private get; set; }

        public IAttrStorage Storage { private get; set; }

        public float Timeout { private get; set; }

        public float ActivityTimeout { private get; set; }

        public bool AutoUpdateFriends { private get; set; }

        public uint AutoUpdateFriendsPhotosSize { private get; set; }

        public struct LoginConfig
        {
            public string BaseUrl;
            public int SecurityTokenErrors;
            public int ConnectivityErrors;
            public bool EnableOnLinkConfirm;
        }

        private LoginConfig _loginConfig;
        private int _availableSecurityTokenErrorRetries;
        private int _availableConnectivityErrorRetries;

        public uint UserMappingsBlock { private get; set; }

        public string Language { private get; set; }

        public GenericData Data { get; private set; }

        public string PrivilegeToken { get; set; }

        public UInt64 UserId
        {
            get
            {
                if(_userId == 0 && Storage != null)
                {
                    try
                    {
                        var attr = Storage.Load(UserIdStorageKey);
                        if(attr != null)
                        {
                            UInt64.TryParse(attr.ToString(), out _userId);
                        }
                    }
                    catch(Exception)
                    {
                    }
                }
                if(_userId == 0)
                {
                    _userId = RandomUtils.GenerateUserId();
                    UserHasRegistered = false;
                    StoreUserId();
                }

                return _userId;
            }

            set
            {
                _userId = value;
                UserHasRegistered = _userId != 0;
                StoreUserId();
            }
        }

        public bool UserHasRegistered
        {
            get
            {
                if(!_userHasRegisteredLoaded)
                {
                    _userHasRegisteredLoaded = true;
                    if(Storage != null)
                    {
                        try
                        {
                            var attr = Storage.Load(UserHasRegisteredStorageKey);
                            _userHasRegistered = attr != null && attr.AsValue.ToBool();
                        }
                        catch(Exception)
                        {
                            // if no user has registered key
                            // but user id stored, means old version
                            // means the user had registered
                            _userHasRegistered = Storage.Has(UserIdStorageKey);
                            Storage.Save(UserHasRegisteredStorageKey, new AttrBool(_userHasRegistered));
                        }
                    }
                }
                return _userHasRegistered;
            }

            private set
            {
                _userHasRegisteredLoaded = true;
                _userHasRegistered = value;
                if(Storage != null)
                {
                    Storage.Save(UserHasRegisteredStorageKey, new AttrBool(_userHasRegistered));
                }
            }
        }

        public TrackEventDelegate TrackEvent{ private get; set; }

        public string SecurityToken
        {
            get
            {
                if(string.IsNullOrEmpty(_securityToken) && Storage != null)
                {
                    try
                    {
                        Attr attr = Storage.Load(SecurityTokenStorageKey);
                        if(attr.AttrType == AttrType.VALUE)
                        {
                            _securityToken = attr.ToString();
                        }
                    }
                    catch(Exception)
                    {
                    }
                }
                if(string.IsNullOrEmpty(_securityToken))
                {
                    SecurityToken = RandomUtils.GenerateSecurityToken();
                }
                return _securityToken;
            }
            set
            {
                _securityToken = value;
                if(Storage != null)
                {
                    if(_securityToken != null)
                    {
                        Storage.Save(SecurityTokenStorageKey, new AttrString(_securityToken));
                    }
                    else
                    {
                        Storage.Remove(SecurityTokenStorageKey);
                    }
                }
            }
        }

        public string SessionId
        {
            get
            {
                if(User != null)
                {
                    return User.SessionId;
                }
                return null;
            }
        }

        IHttpClient _httpClient;
        List<LinkInfo> _links;
        List<LinkInfo> _pendingLinkConfirms;
        List<User> _users;
        bool _restartLogin;
        UInt64 _userId;
        bool _userHasRegistered;
        bool _userHasRegisteredLoaded;
        string _securityToken;

        public event HttpRequestDelegate HttpRequestEvent = delegate{};
        public event NewUserDelegate NewUserEvent = delegate{};
        public event NewLinkDelegate NewLinkBeforeFriendsEvent = delegate{};
        public event NewLinkDelegate NewLinkAfterFriendsEvent = delegate{};
        public event ConfirmLinkDelegate ConfirmLinkEvent = delegate{};
        public event LoginErrorDelegate ErrorEvent = delegate {};
        public event RestartDelegate RestartEvent = delegate {};

        public SocialPointLogin(IHttpClient client, LoginConfig config)
        {
            Init();
            _httpClient = client;
            _loginConfig = config;
            _availableSecurityTokenErrorRetries = config.SecurityTokenErrors;
            _availableConnectivityErrorRetries = config.ConnectivityErrors;

            if(config.BaseUrl == null)
            {
                config.BaseUrl = string.Empty;
            }
            // Ensure the URL always contains a trailing slash
            _loginConfig.BaseUrl = config.BaseUrl.EndsWith(UriSeparator.ToString()) ?
                                   _loginConfig.BaseUrl : _loginConfig.BaseUrl + UriSeparator;
        }
                
        [System.Diagnostics.Conditional("DEBUG_SPLOGIN")]
        void DebugLog(string msg)
        {
            DebugUtils.Log(string.Format("SocialPointLogin {0}", msg));
        }

        private void Init()
        {
            _userId = 0;
            _userHasRegistered = false;
            _userHasRegisteredLoaded = false;
            Friends = new List<User>();
            User = new LocalUser();
            Timeout = DefaultTimeout;
            ActivityTimeout = DefaultActivityTimeout;
            AutoUpdateFriends = DefaultAutoUpdateFriends;
            AutoUpdateFriendsPhotosSize = DefaultAutoUpdateFriendsPhotoSize;
            UserMappingsBlock = DefaultUserMappingsBlock;
            SecurityToken = string.Empty;
            Language = null;
            _user = new LocalUser();
            _users = new List<User>();
            _links = new List<LinkInfo>();
            _pendingLinkConfirms = new List<LinkInfo>();
            _restartLogin = true;
        }

        public bool IsLogged
        {
            get
            {
                return User.Id != 0;
            }
        }

        public void Dispose()
        {
            ClearUsersCache();
            _links.Clear();
            Friends.Clear();
            _pendingLinkConfirms.Clear();
        }

        void AddLinkInfo(LinkInfo info)
        {
            if(!_links.Contains(info))
            {
                info.Link.AddStateChangeDelegate((LinkState state) => OnLinkStateChanged(info, state));
                DebugUtils.Assert(_links.FirstOrDefault(item => item == info) == null);
                _links.Add(info);
            }
        }

        Error HandleLoginErrors(HttpResponse resp, ErrorType def)
        {
            ErrorType typ = def;
            Error err = null;
            AttrDic data = new AttrDic();
            AttrDic json = null;
            if(resp.HasError)
            {
                try
                {
                    json = new JsonAttrParser().Parse(resp.Body).AsDic;
                }
                catch(Exception)
                {
                }
            }
            if(resp.StatusCode == ForceUpgradeError)
            {
                err = new Error("The game needs to be upgraded.");
                typ = ErrorType.Upgrade;
                LoadGenericData(json);
            }
            else if(resp.StatusCode == InvalidSecurityTokenError)
            {
                err = new Error("The user cannot be recovered.");
                typ = ErrorType.InvalidSecurityToken;
            }
            else if(resp.StatusCode == InvalidPrivilegeTokenError)
            {
                err = new Error("Privilege token is invalid.");
                typ = ErrorType.InvalidPrivilegeToken;
            }
            else if(json != null)
            {
                err = AttrUtils.GetError(json);
            }
            if(Error.IsNullOrEmpty(err) && resp.HasError)
            {
                err = resp.Error;
            }
            if(!Error.IsNullOrEmpty(err))
            {
                data.SetValue(AttrKeyHttpCode, resp.StatusCode);
                NotifyError(typ, err, data);
            }
            return err;
        }

        Error HandleLinkErrors(HttpResponse resp, ErrorType def)
        {
            ErrorType typ = def;
            Error err = null;
            AttrDic data = new AttrDic();

            if(resp.StatusCode == InvalidLinkDataError)
            {
                err = new Error("Link data is invalid.");
                typ = ErrorType.InvalidLinkData;
            }
            else if(resp.StatusCode == InvalidProviderTokenError)
            {
                err = new Error("Provider token is invalid.");
                typ = ErrorType.InvalidProviderToken;
            }
            if(Error.IsNullOrEmpty(err) && resp.HasError)
            {
                try
                {
                    var json = new JsonAttrParser().Parse(resp.Body).AsDic;
                    err = AttrUtils.GetError(json);
                }
                catch(Exception)
                {
                }
            }
            if(Error.IsNullOrEmpty(err) && resp.HasError)
            {
                err = resp.Error;
            }
            if(!Error.IsNullOrEmpty(err))
            {
                data.SetValue(AttrKeyHttpCode, resp.StatusCode);
                NotifyError(typ, err, data);
            }
            return err;
        }

        Error HandleResponseErrors(HttpResponse resp, ErrorType def)
        {
            ErrorType typ = def;
            Error err = null;
            AttrDic data = new AttrDic();
            AttrDic json = null;
            if(resp.HasError)
            {
                try
                {
                    json = new JsonAttrParser().Parse(resp.Body).AsDic;
                }
                catch(Exception)
                {
                }
            }
            if(resp.StatusCode == MaintenanceMode)
            {
                err = new Error("Game is under maintenance.");
                typ = ErrorType.MaintenanceMode;
                LoadGenericData(json);
            }
            else if(resp.StatusCode == InvalidSessionError)
            {
                err = new Error("Session is invalid.");
                typ = ErrorType.InvalidSession;
            }
            else if(resp.HasConnectionError)
            {
                if(resp.StatusCode == (int)HttpResponse.StatusCodeType.TimeOutError)
                {
                    err = new Error("The connection timed out.");
                }
                else
                {
                    err = new Error("The connection could not be established.");
                }
                typ = ErrorType.Connection;
            }
            else if(json != null)
            {
                err = AttrUtils.GetError(json);
            }
            if(Error.IsNullOrEmpty(err) && resp.HasError)
            {
                err = resp.Error;
            }
            if(!Error.IsNullOrEmpty(err))
            {
                data.SetValue(AttrKeyHttpCode, resp.StatusCode);
                NotifyError(typ, err, data);
            }
            return err;
        }

        public string GetUrl(string uri)
        {
            var url = _loginConfig.BaseUrl + BaseUri + UriSeparator + uri.TrimStart(UriSeparator);
            string deviceId = "0";
            if(DeviceInfo != null)
            {
                deviceId = DeviceInfo.Uid;
            }
            url = string.Format(url, UserId.ToString(), deviceId);
            return url;
        }

        void DoLogin(ErrorDelegate cbk)
        {
            _pendingLinkConfirms.Clear();
            if(_availableSecurityTokenErrorRetries < 0)
            {
                var err = new Error("Max amount of login retries reached.");
                NotifyError(ErrorType.LoginMaxRetries, err);
                OnLoginEnd(err, cbk);
            }
            else if(_availableConnectivityErrorRetries < 0)
            {
                var err = new Error("There was an error with the connection.");
                NotifyError(ErrorType.Connection, err);
                OnLoginEnd(err, cbk);
            }
            else
            {
                var req = new HttpRequest(GetUrl(LoginUri), HttpRequest.MethodType.POST);
                SetupLoginHttpRequest(req);
                if(HttpRequestEvent != null)
                {
                    HttpRequestEvent(req);
                }

                DebugLog("login\n----\n" + req.ToString() + "----\n");
                _httpClient.Send(req, (resp) => OnLogin(resp, cbk));
            }
        }

        void OnLogin(HttpResponse resp, ErrorDelegate cbk)
        {
            DebugLog("login\n----\n" + resp.ToString() + "----\n");
            if(resp.StatusCode == InvalidSecurityTokenError && !UserHasRegistered)
            {
                ClearStoredUser();
                _availableSecurityTokenErrorRetries--;
                DoLogin(cbk);
                return;
            }
            else if(resp.HasConnectionError || resp.StatusCode >= HttpResponse.MinServerErrorStatusCode)
            {
                _availableConnectivityErrorRetries--;
                DoLogin(cbk);
                return;
            }

            Data = null;
            Error err = null;
            if(Error.IsNullOrEmpty(err))
            {
                err = HandleLoginErrors(resp, ErrorType.Login);
            }
            if(Error.IsNullOrEmpty(err))
            {
                err = HandleResponseErrors(resp, ErrorType.Login);
            }
            if(Error.IsNullOrEmpty(err))
            {
                err = OnNewLocalUser(resp);
            }
            OnLoginEnd(err, cbk);
            if(Error.IsNullOrEmpty(err))
            {
                NextLinkLogin(null, null, LinkInfo.Filter.Auto);
            }
        }

        void LoadGenericData(AttrDic json)
        {
            if(json != null && json.ContainsKey(AttrKeyGenericData))
            {
                if(Data == null)
                {
                    Data = new GenericData();
                }
                Data.Load(json.Get(AttrKeyGenericData));                
                // update server time
                TimeUtils.Offset = Data.DeltaTime;
            }
        }

        void OnLoginEnd(Error err, ErrorDelegate cbk)
        {
            // Reset retry values
            if(Error.IsNullOrEmpty(err))
            {
                _availableConnectivityErrorRetries = _loginConfig.ConnectivityErrors;
                _availableSecurityTokenErrorRetries = _loginConfig.SecurityTokenErrors;
            }
            else
            {
                _availableConnectivityErrorRetries = Math.Max(_loginConfig.ConnectivityErrors, 0);
                _availableSecurityTokenErrorRetries = Math.Max(_loginConfig.SecurityTokenErrors, 0);

            }


            if(Error.IsNullOrEmpty(err) && AutoUpdateFriends && AutoUpdateFriendsPhotosSize > 0)
            {
                GetUsersPhotos(new List<User>(){ User }, AutoUpdateFriendsPhotosSize, (users, err2) => {
                    if(cbk != null)
                    {
                        cbk(err2);
                    }
                });
            }
            else
            {
                if(cbk != null)
                {
                    cbk(err);
                }
            }
        }

        void OnLinkLogin(LinkInfo info, Error err, ErrorDelegate cbk, LinkInfo.Filter filter)
        {
            DebugUtils.Assert(info != null && _links.FirstOrDefault(item => item == info) != null);
            if(!Error.IsNullOrEmpty(err))
            {
                var data = new AttrDic();
                data.SetValue(AttrKeyLinkProvider, info.Link.Name);
                NotifyError(ErrorType.LinkLogin, err, data);
            }
            NextLinkLogin(info, cbk, filter);
        }

        LinkInfo GetNextLinkInfo(LinkInfo info, LinkInfo.Filter filter)
        {
            if(info != null)
            {
                int linkPos = _links.IndexOf(info);
                if(linkPos != -1)
                {
                    linkPos++;
                    if(_links.Count > linkPos)
                    {
                        return _links.GetRange(linkPos, _links.Count - linkPos).FirstOrDefault(item => item.MatchesFilter(filter));
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else if(_links.Count > 0)
            {
                return _links[0];
            }
            return null;
        }

        void NextLinkLogin(LinkInfo info, ErrorDelegate cbk, LinkInfo.Filter filter)
        {    
            
            info = GetNextLinkInfo(info, filter);
            if(info == null)
            {
                OnLoginEnd(null, cbk);
            }
            else
            {
                DoLinkLogin(info, cbk, filter);
            }
        }

        void DoLinkLogin(LinkInfo info, ErrorDelegate cbk, LinkInfo.Filter filter)
        {
            DebugUtils.Assert(info != null && _links.FirstOrDefault(item => item == info) != null);
            info.Link.Login((err) => OnLinkLogin(info, err, cbk, filter));
        }

        void OnLinkStateChanged(LinkInfo info, LinkState state)
        {
            DebugUtils.Assert(info != null && _links.FirstOrDefault(item => item == info) != null);
            
            if(state == LinkState.Disconnected)
            {
                CleanOldFriends();
            }
            else if(state == LinkState.Connected)
            {
                LocalUser tmpUser = (_user != null) ? new LocalUser(_user) : new LocalUser();
                info.Link.UpdateLocalUser(tmpUser);

                if(tmpUser != null && _user != null && tmpUser.Links.SequenceEqual(_user.Links))
                {
                    UpdateLinkData(info, false);
                }
                else
                {
                    OnNewLink(info, state);
                }
            }
        }

        void OnNewLink(LinkInfo info, LinkState state)
        {
            // the user links have changed, we need to tell the server
            info.LinkData = info.Link.GetLinkData();
            var req = new HttpRequest();
            SetupHttpRequest(req, LinkUri);
            req.AddParam(HttpParamSecurityToken, SecurityToken);
            req.AddParam(HttpParamLinkType, info.Link.Name);
            foreach(var pair in info.LinkData)
            {
                req.AddParam(pair.Key, pair.Value);
            }
            DebugLog("link\n----\n" + req.ToString() + "----\n");
            _httpClient.Send(req, (resp) => OnNewLinkResponse(info, state, resp));
        }

        void OnNewLinkResponse(LinkInfo info, LinkState state, HttpResponse resp)
        {
            if((resp.HasConnectionError || resp.StatusCode >= HttpResponse.MinServerErrorStatusCode) &&
               _availableConnectivityErrorRetries > 0)
            {
                _availableConnectivityErrorRetries--;
                OnNewLink(info, state);
                return;
            }

            DebugUtils.Assert(info != null && _links.FirstOrDefault(item => item == info) != null);
            DebugLog("link\n----\n" + resp.ToString() + "----\n");
            var type = LinkConfirmType.None;
            switch(resp.StatusCode)
            {
            case LinkedToLooseError:
                type = LinkConfirmType.LinkedToLoose;
                break;
            case LinkedToLinkedError:
                type = LinkConfirmType.LinkedToLinked;
                break;
            case LooseToLinkedError:
                type = LinkConfirmType.LooseToLinked;
                break;
            case LinkedToSameError:
                    // duplicated link attempt, do nothing
                resp.StatusCode = (int)HttpResponse.StatusCodeType.Success;
                break;
            default:
                break;
            }

            if(!resp.HasError && type != LinkConfirmType.None)
            {
                Attr data = null;
                try
                {
                    var parser = new JsonAttrParser();
                    data = parser.Parse(resp.Body);
                }
                catch(Exception e)
                {
                    var err = new Error(e.ToString());
                    NotifyError(ErrorType.LinkParse, err, info.LinkData);
                }
                if(data != null)
                {
                    var confirmLinkDic = data.AsDic;
                    var linkToken = confirmLinkDic.GetValue(AttrKeyConfirmLinkToken).AsValue.ToString();
                    info.Token = linkToken;
                    info.ConfirmType = type;
                    NotifyConfirmLink(info, type, linkToken, data);
                }
            }
            else
            {
                var err = HandleLinkErrors(resp, ErrorType.Link);
                if(Error.IsNullOrEmpty(err))
                {
                    err = HandleResponseErrors(resp, ErrorType.Link);
                }
                if(Error.IsNullOrEmpty(err))
                {
                    UpdateLinkData(info, false);
                }
            }
        }

        void NotifyConfirmLink(LinkInfo info, LinkConfirmType type, string linkToken, Attr data)
        {

            bool wait = _pendingLinkConfirms.Count != 0;
            _pendingLinkConfirms.Add(info);
            if(!wait)
            {
                DebugUtils.Assert(info != null && _links.FirstOrDefault(item => item == info) != null);
                if(ConfirmLinkEvent != null)
                {
                    ConfirmLinkEvent(info.Link, type, data, (LinkConfirmDecision decision) => OnConfirmLinkNotifyBack(info, type, linkToken, decision));
                }
                else
                {
                    CancelLink(info);
                }
            }
        }

        void OnConfirmLinkNotifyBack(LinkInfo info, LinkConfirmType type, string linkToken, LinkConfirmDecision decision)
        {
            DebugUtils.Assert(info != null && _links.FirstOrDefault(item => item == info) != null);
            if(info.Token == linkToken)
            {
                ConfirmLink(linkToken, decision, (err) => OnConfirmLinkNotifyBackEnd(info));
            }
        }

        void OnConfirmLinkNotifyBackEnd(LinkInfo info)
        {
            _pendingLinkConfirms.Remove(info);
            if(_pendingLinkConfirms.Count > 0)
            {
                LinkInfo tmpInfo = _pendingLinkConfirms.First();
                _pendingLinkConfirms.Remove(tmpInfo);
                NotifyConfirmLink(tmpInfo, tmpInfo.ConfirmType, tmpInfo.Token, tmpInfo.LinkData);
            }
        }

        void CancelLink(LinkInfo info)
        {
            info.Link.Logout();
        }

        List<UserMapping> LoadUserLinks(Attr data)
        {
            var links = new List<UserMapping>();
            if(data.AttrType == AttrType.LIST)
            {
                var linksAttr = data.AsList;
                foreach(var elm in linksAttr)
                {
                    var link = elm.AsDic;
                    var provider = link.GetValue(AttrKeyLinkProvider).AsValue.ToString();
                    var externalId = link.GetValue(AttrKeyLinkExternalId).AsValue.ToString();
                    links.Add(new UserMapping(externalId, provider));
                }
            }
            else if(data.AttrType == AttrType.DICTIONARY)
            {
                var linksAttr = data.AsDic;
                foreach(var elm in linksAttr)
                {
                    var provider = elm.Key;
                    var externalId = elm.Value.AsValue.ToString();
                    links.Add(new UserMapping(externalId, provider));
                }
            }
            return links;
        }

        User LoadUser(Attr data)
        {
            if(data.AttrType != AttrType.DICTIONARY)
            {
                return null;
            }
            var dataDict = data.AsDic;
            if(dataDict.ContainsKey(AttrKeyUserId))
            {
                UInt64 userId = 0;
                UInt64.TryParse(dataDict.GetValue(AttrKeyUserId).ToString(), out userId);
                if(userId == 0)
                {
                    return null;
                }
                var links = LoadUserLinks(dataDict.Get(AttrKeyLinksData));
                return new User(userId, links);
            }
            return null;
        }

        LocalUser LoadLocalUser(Attr data)
        {
            var user = LoadUser(data);
            if(user == null)
            {
                return null;
            }
            var sessionId = data.AsDic.GetValue(AttrKeySessionId).ToString();
            return new LocalUser(user.Id, sessionId, user.Links);
        }

        Error OnNewLocalUser(HttpResponse resp)
        {    
            AttrDic json = null;
            Error err = null;
            ErrorType errType = ErrorType.UserParse;
            _user = null;
            try
            {
                var parser = new JsonAttrParser();
                json = parser.Parse(resp.Body).AsDic;
            }
            catch(Exception e)
            {
                err = new Error(e.ToString());
            }
            if(Error.IsNullOrEmpty(err) && json != null)
            {
                LoadGenericData(json);                
                var userData = json.Get(AttrKeyLoginData);
                _user = LoadLocalUser(userData);
                if(Data != null && Data.Upgrade != null && Data.Upgrade.Type != UpgradeType.None)
                {
                    // Check for upgrade
                    err = new Error(Data.Upgrade.Message);
                    errType = ErrorType.Upgrade;
                }
            }
            if(Error.IsNullOrEmpty(err) && _user == null)
            {
                err = new Error("Could not load the user.");
            }
            if(Error.IsNullOrEmpty(err))
            {
                var changed = UserId != _user.Id;
                if(changed)
                {
                    UserId = _user.Id;
                }
                UserHasRegistered = true;
                foreach(var linkInfo in _links)
                {
                    linkInfo.Link.OnNewLocalUser(_user);
                }
                var gameData = json.Get(AttrKeyGameData);
                if(NewUserEvent != null)
                {
                    NewUserEvent(gameData, changed);
                }
            }
            else
            {
                var errData = new AttrDic();
                errData.Set(AttrKeyData, json);
                NotifyError(errType, err, errData);
            }
            return err;
        }

        void OnLinkConfirmResponse(string linkToken, LinkInfo info, LinkConfirmDecision decision, HttpResponse resp, ErrorDelegate cbk)
        {
            if((resp.HasConnectionError || resp.StatusCode >= HttpResponse.MinServerErrorStatusCode) &&
                _availableConnectivityErrorRetries > 0 && 
                _loginConfig.EnableOnLinkConfirm)
            {
                _availableConnectivityErrorRetries--;
                ConfirmLink(linkToken, decision, cbk);
                return;
            }

            var err = HandleLinkErrors(resp, ErrorType.Link);
            if(Error.IsNullOrEmpty(err))
            {
                err = HandleResponseErrors(resp, ErrorType.Link); //TODO: check only for link
            }
            bool restartNeeded = false;

            if(info != null)
            {
                // unset link info to prevent multiple confirms
                info.Token = "";
                info.ConfirmType = LinkConfirmType.None;
            }

            if(Error.IsNullOrEmpty(err))
            {
                // for security: don't update user unless decision was change!
                if(decision == LinkConfirmDecision.Change)
                {
                    var parser = new JsonAttrParser();
                    Attr data = null;
                    try
                    {
                        data = parser.Parse(resp.Body);
                        if(data != null)
                        {
                            UInt64 newUserId = 0;
                            UInt64.TryParse(data.ToString(), out newUserId);
                            if(newUserId != 0)
                            {
                                // if confirm returns a new user id we need to relogin
                                if(newUserId != UserId)
                                {
                                    UserId = newUserId;
                                    restartNeeded = true;
                                }
                            }
                            else
                            {
                                ClearStoredUser();
                                restartNeeded = true;
                            }
                        }
                        err = new Error("Confirm response did no contain a valid user id.");
                    }
                    catch(Exception e)
                    {
                        err = new Error(e.ToString());
                    }
                    if(!Error.IsNullOrEmpty(err))
                    {
                        var errData = new AttrDic();
                        errData.Set(AttrKeyData, data);
                        NotifyError(ErrorType.LinkConfirmParse, err, errData);
                    }
                }

                if(info != null)
                {
                    if(decision == LinkConfirmDecision.Cancel)
                    {
                        CancelLink(info);
                    }
                    else
                    {
                        UpdateLinkData(info, restartNeeded);
                    }
                }
            }

            if(cbk != null)
            {
                cbk(err);
            }

            if(restartNeeded)
            {
                info.Pending = true;
                restart();
            }

        }

        void restart()
        {
            RestartEvent();

            if(_restartLogin)
            {
                Login();
            }
        }

        void NotifyNewLink(LinkInfo info, bool beforeFriends)
        {   
            DebugUtils.Assert(info != null && _links.FirstOrDefault(item => item == info) != null);
            if(beforeFriends)
            {
                NewLinkBeforeFriendsEvent(info.Link);
            }
            else
            {
                NewLinkAfterFriendsEvent(info.Link);
            }
        }

        string SignatureSuffix
        {
            get
            {
                string suffix = string.Empty;
                if(UserId != 0)
                {
                    suffix = UserId.ToString("X");
                    if(DeviceInfo != null)
                    {
                        var uid = DeviceInfo.Uid;
                        uid = uid.Substring(0, 8);
                        suffix += SignatureSeparator + uid;
                    }
                }
                else if(DeviceInfo != null)
                {
                    suffix = DeviceInfo.Uid;
                }
                return suffix;
            }
        }

        void NotifyError(ErrorType type, Error err, AttrDic data = null)
        {   
            if(data == null)
            {
                data = new AttrDic();
            }
            // add error signature
            var typeCode = string.Empty + (int)type;
            if(data.ContainsKey(AttrKeyHttpCode))
            {
                typeCode += SignatureCodeSeparator + data.GetValue(AttrKeyHttpCode);
            }
            var signature = typeCode + SignatureSeparator + SignatureSuffix;
            data.SetValue(AttrKeySignature, signature);

            if(TrackEvent != null)
            {
                var evData = new AttrDic();
                var errData = new AttrDic();
                evData.Set(AttrKeyEventError, errData);
                var loginData = new AttrDic();
                errData.Set(AttrKeyEventLogin, loginData);
                loginData.SetValue(AttrKeyEventErrorType, (int)type);
                loginData.SetValue(AttrKeyEventErrorCode, err.Code);
                loginData.SetValue(AttrKeyEventErrorMessage, err.Msg);
                var code = 0;
                if(data.AsDic.ContainsKey(AttrKeyHttpCode))
                {
                    code = data.AsDic.GetValue(AttrKeyHttpCode).ToInt();
                }
                loginData.SetValue(AttrKeyEventErrorHttpCode, code);
                loginData.Set(AttrKeyEventErrorData, data);
                TrackEvent(EventNameError, evData);
            }
            if(ErrorEvent != null)
            {
                ErrorEvent(type, err, data);
            }
        }

        void OnAppRequestResponse(HttpResponse resp, AppRequest req, ErrorDelegate cbk)
        {
            DebugLog("app req\n----\n" + resp.ToString() + "---\n");
            var err = HandleResponseErrors(resp, ErrorType.AppRequest);
            if(Error.IsNullOrEmpty(err))
            {
                OnAppRequestLinkNotified(null, req, null, cbk);
                return;
            }
            OnAppRequestEnd(req, err, cbk);
        }

        void OnAppRequestLinkNotified(LinkInfo info, AppRequest req, Error err, ErrorDelegate cbk)
        {
            DebugLog("app req\n----\n" + req.ToString() + "---\n");
            if(Error.IsNullOrEmpty(err))
            {
                info = GetNextLinkInfo(info, LinkInfo.Filter.All);
                if(info != null)
                {
                    info.Link.NotifyAppRequestRecipients(req, (err2) => OnAppRequestLinkNotified(info, req, err2, cbk));
                    return;
                }
            }
            OnAppRequestEnd(req, err, cbk);
        }

        void OnAppRequestEnd(AppRequest req, Error err, ErrorDelegate cbk)
        {
            if(cbk != null)
            {
                cbk(err);
            }
        }

        void UpdateLinkData(LinkInfo info, bool disableUpdatingFriends)
        {
            DebugUtils.Assert(info != null && _links.FirstOrDefault(item => item == info) != null);
            info.Link.UpdateLocalUser(_user);

            NotifyNewLink(info, true);

            if(!disableUpdatingFriends && AutoUpdateFriends)
            {
                var data = new List<UserMapping>();
                info.Link.GetFriendsData(data);
                UpdateFriends(data, (users, err) => NotifyNewLink(info, false));
            }
            else
            {
                NotifyNewLink(info, false);
            }
        }

        void OnUpdateFriendsResponse(HttpResponse resp, List<UserMapping> mappings, uint block, UsersDelegate cbk)
        {
            if(block > 0)
            {
                var err = HandleResponseErrors(resp, ErrorType.Friends);
                if(!Error.IsNullOrEmpty(err))
                {
                    OnUpdateFriendsEnd(mappings, err, cbk);
                    return;
                }
                var tmpFriends = Friends;
                err = ParseUsersResponse(resp, tmpFriends);
                if(!Error.IsNullOrEmpty(err))
                {
                    OnUpdateFriendsEnd(mappings, err, cbk);
                    return;
                }
                Friends = tmpFriends;
            }
            
            var req = new HttpRequest();
            SetupHttpRequest(req, UserMappingUri);
            if(SetupUserMappingsHttpRequest(req, mappings, block))
            {
                _httpClient.Send(req, (resp2) => OnUpdateFriendsResponse(resp2, mappings, block + 1, cbk));
            }
            else
            {
                OnUpdateFriendsEnd(mappings, null, cbk);
            }
        }

        void OnUpdateFriendsEnd(List<UserMapping> mappings, Error err, UsersDelegate cbk)
        {
            var friendsSelection = Friends.Where(u => (mappings.Where(map => u.HasLink(map.Id)).Count() > 0)).ToList();

            if(AutoUpdateFriendsPhotosSize > 0 && friendsSelection.Count > 0)
            {
                GetUsersPhotos(friendsSelection, AutoUpdateFriendsPhotosSize, cbk);
            }
            else if(cbk != null)
            {
                UpdateUsersCache(friendsSelection);
                cbk(Friends, err);
            }
        }
        
        void OnUserPhotoLink(LinkInfo info, User user, List<User> users, uint photoSize, Error err, UsersDelegate cbk)
        {
            if(user == null)
            {
                if(users.Count > 0)
                {
                    OnUserPhotoLink(info, users.First(), users, photoSize, err, cbk);
                }
                else
                {
                    OnUsersPhotosEnd(users, err, cbk);
                }
            }
            else
            {
                // try to get user from cache
                if(user.AppInstalled)
                {
                    GetCachedUserById(user.Id, user);
                }
                info = GetNextLinkInfo(info, LinkInfo.Filter.All);
                if(info != null)
                {
                    info.Link.UpdateUserPhoto(user, photoSize, (err2) => OnUserPhotoLink(info, user, users, photoSize, err2, cbk));
                }
                else
                {
                    int userPos = users.IndexOf(user);
                    if(userPos != -1)
                    {
                        userPos++;
                        if(users.Count > userPos)
                        {
                            user = users[userPos];
                        }
                        else
                        {
                            user = null;
                        }
                    }
                    else
                    {
                        user = null;
                    }

                    if(user != null)
                    { 
                        OnUserPhotoLink(null, user, users, photoSize, err, cbk);
                    }
                    else
                    {
                        OnUsersPhotosEnd(users, err, cbk);
                    }
                }
            }
        }

        void OnUsersPhotosEnd(List<User> users, Error err, UsersDelegate cbk)
        {
            UpdateUsersCache(users);
            OnUsersEnd(users, err, cbk);
        }

        void OnUsersEnd(List<User> users, Error err, UsersDelegate cbk)
        {
            if(cbk != null)
            {
                cbk(users, err);
            }
        }
        
        void OnGetUsersByIdResponse(HttpResponse resp, List<UserMapping> mappings, uint block, uint photoSize, List<User> users, UsersDelegate cbk)
        {
            var err = resp.Error;
            if(Error.IsNullOrEmpty(err) && block > 0)
            {
                err = HandleResponseErrors(resp, ErrorType.Users);
            }
            if(Error.IsNullOrEmpty(err) && block > 0)
            {
                err = ParseUsersResponse(resp, users);
            }
            if(Error.IsNullOrEmpty(err))
            {
                var req = new HttpRequest();
                SetupHttpRequest(req, UserMappingUri);
                if(SetupUserMappingsHttpRequest(req, mappings, block))
                {
                    req.AddQueryParam(HttpParamSessionId, User.SessionId);
                    _httpClient.Send(req, (resp2) => OnGetUsersByIdResponse(resp2, mappings, block + 1, photoSize, users, cbk));
                    return;
                }
            }
            OnGetUsersByIdEnd(photoSize, users, err, cbk);
        }

        void OnGetUsersByIdEnd(uint photoSize, List<User> users, Error err, UsersDelegate cbk)
        {
            UpdateUsersCache(users);
            if(Error.IsNullOrEmpty(err) && photoSize > 0)
            {
                GetUsersPhotos(users, photoSize, (u, err2) => OnUsersEnd(u, err2, cbk));
            }
            else
            {
                OnUsersEnd(users, err, cbk);
            }
        }

        Error ParseUsersResponse(HttpResponse resp, List<User> users)
        {
            if(resp.Body.Length > 0)
            {
                var data = new AttrList();
                try
                {
                    var parser = new JsonAttrParser();
                    data = parser.Parse(resp.Body).AsList;
                }
                catch(Exception e)
                {
                    NotifyError(ErrorType.UsersParse, new Error(e.ToString()));
                    return new Error(e.ToString());
                }

                foreach(var elm in data)
                {
                    var friendDict = elm.AsDic;
                    var tmpUser = LoadUser(friendDict);

                    foreach(var linkInfo in _links)
                    {
                        linkInfo.Link.UpdateUser(tmpUser);
                    }

                    users.RemoveAll(u => u == tmpUser);
                    users.Add(tmpUser);
                }
            }
            return null;
        }
        
        bool GetCachedUserById(UInt64 userId, User user)
        {
            if(User.Id == userId)
            {
                user.Combine(User);
                return true;
            }

            User resultUser = _users.FirstOrDefault(item => item.Id == userId);

            if(resultUser != null)
            {
                user.Combine(resultUser);
            }

            return (resultUser != null);
        }

        bool GetCachedUserByTempId(string tempId, User user)
        {
            if(User.TempId == tempId)
            {
                user.Combine(User);
                return true;
            }
            
            User resultUser = _users.FirstOrDefault(item => item.TempId == tempId);

            if(resultUser != null)
            {
                user.Combine(resultUser);
            }
            
            return (resultUser != null);
        }

        void UpdateUsersCache(List<User> tmpUsers)
        {
            foreach(var user in tmpUsers)
            {
                if(User == user)
                {
                    User.Combine(user);
                }

                User resultFriend = Friends.FirstOrDefault(item => item == user);
                if(resultFriend != null)
                {
                    resultFriend.Combine(user);
                }

                User resultUser = _users.FirstOrDefault(item => item == user);
                if(resultUser != null)
                {
                    resultUser.Combine(user);
                }
                else
                {
                    _users.Add(user);
                }
            }
        }

        void CleanOldFriends()
        {
            Friends.RemoveAll(u => _links.FirstOrDefault(lInfo => lInfo.Link.IsFriend(u)) == null);
        }

        void StoreUserId()
        {
            if(Storage != null)
            {
                Storage.Save(UserIdStorageKey, new AttrString(UserId.ToString()));
            }
        }

        bool SetupUserMappingsHttpRequest(HttpRequest req, List<UserMapping> mappings, uint block)
        {
            if(UserMappingsBlock == 0 && block > 0)
            {
                return false;
            }

            int start = (int)(block * UserMappingsBlock);

            if(mappings.Count <= start)
            {
                return false;
            }

            var param = new AttrDic();

            int max = UserMappingsBlock == 0 ? mappings.Count : (int)(start + UserMappingsBlock);
            if(max > mappings.Count)
            {
                max = mappings.Count;
            }

            foreach(var um in mappings)
            {
                if(!param.ContainsKey(um.Provider))
                {
                    param.Set(um.Provider, new AttrList());
                }
                param.Get(um.Provider).AsList.AddValue(um.Id);
            }
            var serializer = new JsonAttrSerializer();
            req.AddParam(HttpParamUserIds, serializer.SerializeString(param));

            return true;
        }

        void SetupLoginHttpRequest(HttpRequest req)
        {
            req.AddHeader(HttpRequest.AcceptHeader, HttpRequest.ContentTypeJson);
            req.AcceptCompressed = true;
            if(req.Timeout == 0)
            {
                req.Timeout = Timeout;
            }
            if(req.ActivityTimeout == 0)
            {
                req.ActivityTimeout = ActivityTimeout;
            }
            
            string clientToken = SecurityToken;
            if(!req.HasParam(HttpParamSecurityToken) && !string.IsNullOrEmpty(clientToken))
            {
                req.AddParam(HttpParamSecurityToken, clientToken);
            }
            if(!req.HasParam(HttpParamTimestamp))
            {
                req.AddParam(HttpParamTimestamp, TimeUtils.GetTimestamp(DateTime.UtcNow).ToString());
            }
            if(DeviceInfo != null)
            {
                if(!req.HasParam(HttpParamDeviceModel))
                {
                    req.AddParam(HttpParamDeviceModel, DeviceInfo.String);
                }
                if(!req.HasParam(HttpParamClientVersion))
                {
                    req.AddParam(HttpParamClientVersion, DeviceInfo.AppInfo.ShortVersion);
                }
                if(!req.HasParam(HttpParamPlatform))
                {
                    req.AddParam(HttpParamPlatform, DeviceInfo.Platform);
                }
                if(!req.HasParam(HttpParamClientLanguage))
                {
                    req.AddParam(HttpParamClientLanguage, Language);
                }
                if(!req.HasParam(HttpParamDeviceLanguage))
                {
                    req.AddParam(HttpParamDeviceLanguage, DeviceInfo.AppInfo.Language);
                }
                if(!req.HasParam(HttpParamClientBuild))
                {
                    req.AddParam(HttpParamClientBuild, DeviceInfo.AppInfo.Version);
                }
                if(!req.HasParam(HttpParamClientAppId))
                {
                    req.AddParam(HttpParamClientAppId, DeviceInfo.AppInfo.Id);
                }
                if(!req.HasParam(HttpParamPlatformVersion))
                {
                    req.AddParam(HttpParamPlatformVersion, DeviceInfo.PlatformVersion);
                }
                if(!req.HasParam(HttpParamDeviceAid))
                {
                    req.AddParam(HttpParamDeviceAid, DeviceInfo.AdvertisingId);
                }
                if(!req.HasParam(HttpParamDeviceAidEnabled))
                {
                    req.AddParam(HttpParamDeviceAidEnabled, DeviceInfo.AdvertisingIdEnabled ? "1" : "0");
                }
                if(!req.HasParam(HttpParamDeviceRooted))
                {
                    req.AddParam(HttpParamDeviceRooted, DeviceInfo.Rooted ? "1" : "0");
                }
                if(!req.HasParam(HttpParamPrivilegeToken) && !string.IsNullOrEmpty(PrivilegeToken))
                {
                    req.AddParam(HttpParamPrivilegeToken, PrivilegeToken);
                }
            }
        }

        // PUBLIC

        /**
         * Do the main social point login
         *
         * @param callback to call when the login is finished
         * @param which links to also login, by default will login all auto links
         */
        public void Login(ErrorDelegate cbk = null)
        {
            if(TrackEvent != null)
            {
                TrackEvent(EventNameLoading, new AttrDic());
            }
            DoLogin(cbk);
        }

        /**
         * Do the login of the links without autologin
         */
        public void LoginLinks(ErrorDelegate cbk = null)
        {
            NextLinkLogin(null, cbk, LinkInfo.Filter.Normal);
        }

        /**
         * Login a single link
         */
        public void LoginLink(ILink link, ErrorDelegate cbk = null)
        {
            LinkInfo resultLinkInfo = _links.FirstOrDefault(item => item.Link == link);
            DebugUtils.Assert(resultLinkInfo != null);
            DoLinkLogin(resultLinkInfo, cbk, LinkInfo.Filter.None);
        }

        /**
         * Add a new link
         * @param link the link object (will be deleted by SocialPointLogin)
         * @param the link mode defines when the link is logged in:
         *   - Auto: link is logged in automatically after the social point main login
         *   - Normal: link is logged in when calling SocialPointLogin.LoginLinks();
         *   - Manual: link is logged only if SocialPointLogin.LoginLink() is called manually
         */
        public void AddLink(ILink link, LinkMode mode = LinkMode.Auto)
        {
            AddLinkInfo(new LinkInfo(link, mode));
        }

        /**
         * Remove a link
         * @return true if the link was found
         */
        public bool RemoveLink(ILink link)
        {
            LinkInfo linkInfo = _links.FirstOrDefault(item => item.Link == link);
            if(linkInfo != null)
            {
                return _links.Remove(linkInfo);
            }
            return false;
        }

        /**
         * Confirm a link
         * @param the link token that the server sent when trying to link
         * @param result true if the link is confirmed, false if cancelled
         * @param callback called when the link is finished
         */
        public void ConfirmLink(string linkToken, LinkConfirmDecision decision, ErrorDelegate cbk = null)
        {
            LinkInfo linkInfo = _links.FirstOrDefault(item => item.Token == linkToken);
            var req = new HttpRequest();
            SetupHttpRequest(req, LinkConfirmUri);
            req.AddParam(HttpParamSecurityToken, SecurityToken);
            req.AddParam(HttpParamLinkConfirmToken, linkToken);
            req.AddParam(HttpParamLinkDecision, decision.ToString().ToLower());

            DebugLog("link confirm\n----\n" + req.ToString() + "----\n");
            _httpClient.Send(req, (HttpResponse resp) => OnLinkConfirmResponse(linkToken, linkInfo, decision, resp, cbk));
        }

        /**
         * Clear all the user information
         */
        public void ClearStoredUser()
        {
            _userId = 0;
            _userHasRegistered = false;
            _userHasRegisteredLoaded = true;
            if(Storage != null)
            {
                Storage.Remove(UserIdStorageKey);
                Storage.Remove(UserHasRegisteredStorageKey);
            }
        }

        /**
         * Clear the current user
         * Next login will always call the new user callbacks
         */
        public void ClearUser()
        {
            User = new LocalUser();
        }

        /**
         * Clear the user cache
         * This cache is used to prevent doing http requests for users multiple times
         */
        public void ClearUsersCache()
        {
            _users.Clear();
        }

        /**
         * Find friends by id
         */
        public void GetFriendsByTempId(List<string> userIds, List<User> users)
        {
            foreach(var friend in Friends)
            {
                string tmp = userIds.FirstOrDefault(tmpId => tmpId == friend.TempId);
                if(!string.IsNullOrEmpty(tmp))
                {
                    users.Add(friend);
                }
            }
        }

        /**
         * Minimun setup component http requests
         */ 
        public void SetupHttpRequest(HttpRequest req, string Uri)
        {
            if(req.Timeout == 0)
            {
                req.Timeout = Timeout;
            }
            if(req.ActivityTimeout == 0)
            {
                req.ActivityTimeout = ActivityTimeout;
            }
            req.Method = HttpRequest.MethodType.POST;
            req.Url = new Uri(GetUrl(Uri));
            if(User != null && !string.IsNullOrEmpty(User.SessionId))
            {
                req.AddQueryParam(HttpParamSessionId, User.SessionId);
            }
        }

        /**
         * Will update the local user friends list
         */
        public void UpdateFriends(UsersDelegate cbk = null)
        {
            var mappings = new List<UserMapping>();
            foreach(var linkInfo in _links)
            {
                linkInfo.Link.GetFriendsData(mappings);
            }
            UpdateFriends(mappings, cbk);
        }

        public void UpdateFriends(List<UserMapping> mappings, UsersDelegate cbk = null)
        {
            if(mappings.Count > 0)
            {
                HttpResponse resp = new HttpResponse();
                OnUpdateFriendsResponse(resp, mappings, 0, cbk);
            }
            else if(cbk != null)
            {
                cbk(Friends, null);
            }
        }

        /**
         * Will update the photos of a sublist of friends
         */
        public void GetUsersPhotos(List<User> users, uint photoSize, UsersDelegate cbk = null)
        {
            if(users.Count == 0)
            {
                OnUsersPhotosEnd(users, null, cbk);
            }
            else
            {
                OnUserPhotoLink(null, null, users, photoSize, null, cbk);
            }
        }

        public void GetUsersPhotosById(List<UInt64> userIds, uint photoSize, UsersDelegate cbk = null)
        {
            GetUsersById(userIds, photoSize, cbk);
        }

        public void GetUsersPhotosByTempId(List<string> userIds, uint photoSize, UsersDelegate cbk = null)
        {
            var users = new List<User>();

            foreach(var userId in userIds)
            {
                User u = new User();
                if(GetCachedUserByTempId(userId, u))
                {
                    users.Add(u);
                }
            }

            GetUsersPhotos(users, photoSize, cbk);
        }

        /**
         * Will get a list of users by id
         * The users returned by the callback will be deleted just after it, so you need to copy them!
         */
        public void GetUsersById(List<UInt64> userIds, UsersDelegate cbk = null)
        {
            GetUsersById(userIds, 0, cbk);
        }

        public void GetUsersById(List<UInt64> userIds, uint photoSize, UsersDelegate cbk = null)
        {
            var users = new List<User>();
            var mappings = new List<UserMapping>();

            foreach(var userId in userIds)
            {
                var cacheUser = new User();
                if(GetCachedUserById(userId, cacheUser))
                {
                    users.Add(cacheUser);
                }
                else
                {
                    mappings.Add(new UserMapping(userId.ToString(), HttpParamSocialPointUserIds));
                }
            }

            var resp = new HttpResponse();
            OnGetUsersByIdResponse(resp, mappings, 0, photoSize, users, cbk);
        }

        /**
         * Get cached users. The users written in the list are not copied
         * @param the ids to search
         * @param the list to write
         */
        public void GetCachedUsersById(List<UInt64> userIds, List<User> users)
        {
            foreach(var userId in userIds)
            {
                var u = new User();
                if(GetCachedUserById(userId, u))
                {
                    users.Add(u);
                }
            }
        }

        public void GetCachedUsersByTempId(List<string> userIds, List<User> users)
        {
            foreach(var userId in userIds)
            {
                var u = new User();
                if(GetCachedUserByTempId(userId, u))
                {
                    users.Add(u);
                }
            }
        }

        /**
         * Send an app request
         * @param the request to send (will be deleted when finished)
         * @param callback when the request was sent
         */
        public void SendAppRequest(AppRequest req, ErrorDelegate cbk = null)
        {
            var httpReq = new HttpRequest();
            SetupHttpRequest(httpReq, AppRequestsUri);

            var appRequestParams = new AttrDic();
            appRequestParams.Set(HttpParamAppRequestType, new AttrString(req.Type));

            var toParam = new AttrDic();

            foreach(var user in req.Recipients)
            {
                var mapping = user.AppRequestRecipient;
                if(mapping.Id != null)
                {
                    if(mapping.Provider == null)
                    {
                        mapping.Provider = HttpParamSocialPointUserIds;
                    }
                    if(!toParam.ContainsKey(mapping.Provider))
                    {
                        toParam.Set(mapping.Provider, new AttrList());
                    }
                    toParam.Get(mapping.Provider).AsList.AddValue(mapping.Id);
                }
            }
            appRequestParams.Set(HttpParamAppRequestUserIds, toParam);

            httpReq.Body = new JsonAttrSerializer().Serialize(appRequestParams);

            DebugLog("app req\n----\n" + httpReq.ToString() + "----\n");
            _httpClient.Send(httpReq, (resp) => OnAppRequestResponse(resp, req, cbk));
        }

        public void GetReceivedAppRequests(AppRequestDelegate cbk = null)
        {
            var httpReq = new HttpRequest();
            SetupHttpRequest(httpReq, AppRequestsUri);
            httpReq.Method = HttpRequest.MethodType.GET;
            _httpClient.Send(httpReq, (resp) => OnReceivedAppRequestResponse(resp, cbk));
        }

        public void OnReceivedAppRequestResponse(HttpResponse resp, AppRequestDelegate cbk)
        {
            var err = HandleResponseErrors(resp, ErrorType.ReceiveAppRequests);
            var reqs = new List<AppRequest>();
            if(Error.IsNullOrEmpty(err))
            {
                var parser = new JsonAttrParser();
                Attr data = null;
                try
                {
                    data = parser.Parse(resp.Body);
                    if(data != null)
                    {
                        if(data.AttrType == AttrType.DICTIONARY)
                        {
                            var receivedAppRequest = data.AsDic;
                            foreach(var elm in receivedAppRequest)
                            {
                                AttrDic requestData = elm.Value.AsDic;
                                requestData["id"] = new AttrString(elm.Key);
                                AppRequest req = new AppRequest(requestData["type"].AsValue.ToString(), requestData);
                                reqs.Add(req);
                            }
                        }
                        else if(data.AttrType == AttrType.LIST)
                        {
                            var receivedAppRequest = data.AsList;
                            foreach(var elm in receivedAppRequest)
                            {
                                var requestData = elm.AsDic;
                                var type = requestData.GetValue("type").AsValue.ToString();
                                reqs.Add(new AppRequest(type, requestData));
                            }
                        }

                    }
                }
                catch(Exception e)
                {
                    err = new Error(e.ToString());
                }
                if(!Error.IsNullOrEmpty(err))
                {
                    var errData = new AttrDic();
                    errData.Set(AttrKeyData, data);
                    NotifyError(ErrorType.ReceiveAppRequests, err, errData);
                }
            }

            if(cbk != null)
            {
                cbk(reqs, err);
            }
        }

        public void DeleteAppRequest(List<string> ids, ErrorDelegate cbk)
        {
            var req = new HttpRequest();
            SetupHttpRequest(req, AppRequestsUri);
            req.Method = HttpRequest.MethodType.DELETE;
            req.AddQueryParam(HttpParamRequestIds, String.Join(",", ids.ToArray()));
            _httpClient.Send(req, (resp) => OnDeleteAppRequestResponse(resp, cbk));
        }

        public void OnDeleteAppRequestResponse(HttpResponse resp, ErrorDelegate cbk)
        {
            var err = HandleResponseErrors(resp, ErrorType.ReceiveAppRequests);
            if(cbk != null)
            {
                cbk(err);
            }
        }
    }

    public class LinkInfo
    {        
        public enum Filter
        {
            Auto,
            Normal,
            All,
            None
        }

        public ILink Link { get; private set; }

        public LinkMode Mode { get; private set; }

        public string Token { get; set; }

        public LinkConfirmType ConfirmType { get; set; }

        public AttrDic LinkData { get; set; }

        public bool Pending { get; set; }

        private LinkInfo(LinkInfo other)
        {
            Link = other.Link;
            Mode = other.Mode;
            Token = other.Token;
            LinkData = new AttrDic(other.LinkData);
        }

        public LinkInfo(ILink link, LinkMode mode)
        {
            Link = link;
            Mode = mode;
        }

        public bool MatchesFilter(Filter filter)
        {
            bool result = false;

            switch(Mode)
            {
            case LinkMode.Auto:
                result = filter != Filter.None && filter != Filter.Normal;
                break;
            case LinkMode.Normal:
                result = filter != Filter.None && filter != Filter.Auto;
                break;
            case LinkMode.Manual:
                result = filter == Filter.All;
                break;
            }

            return result;
        }
    }
}
