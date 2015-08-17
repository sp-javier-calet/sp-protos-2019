using System;
using System.Collections.Generic;

using SocialPoint.Attributes;
using SocialPoint.Utils;
using SocialPoint.Network;

namespace SocialPoint.Login
{
    public delegate void LoginDelegate(Error err);
    
    public delegate void LoginHttpRequestDelegate(HttpRequest req);
    
    public delegate void LoginUsersDelegate(List<User> users, Error err);
    
    public delegate void LoginConfirmBackLinkDelegate(LinkConfirmDecision decision);
    
    public delegate void LoginConfirmLinkDelegate(ILink link, LinkConfirmType type, Attr data,LoginConfirmBackLinkDelegate cbk);
    
    public delegate void LoginNewUserDelegate(Attr data);
    
    public delegate void LoginNewLinkDelegate(ILink link);
    
    public delegate void LoginErrorDelegate(ErrorType error, string msg, Attr data);
    
    public delegate void RestartDelegate();
    
    public delegate void AppRequestDelegate(List<AppRequest> reqs, Error err);

    public interface ILogin : IDisposable
    {
        event LoginHttpRequestDelegate HttpRequestEvent;
        event LoginNewUserDelegate NewUserEvent;
        event LoginNewLinkDelegate NewLinkBeforeFriendsEvent;
        event LoginNewLinkDelegate NewLinkAfterFriendsEvent;
        event LoginConfirmLinkDelegate ConfirmLinkEvent;
        event LoginErrorDelegate ErrorEvent;
        event RestartDelegate RestartEvent;

        UInt64 UserId{ get; set; }

        string SessionId{ get; }

        void SetupHttpRequest(HttpRequest req, string uri);

		void Login(LoginDelegate cbk = null, LinkFilter filter = LinkFilter.Auto);
    }
}