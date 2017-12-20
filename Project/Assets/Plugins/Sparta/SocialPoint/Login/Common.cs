using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Network;

namespace SocialPoint.Login
{
    public delegate void HttpRequestDelegate(HttpRequest req);
    
    public delegate void UsersDelegate(List<User> users, Error err);
    
    public delegate void ConfirmBackLinkDelegate(LinkConfirmDecision decision);
    
    public delegate void ConfirmLinkDelegate(ILink link, LinkConfirmType type, Attr data, ConfirmBackLinkDelegate cbk);
    
    public delegate void NewUserDelegate(Attr data, bool changed);

    public delegate void NewUserChangeDelegate(bool changed);

    public delegate void NewGenericDataDelegate(Attr data);

    public delegate bool NewUserStreamDelegate(IStreamReader reader);
    
    public delegate void NewLinkDelegate(ILink link);
    
    public delegate void LoginErrorDelegate(ErrorType type, Error err, Attr data);
    
    public delegate void RestartDelegate();
    
    public delegate void AppRequestDelegate(List<AppRequest> reqs, Error err);
    
    public enum LinkConfirmType
    {
        /**
         * No need to confirm
         */
        None,
        
        /**
         * The account is already linked, not to the actual external service, start new game?
         */
        LinkedToLoose,
        
        /**
         * The account is already linked, not to the actual external service, load other game?
         */
        LinkedToLinked,
        
        /**
         * The account is already linked, but the current user is not linked to anything,
         * load the other game and loose the current state?
         */
        LooseToLinked
    }

    public enum LinkConfirmDecision
    {
        /**
         * Don't do anything
         * 
         */
        Cancel,
        
        /**
         * keep the current account
         */
        Keep,
        
        /**
         * change to the new account
         */
        Change
    }

    public enum ErrorType
    {
        /**
         * Error after trying to login the maximum amount of times 
         * and getting InvalidSecurityTokenError http response code each time
         * @see SocialPointLogin::setMaxLoginRetries
         */
        LoginMaxRetries,
        
        /**
         * Error getting ForceUpgradeError http code on login
         * or the login response contains a upgrade part (force or suggested)
         * meaning that the app version is not supported and should be updated
         */
        Upgrade,

        /**
         * Not managed http error code on login
         */
        Login,
        
        /**
         * Error parsing the user data on the login response
         */
        UserParse,
        
        /**
         * Error trying to login a link
         */
        LinkLogin,
        
        /**
         * Not handled error when trying to establish a link
         */
        Link,
        
        /**
         * Error trying to parse the link response json
         */
        LinkParse,
        
        /**
         * Not handled error loading friends
         */
        Friends,
        
        /**
         * Error when link responds SocialPointLogin::kInvalidSessionError http code
         */
        InvalidSession,
        
        /**
         * Error when link responds SocialPointLogin::kInvalidLinkDataError http code
         */
        InvalidLinkData,
        
        /**
         * Error when link responds SocialPointLogin::kInvalidPrivilegeTokenError http code
         */
        InvalidPrivilegeToken,
        
        /**
         * Error when link responds SocialPointLogin::kAlreadyLinkedError http code
         */
        AlreadyLinked,
        
        /**
         * Not handled error when trying to confirm a link
         */
        LinkConfirm,
        
        /**
         * Error when trying to parse the link confirm json
         */
        LinkConfirmParse,
        
        /**
         * Error on the http connection (f. ex. timeout or no internet)
         */
        Connection,
        
        /**
         * Not handled error sending an app request
         */
        AppRequest,
        
        /**
         * Not handled error receiving app requests
         */
        ReceiveAppRequests,

        /**
         * Error parsing users response
         */
        UsersParse,
        
        /**
         * Not handled error loading users
         */
        Users,

        /**
         * Backend temporally unavailable for maintenance reasons.
         */
        MaintenanceMode,

        /**
         * Error when link responds SocialPointLogin::kInvalidProviderTokenError http code
         */
        InvalidProviderToken,
        
        /**
         * Error when link responds SocialPointLogin::kInvalidSecurityTokenError http code
         * ant the user is not new
         */
        InvalidSecurityToken,

        /**
         * Error parsing the game data
         */
        GameDataParse,

        /**
         * 
         * Error getting RootedDeviceError http code on login
         * 
         * */
        Rooted,
    }

    public static class ErrorTypeExtensions
    {
        public static bool IsLinkError(this ErrorType type)
        {
            return type == ErrorType.Link || type == ErrorType.LinkConfirm || type == ErrorType.LinkConfirmParse || type == ErrorType.LinkLogin || type == ErrorType.LinkParse;
        }
    }

    public enum LinkMode
    {
        /**
         * Links with this mode will be logged in just after the main login
         * @see SocialPointLogin::login
         */
        Auto,

        /**
         * Links with this mode will be logged in when calling SocialPointLogin::loginLinks method
         * @see SocialPointLogin::loginLinks
         */
        Normal,
        
        /**
         * Links with this mode will not be logged in by SocialPointLogin
         * they have to login manually
         */
        Manual
    }

    public enum LinkState
    {
        Connected,
        Disconnected,
        Unknown
    }

    public enum UpgradeType
    {
        Suggested,
        Forced,
        None
    }

}
