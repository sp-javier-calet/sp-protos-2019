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
        const string kgameData = "game_data";

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

        public event LoginErrorDelegate ErrorEvent
        {
            add { }
            remove { }
        }

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
            var parser = new JsonAttrParser();
            var configResponse = parser.Parse(resp.Body);

            var mainDic = new AttrDic();
            mainDic.Set(kgameData, configResponse);

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

            if(cbk != null)
            {
                cbk(null);
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
