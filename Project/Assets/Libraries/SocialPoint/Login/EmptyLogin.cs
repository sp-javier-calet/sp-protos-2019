using System;
using System.Collections.Generic;

using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Network;

namespace SocialPoint.Login
{
    public class EmptyLogin : ILogin
    {
        public event LoginHttpRequestDelegate HttpRequestEvent;
        public event LoginNewUserDelegate NewUserEvent;
        public event LoginNewLinkDelegate NewLinkBeforeFriendsEvent;
        public event LoginNewLinkDelegate NewLinkAfterFriendsEvent;
        public event LoginConfirmLinkDelegate ConfirmLinkEvent;
        public event LoginErrorDelegate ErrorEvent;
        public event RestartDelegate RestartEvent;

        public UInt64 UserId{ get; set; }

        public string SessionId{ get{ return null; } }

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

        public void ClearUserId()
        {
        }
    }
}