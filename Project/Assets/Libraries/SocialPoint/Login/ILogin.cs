using System;
using System.Collections.Generic;
using SocialPoint.Network;
using SocialPoint.Base;

namespace SocialPoint.Login
{
    public interface ILogin : IDisposable
    {
        event HttpRequestDelegate HttpRequestEvent;
        event NewUserDelegate NewUserEvent;
        event NewLinkDelegate NewLinkBeforeFriendsEvent;
        event NewLinkDelegate NewLinkAfterFriendsEvent;
        event ConfirmLinkDelegate ConfirmLinkEvent;
        event LoginErrorDelegate ErrorEvent;
        event RestartDelegate RestartEvent;
        event UpgradeDelegate UpgradeEvent;

        UInt64 UserId{ get; }

        string SessionId{ get; }

        string PrivilegeToken{ set; }

        void SetupHttpRequest(HttpRequest req, string uri);

		void Login(ErrorDelegate cbk = null, LinkFilter filter = LinkFilter.Auto);

        void ClearStoredUser();
    }
}