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

        event NewUserChangeDelegate NewUserChangeEvent;

        event NewUserStreamDelegate NewUserStreamEvent;

        event NewGenericDataDelegate NewGenericDataEvent;

        event NewLinkDelegate NewLinkBeforeFriendsEvent;

        event NewLinkDelegate NewLinkAfterFriendsEvent;

        event ConfirmLinkDelegate ConfirmLinkEvent;

        event LoginErrorDelegate ErrorEvent;

        event RestartDelegate RestartEvent;

        UInt64 UserId { get; }

        string SessionId { get; }

        string PrivilegeToken { set; }

        GenericData Data { get; }

        void SetupHttpRequest(HttpRequest req, string uri);

		void Login(ErrorDelegate cbk = null);

        void ClearStoredUser();
    }
}