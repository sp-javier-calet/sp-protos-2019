using System;
using System.Collections.Generic;

using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Network;

namespace SocialPoint.Login
{
    public class EmptyLogin : ILogin
    {
        public event HttpRequestDelegate HttpRequestEvent;
        public event NewUserDelegate NewUserEvent;
        public event NewLinkDelegate NewLinkBeforeFriendsEvent;
        public event NewLinkDelegate NewLinkAfterFriendsEvent;
        public event ConfirmLinkDelegate ConfirmLinkEvent;
        public event LoginErrorDelegate ErrorEvent;
        public event RestartDelegate RestartEvent;

        public UInt64 UserId{ get; set; }

        public string SessionId{ get{ return null; } }

        public string PrivilegeToken{ set{} }

        public GenericData Data{ get; set; }

        private string _baseUri;

        public void Dispose()
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

        public void Login(ErrorDelegate cbk = null, LinkFilter filter = LinkFilter.Auto)
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