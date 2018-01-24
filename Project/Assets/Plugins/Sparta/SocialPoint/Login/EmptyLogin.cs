using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.Login
{
    public sealed class EmptyLogin : ILogin
    {
        public void AddLink(ILink link)
        {
        }

        public void LoginLinks(ErrorDelegate cbk = null)
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

        public event LoginErrorDelegate LinkErrorEvent
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
            if(NewUserStreamEvent != null)
            {
                NewUserStreamEvent(new EmptyStreamReader());
            }
            else if(NewUserEvent != null)
            {
                NewUserEvent(null, false);
            }
            if(cbk != null)
            {
                cbk(null);
            }
        }

        public EmptyLogin(string baseUri = null)
        {
            SetBaseUrl(baseUri);
            User = new LocalUser();
            Friends = new List<User>();
        }

        public void ClearStoredUser()
        {
        }
    }
}
