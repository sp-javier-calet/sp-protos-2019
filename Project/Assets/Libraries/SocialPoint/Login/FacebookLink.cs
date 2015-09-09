using System;
using System.Collections.Generic;
using UnityEngine;

using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.Social;

namespace SocialPoint.Login
{
    public class FacebookLink : ILink
    {
        private IFacebook _facebook;
        private bool _loginWithUi;
        
        private event StateChangeDelegate _eventStateChange;
        
        readonly static string LinkName = "fb";
        
        public FacebookLink(MonoBehaviour behaviour, bool loginWithUi = true)
        {
            _loginWithUi = loginWithUi;
            _facebook = new PlatformFacebook(behaviour);
            Init();
        }
        
        public FacebookLink(IFacebook facebook, bool loginWithUi = true)
        {
            _loginWithUi = loginWithUi;
            _facebook = facebook;
            Init();
        }
        
        void Init()
        {
            _facebook.StateChangeEvent += OnStateChanged;
        }
        
        FacebookUser GetFacebookUser(User user)
        {
            List<string> userIds = user.GetExternalIds(LinkName);
            if(userIds.Contains(_facebook.User.UserId))
            {
                return _facebook.User;
            }

            foreach(var friend in _facebook.Friends)
            {
                if(userIds.Contains(friend.UserId))
                {
                    return friend;
                }
            }
            
            return null;
        }
        
        public override string Name
        {
            get
            {
                return LinkName;
            }
        }
        
        public override void OnNewLocalUser(LocalUser user)
        {
            base.OnNewLocalUser(user);
        }
        
        public override void AddStateChangeDelegate(StateChangeDelegate cbk)
        {
            _eventStateChange += cbk;
        }
        
        void OnStateChanged()
        {
            
            if(_eventStateChange != null && _facebook != null && !_facebook.IsConnecting)
            {
                if(_facebook.IsConnected)
                {
                    _eventStateChange(LinkState.Connected);
                }
                else
                {
                    _eventStateChange(LinkState.Disconnected);
                }
            }
        }
        
        public override void Login(ErrorDelegate cbk)
        {            
            _facebook.Login((err) => OnLogin(err, cbk), _loginWithUi);
        }

        void OnLogin(Error err, ErrorDelegate cbk)
        {            
            if(!Error.IsNullOrEmpty(err) && err.Code == FacebookErrors.LoginNeedsUI)
            {
                err = null;
            }
            if(cbk != null)
            {
                cbk(err);
            }
        }

        public void Logout(ErrorDelegate cbk)
        {
            _facebook.Logout((err) => {
                if(cbk != null)
                {
                    cbk(err);
                }
            });
        }
        
        public override void NotifyAppRequestRecipients(AppRequest req, ErrorDelegate cbk)
        {
            FacebookAppRequest fbReq = new FacebookAppRequest(req.Description);
            fbReq.FrictionLess = true;
            fbReq.Title = req.Title;
            
            List<string> userIds = new List<string>();
            foreach(var recipient in req.Recipients)
            {
                userIds.AddRange(recipient.GetExternalIds(LinkName));
            }
            
            if(userIds.Count > 0)
            {
                fbReq.To = userIds;
                _facebook.SendAppRequest(fbReq, (appReq, err) => cbk(err));
            }
            else
            {
                if(cbk != null)
                {
                    cbk(null);
                }
            }
        }
        
        public override void UpdateUser(User user)
        {
            FacebookUser fbUser = GetFacebookUser(user);
            if(fbUser != null)
            {
                user.AddName(fbUser.Name);
            }
        }
        
        public override void UpdateLocalUser(LocalUser user)
        {
            if(_facebook.IsConnected && _facebook.User != null)
            {
                user.AddLink(_facebook.User.UserId, Name);
                user.AddName(_facebook.User.Name, Name);
            }
        }
        
        public override AttrDic GetLinkData()
        {
            FacebookUser user = _facebook.User;
            AttrDic data = new AttrDic();
            data.SetValue("external_id", user.UserId);
            data.SetValue("fb_access_token", user.AccessToken);
            return data;
        }
        
        public override void GetFriendsData(List<UserMapping> mappings)
        {
            foreach(var friend in _facebook.Friends)
            {
                mappings.Add(new UserMapping(friend.UserId, Name));
            }
        }
        
        public override void UpdateUserPhoto(User user, uint photoSize, ErrorDelegate cbk)
        {
            List<string> userIds = user.GetExternalIds(LinkName);
            if(userIds.Count > 0 && userIds[0].Length > 0)
            {
                string linkName = Name;
                _facebook.LoadPhoto(userIds[0], (texture, err) =>
                {
                    if(Error.IsNullOrEmpty(err))
                    {
                        string tmpFilePath = Application.temporaryCachePath + "/SPLoginFacebook/" + user.Id + "_" + photoSize.ToString() + ".png";
                        err = ImageUtils.SaveTextureToFile(texture, tmpFilePath);
                        if(Error.IsNullOrEmpty(err))
                        {
                            user.AddPhotoPath(tmpFilePath, linkName);
                        }
                        
                    }
                    if(cbk != null)
                    {
                        cbk(err);
                    }
                });
            }
            else
            {
                if(cbk != null)
                {
                    cbk(null);
                }
            }
        }
        
        public override bool IsFriend(User user)
        {
            return GetFacebookUser(user) != null;
        }
        
        public override void Logout()
        {
            _facebook.Logout(null);
        }
        
    }
}
