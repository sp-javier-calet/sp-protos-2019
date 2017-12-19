using System;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Hardware;
using SocialPoint.Locale;
using SocialPoint.Network;
using SocialPoint.Utils;
using SocialPoint.Restart;

namespace SocialPoint.Login
{
    public delegate void TrackEventDelegate(string eventName, AttrDic data = null, ErrorDelegate del = null);

    public sealed class SocialPointLogin : ILogin
    {
        const string DefaultBaseUrl = "http://localhost/";
        const string BaseUri = "{0}/{1}";
        // UserId, DeviceId
        const string LoginUri = "user/login";
        const string LinkUri = "user/link";
        const string LinkConfirmUri = "user/link/confirm";
        const string UserMappingUri = "user/link/mapping";
        const string AppRequestsUri = "requests";

        const string SecurityTokenStorageKey = "SocialPointLoginClientToken";
        const string UserIdStorageKey = "SocialPointLoginUserId";
        const string UserHasRegisteredStorageKey = "SocialPointLoginHasRegistered";

        const string HttpParamSessionId = "session_id";
        const string HttpParamDeviceModel = "device_model";
        const string HttpParamSecurityToken = "security_token";
        const string HttpParamClientVersion = "client_version";
        const string HttpParamPlatform = "platform";
        const string HttpParamClientLanguage = "client_language";
        const string HttpParamDeviceLanguage = "device_language";
        const string HttpParamUserIds = "ids";
        const string HttpParamSocialPointUserIds = "sp";
        const string HttpParamAppRequestUserIds = "to";
        const string HttpParamAppRequestType = "type";
        const string HttpParamTimestamp = "ts";
        const string HttpParamPlatformVersion = "device_os";
        const string HttpParamDeviceAid = "device_adid";
        const string HttpParamDeviceAidEnabled = "device_adid_enabled";
        const string HttpParamDeviceRooted = "device_rooted";
        const string HttpParamClientBuild = "client_build";
        const string HttpParamClientAppId = "client_appid";
        const string HttpParamLinkConfirmToken = "confirm_link_token";
        const string HttpParamLinkDecision = "decision";
        const string HttpParamLinkType = "provider_type";
        const string HttpParamRequestIds = "request_ids";
        const string HttpParamPrivilegeToken = "privileged_session_token";
        const string HttpParamLinkChange = "link_change";
        const string HttpParamLinkChangeCode = "link_change_code";

        const string HttpParamForcedErrorCode = "fake_error_code";
        const string HttpParamForcedErrorType = "fake_error_type";

        const string HttpParamDeviceTotalMemory = "device_total_memory";
        const string HttpParamDeviceUsedMemory = "device_used_memory";
        const string HttpParamDeviceTotalStorage = "device_total_storage";
        const string HttpParamDeviceUsedStorage = "device_used_storage";
        const string HttpParamDeviceMaxTextureSize = "device_max_texture_size";
        const string HttpParamDeviceScreenWidth = "device_screen_width";
        const string HttpParamDeviceScreenHeight = "device_screen_height";
        const string HttpParamDeviceScreenDpi = "device_screen_dpi";
        const string HttpParamDeviceCpuCores = "device_cpu_cores";
        const string HttpParamDeviceCpuFreq = "device_cpu_freq";
        const string HttpParamDeviceCpuModel = "device_cpu_model";
        const string HttpParamDeviceOpenglVendor = "device_opengl_vendor";
        const string HttpParamDeviceOpenglRenderer = "device_opengl_renderer";
        const string HttpParamDeviceOpenglShading = "device_opengl_shading";
        const string HttpParamDeviceOpenglVersion = "device_opengl_version";
        const string HttpParamDeviceOpenglMemory = "device_opengl_memory";


        const string AttrKeySessionId = "session_id";
        const string AttrKeyLinksData = "linked_accounts";
        const string AttrKeyUserId = "user_id";
        const string AttrKeyLinkProvider = "provider_type";
        const string AttrKeyLinkExternalId = "external_id";
        const string AttrKeyConfirmLinkToken = "confirm_link_token";
        const string AttrKeyLoginData = "login_data";
        const string AttrKeyGameData = "game_data";
        const string AttrKeyGenericData = "generic_data";
        public const string AttrKeyData = "data";
        const string AttrKeyEventError = "error";
        const string AttrKeyEventLogin = "login";
        const string AttrKeyEventErrorType = "error_type";
        const string AttrKeyEventErrorCode = "error_code";
        const string AttrKeyEventErrorMessage = "error_desc";
        const string AttrKeyEventErrorHttpCode = "http_code";
        const string AttrKeyEventErrorData = "data";
        public const string AttrKeyHttpCode = "http_code";
        public const string AttrKeySignature = "signature";
        const string AttrKeyHttpDuration = "duration";
        const string AttrKeyHttpTransferDuration = "transfer_duration";
        const string AttrKeyHttpConnectionDuration = "connection_duration";
        const string AttrKeyHttpDownloadSize = "download_size";
        const string AttrKeyHttpDownloadSpeed = "download_speed";

        const string EventNameLoading = "game.loading";
        const string EventNameLogin = "game.login";
        const string EventNameLoginError = "errors.login_error";
        const string EventNameLinkError = "errors.link_error";

        const long TrackErrorMinElapsedTime = 60;

        const string SignatureSeparator = ":";
        const string SignatureCodeSeparator = "-";

        const int InvalidSecurityTokenError = 480;
        const int InvalidSessionError = 482;
        const int InvalidLinkDataError = 483;
        const int InvalidProviderTokenError = 484;
        const int InvalidPrivilegeTokenError = 486;
        const int MaintenanceMode = 503;
        const int LooseToLinkedError = 264;
        const int LinkedToLooseError = 265;
        const int LinkedToSameError = 266;
        const int LinkedToLinkedError = 267;
        const int ForceUpgradeError = 285;
        const int RootedDeviceError = 479;

