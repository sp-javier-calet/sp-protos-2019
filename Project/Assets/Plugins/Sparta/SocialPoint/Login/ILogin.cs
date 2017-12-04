using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Network;

namespace SocialPoint.Login
{
    public interface ILoginData
    {
        /// <summary>
        /// Current Local User
        /// </summary>
        LocalUser User { get; }

        /// <summary>
        /// Friend List
        /// </summary>
        List<User> Friends { get; }

        /// <summary>
        /// Current User Id
        /// </summary>
        UInt64 UserId { get; }

        /// <summary>
        /// Current Session Id
        /// </summary>
        string SessionId { get; }

        /// <summary>
        /// Current Security Token
        /// </summary>
        string SecurityToken { get; }

        /// <summary>
        /// Current Privilege Token
        /// </summary>
        string PrivilegeToken { get; }

        /// <summary>
        /// Generic data received in login response
        /// </summary>
        GenericData Data { get; }

        /// <summary>
        /// Base URL to the backend environment
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// Setup delegate for Http requests
        /// </summary>
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

        event HttpRequestDelegate HttpRequestEvent;

        void Login(ErrorDelegate cbk = null);

        void ClearStoredUser();

        void SetBaseUrl(string url);

        void AddLink(ILink link);

        void LoginLinks(ErrorDelegate cbk = null);

        void LoginLink(ILink link, ErrorDelegate cbk = null);

        void ClearUser();

    }
}