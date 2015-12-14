using System;
using System.Collections.Generic;

using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Network;

namespace SocialPoint.Login
{
    public class EmptyLogin : ILogin
    {
        public event HttpRequestDelegate HttpRequestEvent
        {
            add { }
            remove { }
        }

        public event NewUserDelegate NewUserEvent
        {
            add { }
            remove { }
        }

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

        public UInt64 UserId{ get; set; }

        public string SessionId{ get{ return null; } }

        public string PrivilegeToken{ set{} }

        public GenericData Data{ get; set; }

        private string _baseUri;

        virtual public void Dispose()
        {
        }

        public void SetupHttpRequest(HttpRequest req, string uri)
        {
            if(_baseUri != null)
            {
                req.Url = new Uri(new Uri(_baseUri), uri);
            }
            else
            {
                req.Url = new Uri(uri);
            }
        }

        public void Login(ErrorDelegate cbk = null)
        {
        }

        public EmptyLogin(string baseUri = null)
        {
            _baseUri = baseUri;
        }

        public void ClearStoredUser()
        {
        }
    }
}
