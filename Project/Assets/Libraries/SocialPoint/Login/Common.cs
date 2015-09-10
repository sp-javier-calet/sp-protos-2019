using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Network;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Login
{
    public class UpgradeData
    {
        public UpgradeType Type;
        public string Message;
        public string Version;
        
        private const string AttrKeyUpgradeMessage = "message";
        private const string AttrKeyUpgradeVersion = "version";
        
        public UpgradeData(UpgradeType type, Attr data = null)
        {
            Type = type;
            
            if(data != null && data.AttrType == AttrType.DICTIONARY)
            {
                var datadic = data.AsDic;
                Message = datadic.Get(AttrKeyUpgradeMessage).AsValue.ToString();
                Version = datadic.GetValue(AttrKeyUpgradeVersion).AsValue.ToString();
                
            }
        }
    }
    
    public class GenericData
    {
        public TimeSpan DeltaTime;
        public string StoreUrl;
        public UpgradeData Upgrade;
        
        private const string AttrKeyTimestamp = "ts";
        private const string AttrKeyStoreUrl = "store";
        private const string AttrKeyUpgradeSuggested = "suggested_upgrade";
        private const string AttrKeyUpgradeForced = "forced_upgrade";
        
        public GenericData(Attr data)
        {
            var datadic = data.AsDic;
            var serverTime = TimeUtils.GetDateTime(datadic.Get(AttrKeyTimestamp).AsValue.ToLong());
            DeltaTime = serverTime - DateTime.UtcNow;
            StoreUrl = datadic.GetValue(AttrKeyStoreUrl).ToString();
            
            if(datadic.ContainsKey(AttrKeyUpgradeForced))
            {
                Upgrade = new UpgradeData(UpgradeType.Forced, datadic.Get(AttrKeyUpgradeForced));
            }
            else
                if(datadic.ContainsKey(AttrKeyUpgradeSuggested))
            {
                Upgrade = new UpgradeData(UpgradeType.Suggested, datadic.Get(AttrKeyUpgradeSuggested));
            }
            else
            {
                Upgrade = new UpgradeData(UpgradeType.None);
            }
        }
        
        [Obsolete("Use Upgrade.Type")]
        public bool ForcedUpgradeRequired
        {
            get
            {
                return Upgrade.Type == UpgradeType.Forced;
            }
        }
    }
    
    public class MaintenanceData
    {
        public string Title;
        public string Message;
        
        private const string AttrKeyMaintenanceMessage = "message";
        private const string AttrKeyMaintenanceTitle = "title";
        
        public MaintenanceData(Attr data = null)
        {
            if(data != null && data.AttrType == AttrType.DICTIONARY)
            {
                var datadic = data.AsDic;
                Message = datadic.Get(AttrKeyMaintenanceMessage).AsValue.ToString();
                Title = datadic.GetValue(AttrKeyMaintenanceTitle).AsValue.ToString();
            }
        }
    }
    
    
    public delegate void HttpRequestDelegate(HttpRequest req);
    
    public delegate void UsersDelegate(List<User> users, Error err);
    
    public delegate void ConfirmBackLinkDelegate(LinkConfirmDecision decision);
    
    public delegate void ConfirmLinkDelegate(ILink link, LinkConfirmType type, Attr data,ConfirmBackLinkDelegate cbk);
    
    public delegate void NewUserDelegate(Attr data);
    
    public delegate void NewLinkDelegate(ILink link);
    
    public delegate void LoginErrorDelegate(ErrorType error, string msg, Attr data);
    
    public delegate void UpgradeDelegate(GenericData data);
    
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
    };

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
         * meaning that the app version is not supported and should be updated
         */
        ForceUpgrade,
        
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
        InvalidSecurityToken
    }
    ;
    
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
    ;
    
    public enum LinkFilter
    {
        Auto,
        Normal,
        AutoAndNormal,
        All,
        None
    }
    ;
    
    public enum LinkState
    {
        Connected,
        Disconnected,
        Unknown
    }
    ;

    public enum UpgradeType
    {
        Suggested,
        Forced,
        None
    };
}