using System;
using System.Collections.Generic;
using SocialPoint.Network;
using SocialPoint.Base;

namespace SocialPoint.Login
{
    public interface ILoginData
    {
        LocalUser User { get; }

        List<User> Friends { get; }

        UInt64 UserId { get; }

        string SessionId { get; }

        GenericData Data { get; }

        string BaseUrl { get; set; }

        void SetupHttpRequest(HttpRequest req, string uri);
    }

    public interface ILogin : ILoginData, IDisposable
    {
        event NewUserDelegate NewUserEvent;

        event NewUserChangeDelegate NewUserChangeEvent;

        event NewUserStreamDelegate NewUserStreamEvent;

        event NewGenericDataDelegate NewGenericDataEvent;

        event NewLinkDelegate NewLinkBeforeFriendsEvent;

        event NewLinkDelegate NewLinkAfterFriendsEvent;

        event ConfirmLinkDelegate ConfirmLinkEvent;

        event LoginErrorDelegate ErrorEvent;

        event RestartDelegate RestartEvent;

        void Login(ErrorDelegate cbk = null);

        void ClearStoredUser();
    }

}