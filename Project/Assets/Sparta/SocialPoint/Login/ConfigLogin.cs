using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.Login
{
    public sealed class ConfigLogin : ILogin
    {
        const string GameData = "game_data";
        const string Config = "config";

        IHttpClient _httpClient;

        public void AddLink(ILink link, LinkMode mode = LinkMode.Auto)
        {
        }

        public void LoginLink(ILink link, ErrorDelegate cbk = null)
        {
        }

        public void ClearUser()
        {
        }

        public event HttpRequestDelegate HttpRequestEvent
        {
            add { }
            remove { }
        }

        public event NewUserDelegate NewUserEvent;

        public event NewUserChangeDelegate NewUserChangeEvent
        {
            add { }
            remove { }
        }

        public event NewUserStreamDelegate NewUserStreamEvent;

        public event NewGenericDataDelegate NewGenericDataEvent
        {
            add { }
            remove { }
        }

        public event NewLinkDelegate NewLinkBeforeFriendsEvent
        {
            add { }
            remove { }
        }

        public event NewLinkDelegate NewLinkAfterFriendsEvent
        {
            add { }
            remove { }
        }

        public event ConfirmLinkDelegate ConfirmLinkEvent
        {
            add { }
            remove { }
        }

        public event LoginErrorDelegate ErrorEvent = null;

        public event RestartDelegate RestartEvent
        {
            add { }
            remove { }
        }

        public List<User> Friends { get; private set; }

        public LocalUser User { get; private set; }

        public UInt64 UserId{ get; set; }

        public string SessionId{ get { return null; } }

        public string SecurityToken{ get { return null; } }

        public string PrivilegeToken{ get { return null; } set { } }

        public GenericData Data{ get; set; }

        string _baseUrl;

        public string BaseUrl
        {
            get
            {
                return _baseUrl;
            }
        }

        public void SetBaseUrl(string url)
        {
            _baseUrl = StringUtils.FixBaseUri(url);
        }

        public void Dispose()
        {
        }

        public void SetupHttpRequest(HttpRequest req, string uri)
        {
            req.Url = new Uri(StringUtils.CombineUri(BaseUrl, uri));
        }

        public void Login(ErrorDelegate cbk = null)
        {
            DoGetConfigData(cbk);
        }

        void DoGetConfigData(ErrorDelegate cbk)
        {
            var req = new HttpRequest(BaseUrl);
            _httpClient.Send(req, resp => OnGetConfigData(resp, cbk));
        }

        void OnGetConfigData(HttpResponse resp, ErrorDelegate cbk)
        {
            Error err = null;
            var parser = new JsonAttrParser();

            if(resp.HasError)
            {
                err = resp.Error;
                if(ErrorEvent != null)
                {
                    var errData = new AttrDic();
                    string errorString = parser.Parse(resp.Body).ToString();
                    errData.SetValue(SocialPointLogin.AttrKeySignature, errorString);
                    ErrorEvent(ErrorType.GameDataParse, err, errData);
                    Log.e("Error: "+ err.Code + " Message: "+ err.Msg+ " Signature: "+errorString);
                }
            }
            else
            {
                var configResponse = parser.Parse(resp.Body);

                var configDic = new AttrDic();
                configDic.Set(Config, configResponse);

                var mainDic = new AttrDic();
                mainDic.Set(GameData, configDic);

                var serializer = new JsonAttrSerializer();
                var finalBytes = serializer.Serialize(mainDic);
                
                var reader = new JsonStreamReader(finalBytes);

                if(!reader.Read() || reader.Token != StreamToken.ObjectStart)
                {
                    return;
                }

                Attr gameData = null;
                while(reader.Read() && reader.Token != StreamToken.ObjectEnd)
                {
                    if(reader.Token != StreamToken.PropertyName)
                    {
                        return;
                    }
                    reader.Read();

                    if(NewUserStreamEvent != null)
                    {
                        NewUserStreamEvent(reader);
                    }
                    else if(NewUserEvent != null)
                    {
                        gameData = reader.ParseElement();
                        NewUserEvent(gameData, false);
                    }
                    else
                    {
                        reader.SkipElement();
                    }
                }
            }

            if(cbk != null)
            {
                cbk(err);
            }
        }

        public ConfigLogin(IHttpClient client, string baseUri = null)
        {
            Init();
            SetBaseUrl(baseUri);
            _httpClient = client;

        }

        void Init()
        {
            User = new LocalUser();
            Friends = new List<User>();
        }

        public void ClearStoredUser()
        {
        }
    }
}