        public const int DefaultMaxSecurityTokenErrorRetries = 5;
        public const int DefaultMaxConnectivityErrorRetries = 0;
        public const bool DefaultEnableLinkConfirmRetries = false;
        public const float DefaultTimeout = 120.0f;
        public const float DefaultActivityTimeout = 15.0f;
        public const bool DefaultAutoUpdateFriends = true;
        public const uint DefaultAutoUpdateFriendsPhotoSize = 0;
        public const uint DefaultUserMappingsBlock = 50;

        public const string LoadExternalSourceHost = "load-external-user";
        public const string SourceParamEnvironment = "envurl";
        public const string SourceParamPrivilegeToken = "privilegedToken";
        public const string SourceParamUserId = "userId";

        public struct LoginConfig
        {
            public string BaseUrl;
            public int SecurityTokenErrors;
            public int ConnectivityErrors;
            public bool EnableOnLinkConfirm;
        }

        LoginConfig _loginConfig;
        string _baseUrl;
        int _availableSecurityTokenErrorRetries;
        int _availableConnectivityErrorRetries;
        IHttpClient _httpClient;
        IAppEvents _appEvents;
        List<LinkInfo> _links;
        List<LinkInfo> _pendingLinkConfirms;
        List<User> _users;
        bool _restartLogin;
        UInt64 _userId;
        bool _userHasRegistered;
        bool _userHasRegisteredLoaded;
        string _securityToken;
        bool _linkChange;
        int _linkChangeCode;

        long _lastTrackedErrorTimestamp;
        int _lastTrackedErrorCode;

        string _forcedErrorCode = null;
        string _forcedErrorType = null;

        public event HttpRequestDelegate HttpRequestEvent = null;
        public event NewUserDelegate NewUserEvent = null;
        public event NewUserStreamDelegate NewUserStreamEvent = null;
        public event NewUserChangeDelegate NewUserChangeEvent = null;
        public event NewGenericDataDelegate NewGenericDataEvent = null;
        public event NewLinkDelegate NewLinkBeforeFriendsEvent = null;
        public event NewLinkDelegate NewLinkAfterFriendsEvent = null;
        public event ConfirmLinkDelegate ConfirmLinkEvent = null;
        public event LoginErrorDelegate ErrorEvent = null;
        public event LoginErrorDelegate LinkErrorEvent = null;
        public event RestartDelegate RestartEvent = null;

        public LocalUser User
        {
            get;
            private set;
        }

        public IRestarter Restarter { get; set; }

        public IAppEvents AppEvents
        {
            get
            {
                return _appEvents;
            }

            set
            {
                if(_appEvents != null)
                {
                    _appEvents.OpenedFromSource -= OnAppOpenedFromSource;
                }
                _appEvents = value;
                if(_appEvents != null)
                {
                    _appEvents.OpenedFromSource += OnAppOpenedFromSource;
                }
            }
        }

        void OnAppOpenedFromSource(AppSource src)
        {
            #if DEBUG
            if(SetAppSource(src))
            {
                _appEvents.RestartGame();
            }
            #endif
        }

        bool SetAppSource(AppSource src)
        {
            if(src == null || src.Empty || src.Host != LoadExternalSourceHost)
            {
                return false;
            }
            var parms = src.Parameters;
            string val;
            bool changed = false;
            if(parms.TryGetValue(SourceParamEnvironment, out val))
            {
                if(BaseUrl != val)
                {
                    changed = true;
                    SetBaseUrl(val);
                }
            }
            if(parms.TryGetValue(SourceParamPrivilegeToken, out val))
            {
                if(PrivilegeToken != val)
                {
                    changed = true;
                    PrivilegeToken = val;
                }
            }
            if(parms.TryGetValue(SourceParamUserId, out val))
            {
                ulong userId;
                if(ulong.TryParse(val, out userId))
                {
                    if(UserId != userId)
                    {
                        changed = true;
                        ImpersonatedUserId = userId;
                    }
                }
            }
            return changed;
        }

        public List<User> Friends { get; private set; }

        public IDeviceInfo DeviceInfo { private get; set; }

        public IAttrStorage Storage { get; set; }

        public ILocalizationManager Localization { get; set; }

        public float Timeout { get; set; }

        public float ActivityTimeout { get; set; }

        public bool AutoUpdateFriends { get; set; }

        public uint AutoUpdateFriendsPhotosSize { get; set; }

        public uint UserMappingsBlock { get; set; }


        public GenericData Data { get; private set; }

        public string PrivilegeToken { get; set; }

        public UInt64 UserId
        {
            get
            {
                if(ImpersonatedUserId != 0)
                {
                    return ImpersonatedUserId;
                }
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

        public UInt64 ImpersonatedUserId;

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
                        if(attr != null && attr.AttrType == AttrType.VALUE)
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
                return User != null ? User.SessionId : null;
            }
        }

        public string BaseUrl
        {
            get
            {
                return _baseUrl;
            }
        }

        public string Language
        {
            get
            {
                return Localization != null ? Localization.CurrentLanguage : null;
            }
        }

        public void SetBaseUrl(string url)
        {
            var baseurl = StringUtils.FixBaseUri(url);
            if(baseurl != null)
            {
                Uri uri;
                if(!Uri.TryCreate(baseurl, UriKind.Absolute, out uri))
                {
                    throw new InvalidOperationException("Invalid base Url.");
                }
            }
            _baseUrl = baseurl;

        }

        bool FakeEnvironment
        {
            get
            {
                return string.IsNullOrEmpty(_baseUrl);
            }
        }

        public SocialPointLogin(IHttpClient client, LoginConfig config)
        {
            Init();
            SetBaseUrl(config.BaseUrl);
            _httpClient = client;
            _loginConfig = config;
            _availableSecurityTokenErrorRetries = config.SecurityTokenErrors;
            _availableConnectivityErrorRetries = config.ConnectivityErrors;
        }

        bool CheckFakeEnvironment(ErrorDelegate cbk)
        {
            if(FakeEnvironment)
            {
                if(cbk != null)
                {
                    cbk(new Error("Not supported"));
                }
                return true;
            }
            return false;
        }

