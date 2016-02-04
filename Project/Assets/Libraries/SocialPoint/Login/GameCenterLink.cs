using System;
using System.Collections.Generic;
using UnityEngine;

using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Social;

namespace SocialPoint.Login
{
    public class GameCenterLink : ILink
    {
        private IGameCenter _gameCenter;
        
        private event StateChangeDelegate _eventStateChange;
        
        public readonly static string LinkName = "gc";

        LinkState _state;

        public GameCenterLink()
        {
            _gameCenter = new UnityGameCenter();
            Init();
        }
        
        public GameCenterLink(IGameCenter gameCenter)
        {
            _gameCenter = gameCenter;
            Init();
        }
        
        void Init()
        {
            _state = _gameCenter.IsConnected ? LinkState.Connected : LinkState.Disconnected;
            _gameCenter.StateChangeEvent += OnStateChanged;
        }

        public void Dispose()
        {
            _gameCenter.StateChangeEvent -= OnStateChanged;
        }
        
        void GetUserIdsFromLinkData(Attr linkData, List<string> userIds)
        {
            if(linkData.AttrType == AttrType.VALUE)
            {
                userIds.Add(linkData.AsValue.ToString());
            }
            else if(linkData.AttrType == AttrType.LIST)
            {
                userIds.AddRange(linkData.AsList.ToList<string>());
            }
            else if(linkData.AttrType == AttrType.DICTIONARY)
            {
                GetUserIdsFromLinkData(linkData.AsDic.Get(Name), userIds);
            }
        }
        
        GameCenterUser GetGameCenterUser(User user)
        {
            List<string> userIds = user.GetExternalIds(LinkName);
            if(_gameCenter.User != null && userIds.Contains(_gameCenter.User.UserId))
            {
                return _gameCenter.User;
            }

            foreach(var friend in _gameCenter.Friends)
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
            
            if(_eventStateChange != null && _gameCenter != null && !_gameCenter.IsConnecting)
            {
                if(_gameCenter.IsConnected)
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
            _gameCenter.Login((err) => OnLogin(err, cbk));
        }

        void OnLogin(Error err, ErrorDelegate cbk)
        {
            if(!Error.IsNullOrEmpty(err) && err.Code == GameCenterErrors.LoginCancelled)
            {
                err = null;
            }

            if(cbk != null)
            {
                cbk(err);
            }
        }

        public void NotifyAppRequestRecipients(AppRequest req, ErrorDelegate cbk)
        {
            if(cbk != null)
            {
                cbk(null);
            }
        }
        
        public void UpdateUser(User user)
        {
            GameCenterUser gcUser = GetGameCenterUser(user);
            if(gcUser != null)
            {
                user.AddName(gcUser.Alias);
            }
        }
        
        public void UpdateLocalUser(LocalUser user)
        {
            if(_gameCenter.IsConnected && _gameCenter.User != null)
            {
                user.AddLink(_gameCenter.User.UserId, Name);
                user.AddName(_gameCenter.User.Alias, Name);
            }
        }
        
        public AttrDic GetLinkData()
        {
            GameCenterUser user = _gameCenter.User;
            AttrDic data = new AttrDic();
            data.SetValue("gc_external_id", user.UserId);
            data.SetValue("alias", user.Alias);
            string ageGroup = "unknown";
            switch(user.Age)
            {
            case GameCenterUser.AgeGroup.Underage:
                ageGroup = "underage";
                break;
            case GameCenterUser.AgeGroup.Adult:
                ageGroup = "adult";
                break;
            case GameCenterUser.AgeGroup.Unknown:
                ageGroup = "unknown";
                break;
            }
            data.SetValue("ageGroup", ageGroup);
            GameCenterUserVerification veri = user.Verification;
            if(!string.IsNullOrEmpty(veri.Url))
            {
                data.SetValue("gc_verification_url", veri.Url);
                data.SetValue("gc_verification_signature", Convert.ToBase64String(veri.Signature));
                data.SetValue("gc_verification_salt", Convert.ToBase64String(veri.Salt));
                data.SetValue("gc_verification_time", veri.Time.ToString());
            }
            else
            {
                data.SetValue("gc_verification_url", string.Empty);
                data.SetValue("gc_verification_signature", string.Empty);
                data.SetValue("gc_verification_salt", string.Empty);
                data.SetValue("gc_verification_time", string.Empty);
            }
            return data;
        }
        
        public void GetFriendsData(List<UserMapping> mappings)
        {
            foreach(var friend in _gameCenter.Friends)
            {
                mappings.Add(new UserMapping(friend.UserId, Name));
            }
        }
        
        public void UpdateUserPhoto(User user, uint photoSize, ErrorDelegate cbk)
        {
            List<string> userIds = user.GetExternalIds(LinkName);
            if(userIds.Count > 0 && !string.IsNullOrEmpty(userIds[0]))
            {
                string linkName = Name;
                _gameCenter.LoadPhoto(userIds[0], photoSize, (path, err) =>
                {
                    if(Error.IsNullOrEmpty(err))
                    {
                        user.AddPhotoPath(path, linkName);
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
            return GetGameCenterUser(user) != null;
        }
        
        public void Logout()
        {
            return;
        }
    }
}
