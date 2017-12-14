using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Social
{
    public sealed class EmptyFacebook : BaseFacebook
    {
        List<FacebookUser> _friends = new List<FacebookUser>();

        public override List<FacebookUser> Friends
        {
            get
            {
                return _friends;
            }
        }

        FacebookUser _user = new FacebookUser();

        public override FacebookUser User
        {
            get
            {
                return _user;
            }
        }

        List<string> _loginPermissions = new List<string>();

        public override List<string> LoginPermissions
        {
            get
            {
                return _loginPermissions;
            }
        }

        bool _isConnected;

        public override bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }

        public override bool IsConnecting
        {
            get
            {
                return false;
            }
        }

        public override string AppId
        {   
            get
            {
                return string.Empty;
            }
            set
            {
            }
        }

        public override string ApiVersion
        { 
            get
            {
                return string.Empty;
            }
            set
            {
            }
        }

        public override void SendAppRequest(FacebookAppRequest req, FacebookAppRequestDelegate callback = null)
        {
            if(callback != null)
            {
                callback(req, null);
            }
        }

        public override void PostOnWallWithDialog(FacebookWallPost post, FacebookWallPostDelegate callback = null)
        {
            if(callback != null)
            {
                callback(post, null);
            }
        }

        public override bool HasPermissions(IList<string> permissions)
        {
            return false;
        }

        public override void AskForPermissions(List<string> permissions, FacebookPermissionsDelegate callback = null)
        {
            if(callback != null)
            {
                callback(new List<string>(), null);
            }
        }

        public override void QueryGraph(FacebookGraphQuery req, FacebookGraphQueryDelegate callback = null)
        {
            if(callback != null)
            {
                callback(req, null);
            }
        }

        public override void Login(ErrorDelegate callback = null, bool withUi = true)
        {
            _isConnected = true;
            if(callback != null)
            {
                callback(null);
            }
            NotifyStateChanged();
        }

        public override void Logout(ErrorDelegate callback = null)
        {
            _isConnected = false;
            if(callback != null)
            {
                callback(null);
            }
            NotifyStateChanged();
        }

        public override void LoadPhoto(string userId, FacebookPhotoDelegate callback = null)
        {
            if(callback != null)
            {
                callback(null, null);
            }
        }

        public override void RefreshFriends(ErrorDelegate callback = null)
        {
            if(callback != null)
            {
                callback(null);
            }
        }

    }
}