        [System.Diagnostics.Conditional(DebugFlags.DebugLoginFlag)]
        void DebugLog(string msg)
        {
            Log.i(string.Format("SocialPointLogin {0}", msg));
        }

        void Init()
        {
            _userId = 0;
            ImpersonatedUserId = 0;
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
            User = new LocalUser();
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
            for(var i = 0; i < _links.Count; ++i)
            {
                var info = _links[i];
                info.Link.ClearStateChangeDelegate();
            }
            _links.Clear();
            Friends.Clear();
            _pendingLinkConfirms.Clear();
        }

        void AddLinkInfo(LinkInfo info)
        {
            DebugLog("AddLinkInfo");

            if(!_links.Contains(info))
            {
                info.Link.AddStateChangeDelegate(state => OnLinkStateChanged(info, state));
                DebugUtils.Assert(_links.FirstOrDefault(item => item == info) == null);
                _links.Add(info);
            }
        }

        Error HandleLoginErrors(HttpResponse resp, ErrorType def)
        {
            DebugLog("HandleLoginErrors");

            ErrorType typ = def;
            Error err = null;
            var data = new AttrDic();
            AttrDic json = null;
            if(resp.HasError)
            {
                json = new JsonAttrParser().Parse(resp.Body).AsDic;
            }

            if(resp.StatusCode == ForceUpgradeError)
            {
                err = new Error("The game needs to be upgraded.");
                typ = ErrorType.Upgrade;
                if(resp.Body != null)
                {
                    json = new JsonAttrParser().Parse(resp.Body).AsDic;
                }
                LoadGenericData(json.Get(AttrKeyGenericData));
            }
            else if(resp.StatusCode == RootedDeviceError)
            {
                err = new Error("The device has been rooted.");
                typ = ErrorType.Rooted;
                LoadGenericData(json.Get(AttrKeyGenericData));
            }
            else if(resp.StatusCode == InvalidSecurityTokenError)
            {
                err = new Error("The user cannot be recovered.");
                typ = ErrorType.InvalidSecurityToken;
            }
            else if(resp.StatusCode == InvalidPrivilegeTokenError)
            {
                err = new Error("Privilege token is invalid.");
                PrivilegeToken = null;
                ImpersonatedUserId = 0;
                typ = ErrorType.InvalidPrivilegeToken;
            }
            else if(json != null)
            {
                err = AttrUtils.GetError(json);
            }
            if(Error.IsNullOrEmpty(err) && resp.HasError && resp.StatusCode != MaintenanceMode)
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
            DebugLog("HandleLinkErrors");

            ErrorType typ = def;
            Error err = null;
            var data = new AttrDic();

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
            DebugLog("HandleResponseErrors");

            ErrorType typ = def;
            Error err = null;
            var data = new AttrDic();
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

                Attr genericDataAttr = null;
                if(json != null)
                {
                    genericDataAttr = json.Get(AttrKeyGenericData);
                }
                LoadGenericData(genericDataAttr);
            }
            else if(resp.StatusCode == InvalidSessionError)
            {
                err = new Error("Session is invalid.");
                typ = ErrorType.InvalidSession;
            }
            else if(resp.HasConnectionError)
            {
                err = resp.StatusCode == (int)HttpResponse.StatusCodeType.TimeOutError ? new Error("The connection timed out.") : new Error("The connection could not be established.");
                typ = ErrorType.Connection;
                err.Code = resp.ErrorCode;
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

        public Uri GetUrl(string path)
        {
            var baseUrl = _baseUrl;
            if(string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = DefaultBaseUrl;
            }
            var uriStr = StringUtils.CombineUri(baseUrl + BaseUri, path);
            string deviceId = "0";
            if(DeviceInfo != null)
            {
                deviceId = DeviceInfo.Uid;
            }
            uriStr = string.Format(uriStr, UserId, deviceId);
            Uri uri;
            Uri.TryCreate(uriStr, UriKind.Absolute, out uri);
            return uri;
        }

        void DoFakeLogin(ErrorDelegate cbk)
        {
            if(NewUserStreamEvent != null)
            {
                NewUserStreamEvent(new EmptyStreamReader());
            }
            else if(NewUserEvent != null)
            {
                NewUserEvent(null, false);
            }
            OnLoginEnd(null, cbk);
        }

        void DoLogin(ErrorDelegate cbk, int lastErrCode = 0, byte[] responseBody = null)
        {
            DebugLog("DoLogin");

            if(_appEvents != null)
            {
                SetAppSource(_appEvents.Source);
            }
            _pendingLinkConfirms.Clear();
            if(_availableSecurityTokenErrorRetries < 0)
            {
                DebugLog("DoLogin - _availableSecurityTokenErrorRetries < 0");

                var err = new Error(lastErrCode, "Max amount of login retries reached.");
                NotifyError(ErrorType.LoginMaxRetries, err);
                OnLoginEnd(err, cbk);
            }
            else if(_availableConnectivityErrorRetries < 0)
            {
                DebugLog("DoLogin - _availableConnectivityErrorRetries < 0");

                #if DEBUG
                if(responseBody != null && responseBody.Length > 0)
                {
                    DebugLog(string.Format("SocialPointLogin Error Response:\n{0}", System.Text.Encoding.Default.GetString(responseBody)));
                }
                #endif

                var err = new Error(lastErrCode, "There was an error with the connection.");
                NotifyError(ErrorType.Connection, err);
                OnLoginEnd(err, cbk);
            }
            else if(FakeEnvironment)
            {
                DoFakeLogin(cbk);
            }
            else
            {
                var req = new HttpRequest(GetUrl(LoginUri), HttpRequest.MethodType.POST);
                SetupLoginHttpRequest(req);
                if(HttpRequestEvent != null)
                {
                    HttpRequestEvent(req);
                }

                DebugLog("DoLogin- login\n----\n" + req + "----\n");
                _httpClient.Send(req, resp => OnLogin(resp, cbk));
            }
        }

        void OnLogin(HttpResponse resp, ErrorDelegate cbk)
        {
            DebugLog("OnLogin - login\n----\n" + resp + "----\n");

            if(resp.StatusCode == InvalidSecurityTokenError && !UserHasRegistered)
            {
                ClearStoredUser();
                _availableSecurityTokenErrorRetries--;
                DoLogin(cbk, resp.ErrorCode);
                return;
            }
            if(resp.HasRecoverableError && resp.StatusCode != MaintenanceMode)
            {
                _availableConnectivityErrorRetries--;
                DoLogin(cbk, resp.ErrorCode, resp.Body);
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

            DebugLog("OnLogin - error\n----\n" + err + "----\n");

            //If no errors, track successful login
            if(Error.IsNullOrEmpty(err) && TrackEvent != null)
            {
                var loginData = new AttrDic();
                loginData.SetValue(AttrKeyHttpDuration, resp.Duration);
                loginData.SetValue(AttrKeyHttpTransferDuration, resp.TransferDuration);
                loginData.SetValue(AttrKeyHttpConnectionDuration, resp.ConnectionDuration);
                loginData.SetValue(AttrKeyHttpDownloadSize, resp.DownloadSize / 1024.0);
                loginData.SetValue(AttrKeyHttpDownloadSpeed, resp.DownloadSpeed / 1024.0);
                var loginEvent = new AttrDic();
                loginEvent.Set(AttrKeyEventLogin, loginData);
                TrackEvent(EventNameLogin, loginEvent);
            }

            OnLoginEnd(err, cbk);
        }

        void LoadGenericData(Attr genericData)
        {
            if(genericData == null)
            {
                genericData = Attr.InvalidDic;
            }
            if(Data == null)
            {
                Data = new GenericData();
            }
            if(NewGenericDataEvent != null)
            {
                NewGenericDataEvent(genericData);
            }
            Data.Load(genericData);
            OnGenericDataLoaded();
        }

        void LoadGenericData(IStreamReader reader)
        {
            if(NewGenericDataEvent != null)
            {
                LoadGenericData(reader.ParseElement());
                return;
            }
            if(Data == null)
            {
                Data = new GenericData();
            }
            Data.Load(reader);
            OnGenericDataLoaded();
        }

        void OnGenericDataLoaded()
        {
            // update server time
            TimeUtils.Offset = Data.DeltaTime;
        }

        void OnLoginEnd(Error err, ErrorDelegate cbk)
        {
            // Reset retry values
            _availableConnectivityErrorRetries = _loginConfig.ConnectivityErrors;
            _availableSecurityTokenErrorRetries = _loginConfig.SecurityTokenErrors;
            if(Error.IsNullOrEmpty(err))
            {
                _linkChange = false;
                _linkChangeCode = 0;
            }
            if(cbk != null)
            {
                cbk(err);
            }
            if(ImpersonatedUserId == 0 && Error.IsNullOrEmpty(err))
            {
                NextLinkLogin(null, null, LinkInfo.Filter.Auto);
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
                    return _links.Count > linkPos ? _links.GetRange(linkPos, _links.Count - linkPos).FirstOrDefault(item => item.MatchesFilter(filter)) : null;
                }
                return null;
            }
            return _links.FirstOrDefault(item => item.MatchesFilter(filter));
        }

        void NextLinkLogin(LinkInfo info, ErrorDelegate cbk, LinkInfo.Filter filter)
        {
            info = GetNextLinkInfo(info, filter);
            if(info == null)
            {
                if(AutoUpdateFriends && AutoUpdateFriendsPhotosSize > 0)
                {
                    GetUsersPhotos(new List<User> { User }, AutoUpdateFriendsPhotosSize, (users, err) => {
                        if(cbk != null)
                        {
                            cbk(err);
                        }
                    });
                }
                else if(cbk != null)
                {
                    cbk(null);
                }
            }
            else
            {
                DoLinkLogin(info, cbk, filter);
            }
        }

        void DoLinkLogin(LinkInfo info, ErrorDelegate cbk, LinkInfo.Filter filter)
        {
            DebugUtils.Assert(info != null && _links.FirstOrDefault(item => item == info) != null);
            info.Link.Login(err => OnLinkLogin(info, err, cbk, filter));
        }

        void OnLinkStateChanged(LinkInfo info, LinkState state)
        {
            DebugLog("OnLinkStateChanged");
            DebugLog("OnLinkStateChanged info: " + info);
            DebugLog("OnLinkStateChanged state: " + state);

            DebugUtils.Assert(info != null && _links.FirstOrDefault(item => item == info) != null);
            if(ImpersonatedUserId != 0)
            {
                return;
            }

            if(state == LinkState.Disconnected)
            {
                CleanOldFriends();
            }
            else if(state == LinkState.Connected)
            {
                LocalUser tmpUser = (User != null) ? new LocalUser(User) : new LocalUser();
                info.Link.UpdateLocalUser(tmpUser);

                if(tmpUser != null && User != null && tmpUser.Links.SequenceEqual(User.Links))
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
            DebugLog("OnNewLink - link info\n----\n" + info + "----\n");
            DebugLog("OnNewLink - link state\n----\n" + state + "----\n");

            // the user links have changed, we need to tell the server
            info.LinkData = info.Link.GetLinkData();

            if(info.LinkData.Count == 0)
            {
                return;
            }

            if(FakeEnvironment)
            {
                UpdateLinkData(info, false);
                return;
            }

            var req = new HttpRequest();
            SetupHttpRequest(req, LinkUri);
            req.AddParam(HttpParamSecurityToken, SecurityToken);
            req.AddParam(HttpParamLinkType, info.Link.Name);
            var itr = info.LinkData.GetEnumerator();
            while(itr.MoveNext())
            {
                var pair = itr.Current;
                req.AddParam(pair.Key, pair.Value);
            }
            itr.Dispose();
            DebugLog("OnNewLink - link\n----\n" + req + "----\n");
            _httpClient.Send(req, resp => OnNewLinkResponse(info, state, resp));
        }

        void OnNewLinkResponse(LinkInfo info, LinkState state, HttpResponse resp)
        {
            DebugLog("OnNewLinkResponse");

            if((resp.HasRecoverableError) && _availableConnectivityErrorRetries > 0)
            {
                _availableConnectivityErrorRetries--;
                OnNewLink(info, state);
                return;
            }

            DebugUtils.Assert(info != null && _links.FirstOrDefault(item => item == info) != null);
            DebugLog("OnNewLinkResponse - link resp.StatusCode\n----\n" + resp.StatusCode + "----\n");
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
            DebugLog("NotifyConfirmLink");

            bool wait = _pendingLinkConfirms.Count != 0;
            _pendingLinkConfirms.Add(info);
            if(!wait)
            {
                DebugUtils.Assert(info != null && _links.FirstOrDefault(item => item == info) != null);
                if(ConfirmLinkEvent != null)
                {
                    ConfirmLinkEvent(info.Link, type, data, decision => OnConfirmLinkNotifyBack(info, type, linkToken, decision));
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
                ConfirmLink(linkToken, decision, err => OnConfirmLinkNotifyBackEnd(info));
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

        static void CancelLink(LinkInfo info)
        {
            info.Link.Logout();
        }

        static List<UserMapping> LoadUserLinks(Attr data)
        {
            var links = new List<UserMapping>();
            if(data.AttrType == AttrType.LIST)
            {
                var linksAttr = data.AsList;
                var itr = linksAttr.GetEnumerator();
                while(itr.MoveNext())
                {
                    var elm = itr.Current;
                    var link = elm.AsDic;
                    var provider = link.GetValue(AttrKeyLinkProvider).AsValue.ToString();
                    var externalId = link.GetValue(AttrKeyLinkExternalId).AsValue.ToString();
                    links.Add(new UserMapping(externalId, provider));
                }
                itr.Dispose();
            }
            else if(data.AttrType == AttrType.DICTIONARY)
            {
                var linksAttr = data.AsDic;
                var itr = linksAttr.GetEnumerator();
                while(itr.MoveNext())
                {
                    var elm = itr.Current;
                    var provider = elm.Key;
                    var externalId = elm.Value.AsValue.ToString();
                    links.Add(new UserMapping(externalId, provider));
                }
                itr.Dispose();
            }
            return links;
        }

        static User LoadUser(Attr data)
        {
            if(data.AttrType != AttrType.DICTIONARY)
            {
                return null;
            }
            var dataDict = data.AsDic;
            if(dataDict.ContainsKey(AttrKeyUserId))
            {
                UInt64 userId;
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

        static LocalUser LoadLocalUser(Attr data)
        {
            var user = LoadUser(data);
            if(user == null)
            {
                return null;
            }
            var sessionId = data.AsDic.GetValue(AttrKeySessionId).ToString();
            return new LocalUser(user.Id, sessionId, user.Links);
        }

        Error ReadNewLocalUser(byte[] data, out ErrorType errType)
        {
            errType = ErrorType.UserParse;
            var reader = new JsonStreamReader(data);            
            if(!reader.Read() || reader.Token != StreamToken.ObjectStart)
            {
                return new Error("Empty login response");
            }

            Error err = null;
            bool userIdChanged = false;
            Attr gameData = null;
            while(reader.Read() && reader.Token != StreamToken.ObjectEnd && Error.IsNullOrEmpty(err))
            {
                if(reader.Token != StreamToken.PropertyName)
                {
                    err = new Error("Trying to parse object without property name.");
                }
                var key = reader.GetStringValue();
                reader.Read();
                switch(key)
                {
                case AttrKeyLoginData:
                    {
                        User = LoadLocalUser(reader.ParseElement());
                        if(User == null)
                        {
                            err = new Error("Could not load the user.");
                        }
                        else
                        {
                            userIdChanged = UserId != User.Id;
                            UserId = User.Id;
                        }
                        break;
                    }
                case AttrKeyGameData:
                    {
                        if(NewUserStreamEvent != null)
                        {
                            if(!NewUserStreamEvent(reader))
                            {
                                errType = ErrorType.GameDataParse;
                                err = new Error("Could not load Game Data");
                            }
                        }
                        else if(NewUserEvent != null)
                        {
                            gameData = reader.ParseElement();
                        }
                        else
                        {
                            reader.SkipElement();
                        }
                        break;
                    }
                case AttrKeyGenericData:
                    {
                        LoadGenericData(reader);
                        if(Data != null && Data.Upgrade != null && Data.Upgrade.Type != UpgradeType.None)
                        {
                            // Check for upgrade
                            err = new Error(Data.Upgrade.Message);
                            errType = ErrorType.Upgrade;
                        }
                        break;
                    }
                default:
                    reader.SkipElement();
                    break;
                }
            }

            if(Error.IsNullOrEmpty(err) && User != null)
            {
                if(NewUserEvent != null)
                {
                    NewUserEvent(gameData, userIdChanged);
                }
                if(NewUserChangeEvent != null)
                {
                    NewUserChangeEvent(userIdChanged);
                }
            }

            return err;
        }

        Error OnNewLocalUser(HttpResponse resp)
        {
            Error err;
            ErrorType errType = ErrorType.UserParse;
            User = null;
            Exception exc = null;
            try
            {
                err = ReadNewLocalUser(resp.Body, out errType);
            }
            catch(Exception e)
            {
                exc = e;
                err = new Error(e.ToString());
            }
            if(Error.IsNullOrEmpty(err))
            {
                UserHasRegistered = true;
                for(int i = 0, _linksCount = _links.Count; i < _linksCount; i++)
                {
                    var linkInfo = _links[i];
                    linkInfo.Link.OnNewLocalUser(User);
                }
            }
            else
            {
                var errData = new AttrDic();
                // If the error is a parse error, we don't want to deserialize all the gameData.
                if(errType != ErrorType.GameDataParse && (exc != null && exc.GetType() != typeof(UnityEngine.Assertions.AssertionException)))
                {
                    var attrParser = new JsonAttrParser();
                    errData.Set(AttrKeyData, attrParser.Parse(resp.Body));
                }
                NotifyError(errType, err, errData);
            }
            return err;
        }

        static int GetLinkConfirmTypeCode(LinkConfirmType type)
        {
            switch(type)
            {
            case LinkConfirmType.LinkedToLinked:
                return LinkedToLinkedError;
            case LinkConfirmType.LinkedToLoose:
                return LinkedToLooseError;
            case LinkConfirmType.LooseToLinked:
                return LooseToLinkedError;
            default:
                return 0;
            }
        }

        void OnLinkConfirmResponse(string linkToken, LinkInfo info, LinkConfirmDecision decision, HttpResponse resp, ErrorDelegate cbk)
        {
            if((resp.HasRecoverableError) && _availableConnectivityErrorRetries > 0 && _loginConfig.EnableOnLinkConfirm)
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

            var linkConfirmTypeCode = 0;

            if(info != null)
            {
                linkConfirmTypeCode = GetLinkConfirmTypeCode(info.ConfirmType);
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
                            UInt64 newUserId;
                            UInt64.TryParse(data.ToString(), out newUserId);
                            if(newUserId != 0)
                            {
                                // if confirm returns a new user id we need to relogin
                                if(newUserId != UserId)
                                {
                                    _linkChangeCode = linkConfirmTypeCode;
                                    _linkChange = true;

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
                Restart();
            }

        }

        void Restart()
        {
            if(RestartEvent != null)
            {
                RestartEvent();
            }

            if(_restartLogin)
            {
                Login();
            }
        }

        void NotifyNewLink(LinkInfo info, bool beforeFriends)
        {
            DebugLog("NotifyNewLink");

            DebugUtils.Assert(info != null && _links.FirstOrDefault(item => item == info) != null);
            if(beforeFriends)
            {
                if(NewLinkBeforeFriendsEvent != null)
                {
                    NewLinkBeforeFriendsEvent(info.Link);
                }
            }
            else
            {
                if(NewLinkAfterFriendsEvent != null)
                {
                    NewLinkAfterFriendsEvent(info.Link);
                }
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
                        uid = uid != null && uid.Length > 7 ? uid.Substring(0, 8) : "";
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

            TrackError(type, err, data);

            if(!type.IsLinkError())
            {
                if(ErrorEvent != null)
                {
                    ErrorEvent(type, err, data);
                }
            }
            else
            {
                if(LinkErrorEvent != null)
                {
                    LinkErrorEvent(type, err, data);
                }
            }
        }

        void TrackError(ErrorType type, Error err, AttrDic data)
        {
            if(TrackEvent != null && CanTrackLoginError(err.Code))
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

                if(type.IsLinkError())
                {
                    TrackEvent(EventNameLinkError, evData);
                }
                else
                {
                    TrackEvent(EventNameLoginError, evData);
                }
            }
        }

        bool CanTrackLoginError(int code)
        {
            var now = TimeUtils.Timestamp;
            var elapsed = now - _lastTrackedErrorTimestamp;
            bool isErrorRepeating = (_lastTrackedErrorCode == code);

            // Avoid trackign repeated errors in a defined span of time
            if(isErrorRepeating && elapsed < TrackErrorMinElapsedTime)
            {
                return false;
            }

            _lastTrackedErrorTimestamp = now;
            _lastTrackedErrorCode = code;

            return true;
        }

        void OnAppRequestResponse(HttpResponse resp, AppRequest req, ErrorDelegate cbk)
        {
            DebugLog("OnAppRequestResponse req\n----\n" + resp + "---\n");
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
            DebugLog("OnAppRequestLinkNotified req\n----\n" + req + "---\n");
            if(Error.IsNullOrEmpty(err))
            {
                info = GetNextLinkInfo(info, LinkInfo.Filter.All);
                if(info != null)
                {
                    info.Link.NotifyAppRequestRecipients(req, err2 => OnAppRequestLinkNotified(info, req, err2, cbk));
                    return;
                }
            }
            OnAppRequestEnd(req, err, cbk);
        }

        static void OnAppRequestEnd(AppRequest req, Error err, ErrorDelegate cbk)
        {
            if(cbk != null)
            {
                cbk(err);
            }
        }

        void UpdateLinkData(LinkInfo info, bool disableUpdatingFriends)
        {
            DebugLog("UpdateLinkData");

            DebugUtils.Assert(info != null && _links.FirstOrDefault(item => item == info) != null);
            info.Link.UpdateLocalUser(User);

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
                _httpClient.Send(req, resp2 => OnUpdateFriendsResponse(resp2, mappings, block + 1, cbk));
            }
            else
            {
                OnUpdateFriendsEnd(mappings, null, cbk);
            }
        }

        void OnUpdateFriendsEnd(List<UserMapping> mappings, Error err, UsersDelegate cbk)
        {
            var friendsSelection = Friends.Where(u => (mappings.Where(map => u.HasLink(map.Id)).Count > 0));

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
                    info.Link.UpdateUserPhoto(user, photoSize, err2 => OnUserPhotoLink(info, user, users, photoSize, err2, cbk));
                }
                else
                {
                    int userPos = users.IndexOf(user);
                    if(userPos != -1)
                    {
                        userPos++;
                        user = users.Count > userPos ? users[userPos] : null;
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

        static void OnUsersEnd(List<User> users, Error err, UsersDelegate cbk)
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
                    _httpClient.Send(req, resp2 => OnGetUsersByIdResponse(resp2, mappings, block + 1, photoSize, users, cbk));
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
            if(resp.Body != null && resp.Body.Length > 0)
            {
                AttrList data;
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

                var itr = data.GetEnumerator();
                while(itr.MoveNext())
                {
                    var elm = itr.Current;
                    var friendDict = elm.AsDic;
                    var tmpUser = LoadUser(friendDict);

                    for(int i = 0, _linksCount = _links.Count; i < _linksCount; i++)
                    {
                        var linkInfo = _links[i];
                        linkInfo.Link.UpdateUser(tmpUser);
                    }

                    users.RemoveAll(u => u == tmpUser);
                    users.Add(tmpUser);
                }
                itr.Dispose();
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
            for(int i = 0, tmpUsersCount = tmpUsers.Count; i < tmpUsersCount; i++)
            {
                var user = tmpUsers[i];
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

            for(int i = 0, mappingsCount = mappings.Count; i < mappingsCount; i++)
            {
                var um = mappings[i];
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
            if(Math.Abs(req.Timeout) < Single.Epsilon)
            {
                req.Timeout = Timeout;
            }
            if(Math.Abs(req.ActivityTimeout) < Single.Epsilon)
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
                if(!req.HasParam(HttpParamDeviceTotalMemory))
                {
                    req.AddParam(HttpParamDeviceTotalMemory, DeviceInfo.MemoryInfo.TotalMemory.ToString());
                }
                if(!req.HasParam(HttpParamDeviceUsedMemory))
                {
                    req.AddParam(HttpParamDeviceUsedMemory, DeviceInfo.MemoryInfo.UsedMemory.ToString());
                }
                if(!req.HasParam(HttpParamDeviceTotalStorage))
                {
                    req.AddParam(HttpParamDeviceTotalStorage, DeviceInfo.StorageInfo.TotalStorage.ToString());
                }
                if(!req.HasParam(HttpParamDeviceUsedStorage))
                {
                    req.AddParam(HttpParamDeviceUsedStorage, DeviceInfo.StorageInfo.UsedStorage.ToString());
                }
                if(!req.HasParam(HttpParamDeviceMaxTextureSize))
                {
                    req.AddParam(HttpParamDeviceMaxTextureSize, DeviceInfo.MaxTextureSize.ToString());
                }
                if(!req.HasParam(HttpParamDeviceScreenWidth))
                {
                    req.AddParam(HttpParamDeviceScreenWidth, DeviceInfo.ScreenSize.x.ToString());
                }
                if(!req.HasParam(HttpParamDeviceScreenHeight))
                {
                    req.AddParam(HttpParamDeviceScreenHeight, DeviceInfo.ScreenSize.y.ToString());
                }
                if(!req.HasParam(HttpParamDeviceScreenDpi))
                {
                    req.AddParam(HttpParamDeviceScreenDpi, DeviceInfo.ScreenDpi.ToString());
                }
                if(!req.HasParam(HttpParamDeviceCpuCores))
                {
                    req.AddParam(HttpParamDeviceCpuCores, DeviceInfo.CpuCores.ToString());
                }
                if(!req.HasParam(HttpParamDeviceCpuFreq))
                {
                    req.AddParam(HttpParamDeviceCpuFreq, DeviceInfo.CpuFreq.ToString());
                }
                if(!req.HasParam(HttpParamDeviceCpuModel))
                {
                    req.AddParam(HttpParamDeviceCpuModel, DeviceInfo.CpuModel);
                }
                if(!req.HasParam(HttpParamDeviceOpenglVendor))
                {
                    req.AddParam(HttpParamDeviceOpenglVendor, DeviceInfo.OpenglVendor);
                }
                if(!req.HasParam(HttpParamDeviceOpenglRenderer))
                {
                    req.AddParam(HttpParamDeviceOpenglRenderer, DeviceInfo.OpenglRenderer);
                }
                if(!req.HasParam(HttpParamDeviceOpenglShading))
                {
                    req.AddParam(HttpParamDeviceOpenglShading, DeviceInfo.OpenglShadingVersion.ToString());
                }
                if(!req.HasParam(HttpParamDeviceOpenglVersion))
                {
                    req.AddParam(HttpParamDeviceOpenglVersion, DeviceInfo.OpenglVersion);
                }
                if(!req.HasParam(HttpParamDeviceOpenglMemory))
                {
                    req.AddParam(HttpParamDeviceOpenglMemory, DeviceInfo.OpenglMemorySize.ToString());
                }
            }
            if(!req.HasParam(HttpParamLinkChange))
            {
                req.AddParam(HttpParamLinkChange, _linkChange ? "1" : "0");
            }
            if(!req.HasParam(HttpParamLinkChangeCode))
            {
                req.AddParam(HttpParamLinkChangeCode, new AttrInt(_linkChangeCode));
            }

            AddForcedErrorRequestParams(req);
        }

        #region Forced Login Errors

        public void SetForcedErrorCode(string code)
        {
            _forcedErrorCode = code;
        }

        public string GetForcedErrorCode()
        {
            return _forcedErrorCode;
        }

        public void SetForcedErrorType(string type)
        {
            _forcedErrorType = type;
        }

        public string GetForcedErrorType()
        {
            return _forcedErrorType;
        }

        public void AddForcedErrorRequestParams(HttpRequest req)
        {
            #if ADMIN_PANEL
            if(AdminPanel.AdminPanel.IsAvailable)
            {
                if(!string.IsNullOrEmpty(_forcedErrorCode))
                {
                    req.AddParam(HttpParamForcedErrorCode, _forcedErrorCode);
                }
                if(!string.IsNullOrEmpty(_forcedErrorType))
                {
                    req.AddParam(HttpParamForcedErrorType, _forcedErrorType);
                }
            }
            #endif
        }

        #endregion

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
         */
        public void AddLink(ILink link)
        {
            AddLinkInfo(new LinkInfo(link));
        }

        /**
         * Remove a link
         * @return true if the link was found
         */
        public bool RemoveLink(ILink link)
        {
            LinkInfo linkInfo = _links.FirstOrDefault(item => item.Link == link);
            return linkInfo != null && _links.Remove(linkInfo);
        }

        /**
         * Confirm a link
         * @param the link token that the server sent when trying to link
         * @param result true if the link is confirmed, false if cancelled
         * @param callback called when the link is finished
         */
        public void ConfirmLink(string linkToken, LinkConfirmDecision decision, ErrorDelegate cbk = null)
        {
            DebugLog("ConfirmLink");

            if(CheckFakeEnvironment(cbk))
            {
                return;
            }
            LinkInfo linkInfo = _links.FirstOrDefault(item => item.Token == linkToken);
            var req = new HttpRequest();
            SetupHttpRequest(req, LinkConfirmUri);
            req.AddParam(HttpParamSecurityToken, SecurityToken);
            req.AddParam(HttpParamLinkConfirmToken, linkToken);
            req.AddParam(HttpParamLinkDecision, decision.ToString().ToLower());

            DebugLog("ConfirmLink - link confirm\n----\n" + req + "----\n");
            _httpClient.Send(req, resp => OnLinkConfirmResponse(linkToken, linkInfo, decision, resp, cbk));
        }

        /**
         * Clear all the user information
         */
        public void ClearStoredUser()
        {
            _userId = 0;
            ImpersonatedUserId = 0;
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
            for(int i = 0, FriendsCount = Friends.Count; i < FriendsCount; i++)
            {
                var friend = Friends[i];
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
            if(Math.Abs(req.Timeout) < Single.Epsilon)
            {
                req.Timeout = Timeout;
            }
            if(Math.Abs(req.ActivityTimeout) < Single.Epsilon)
            {
                req.ActivityTimeout = ActivityTimeout;
            }
            req.Method = HttpRequest.MethodType.POST;
            req.Url = GetUrl(Uri);
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
            for(int i = 0, _linksCount = _links.Count; i < _linksCount; i++)
            {
                var linkInfo = _links[i];
                linkInfo.Link.GetFriendsData(mappings);
            }
            UpdateFriends(mappings, cbk);
        }

        public void UpdateFriends(List<UserMapping> mappings, UsersDelegate cbk = null)
        {
            if(FakeEnvironment)
            {
                if(cbk != null)
                {
                    cbk(new List<User>(), null);
                }
                return;
            }
            if(mappings.Count > 0)
            {
                var resp = new HttpResponse();
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

            for(int i = 0, userIdsCount = userIds.Count; i < userIdsCount; i++)
            {
                var userId = userIds[i];
                var u = new User();
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

            for(int i = 0, userIdsCount = userIds.Count; i < userIdsCount; i++)
            {
                var userId = userIds[i];
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
            for(int i = 0, userIdsCount = userIds.Count; i < userIdsCount; i++)
            {
                var userId = userIds[i];
                var u = new User();
                if(GetCachedUserById(userId, u))
                {
                    users.Add(u);
                }
            }
        }

        public void GetCachedUsersByTempId(List<string> userIds, List<User> users)
        {
            for(int i = 0, userIdsCount = userIds.Count; i < userIdsCount; i++)
            {
                var userId = userIds[i];
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
            if(CheckFakeEnvironment(cbk))
            {
                return;
            }
            var httpReq = new HttpRequest();
            SetupHttpRequest(httpReq, AppRequestsUri);

            var appRequestParams = new AttrDic();
            appRequestParams.Set(HttpParamAppRequestType, new AttrString(req.Type));

            var toParam = new AttrDic();

            for(int i = 0, reqRecipientsCount = req.Recipients.Count; i < reqRecipientsCount; i++)
            {
                var user = req.Recipients[i];
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

            DebugLog("SendAppRequest app req\n----\n" + httpReq + "----\n");
            _httpClient.Send(httpReq, resp => OnAppRequestResponse(resp, req, cbk));
        }

        public void GetReceivedAppRequests(AppRequestDelegate cbk = null)
        {
            if(FakeEnvironment)
            {
                if(cbk != null)
                {
                    cbk(new List<AppRequest>(), null);
                }
                return;
            }
            var httpReq = new HttpRequest();
            SetupHttpRequest(httpReq, AppRequestsUri);
            httpReq.Method = HttpRequest.MethodType.GET;
            _httpClient.Send(httpReq, resp => OnReceivedAppRequestResponse(resp, cbk));
        }

        void OnReceivedAppRequestResponse(HttpResponse resp, AppRequestDelegate cbk)
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
                            var itr = receivedAppRequest.GetEnumerator();
                            while(itr.MoveNext())
                            {
                                var elm = itr.Current;
                                AttrDic requestData = elm.Value.AsDic;
                                requestData["id"] = new AttrString(elm.Key);
                                var req = new AppRequest(requestData["type"].AsValue.ToString(), requestData);
                                reqs.Add(req);
                            }
                            itr.Dispose();
                        }
                        else if(data.AttrType == AttrType.LIST)
                        {
                            var receivedAppRequest = data.AsList;
                            var itr = receivedAppRequest.GetEnumerator();
                            while(itr.MoveNext())
                            {
                                var elm = itr.Current;
                                var requestData = elm.AsDic;
                                var type = requestData.GetValue("type").AsValue.ToString();
                                reqs.Add(new AppRequest(type, requestData));
                            }
                            itr.Dispose();
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
            if(CheckFakeEnvironment(cbk))
            {
                return;
            }
            var req = new HttpRequest();
            SetupHttpRequest(req, AppRequestsUri);
            req.Method = HttpRequest.MethodType.DELETE;
            req.AddQueryParam(HttpParamRequestIds, String.Join(",", ids.ToArray()));
            _httpClient.Send(req, resp => OnDeleteAppRequestResponse(resp, cbk));
        }

        void OnDeleteAppRequestResponse(HttpResponse resp, ErrorDelegate cbk)
        {
            var err = HandleResponseErrors(resp, ErrorType.ReceiveAppRequests);
            if(cbk != null)
            {
                cbk(err);
            }
        }
    }

    public sealed class LinkInfo
    {
        public enum Filter
        {
            Auto,
            Normal,
            All,
            None
        }

        public ILink Link { get; private set; }

        public string Token { get; set; }

        public LinkConfirmType ConfirmType { get; set; }

        public AttrDic LinkData { get; set; }

        public bool Pending { get; set; }

        LinkInfo(LinkInfo other)
        {
            Link = other.Link;
            Token = other.Token;
            LinkData = new AttrDic(other.LinkData);
        }

        public LinkInfo(ILink link)
        {
            Link = link;
        }

        public bool MatchesFilter(Filter filter)
        {
            bool result = false;

            switch(Link.Mode)
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
