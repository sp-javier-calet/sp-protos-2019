using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.Social;
using SocialPoint.IO;

namespace SocialPoint.Login
{
    public class FacebookLink : ILink
    {
        private IFacebook _facebook;
        private bool _loginWithUi;

        private event StateChangeDelegate _eventStateChange;

        public readonly static string LinkName = "fb";

        LinkState _state;

        public FacebookLink(ICoroutineRunner runner, bool loginWithUi = true)
        {
            _loginWithUi = loginWithUi;
            _facebook = new UnityFacebook(runner);
            _state = _facebook.IsConnected ? LinkState.Connected : LinkState.Disconnected;
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

        public void Dispose()
        {
            _facebook.StateChangeEvent -= OnStateChanged;
        }

        FacebookUser GetFacebookUser(User user)
        {
            List<string> userIds = user.GetExternalIds(LinkName);
            if(_facebook.User != null && userIds.Contains(_facebook.User.UserId))
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

        public string Name
        {
            get
            {
                return LinkName;
            }
        }

        public LinkState State
        {
            get
            {
                return _state;
            }
        }

        public void AddStateChangeDelegate(StateChangeDelegate cbk)
        {
            _eventStateChange += cbk;
        }

        void OnStateChanged()
        {
            if(_eventStateChange != null && _facebook != null && !_facebook.IsConnecting)
            {
                if(_facebook.IsConnected)
                {
                    _state = LinkState.Connected;
                }
                else
                {
                    _state = LinkState.Disconnected;
                }
                _eventStateChange(_state);
            }
        }

        public void Login(ErrorDelegate cbk)
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

        public void NotifyAppRequestRecipients(AppRequest req, ErrorDelegate cbk)
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

        public void UpdateUser(User user)
        {
            FacebookUser fbUser = GetFacebookUser(user);
            if(fbUser != null)
            {
                user.AddName(fbUser.Name);
            }
        }

        public void UpdateLocalUser(LocalUser user)
        {
            if(_facebook.IsConnected && _facebook.User != null)
            {
                user.AddLink(_facebook.User.UserId, Name);
                user.AddName(_facebook.User.Name, Name);
            }
        }

        public AttrDic GetLinkData()
        {
            FacebookUser user = _facebook.User;
            AttrDic data = new AttrDic();
            data.SetValue("external_id", user.UserId);
            data.SetValue("fb_access_token", user.AccessToken);
            return data;
        }

        public void GetFriendsData(List<UserMapping> mappings)
        {
            foreach(var friend in _facebook.Friends)
            {
                mappings.Add(new UserMapping(friend.UserId, Name));
            }
        }

        public void UpdateUserPhoto(User user, uint photoSize, ErrorDelegate cbk)
        {
            List<string> userIds = user.GetExternalIds(LinkName);
            if(userIds.Count > 0 && userIds[0].Length > 0)
            {
                string linkName = Name;
                _facebook.LoadPhoto(userIds[0], (texture, err) => {
                    if(Error.IsNullOrEmpty(err))
                    {
                        var tmpFilePath = FileUtils.Combine(PathsManager.AppPersistentDataPath, "SPLoginFacebook/" + user.Id + "_" + photoSize.ToString() + ".png");
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

        public bool IsFriend(User user)
        {
            return GetFacebookUser(user) != null;
        }

        public void Logout()
        {
            _facebook.Logout(null);
        }

    }
}
