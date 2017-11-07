using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Social;

namespace SocialPoint.Login
{
    public sealed class GameCenterLink : ILink
    {
        IGameCenter _gameCenter;

        event StateChangeDelegate _eventStateChange;

        public readonly static string LinkName = "gc";

        LinkState _state;
        public LinkMode Mode
        {
            get;
            private set;
        }

        public GameCenterLink(IGameCenter gameCenter, LinkMode mode)
        {
            _gameCenter = gameCenter;
            Mode = mode;
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

            for(int i = 0, _gameCenterFriendsCount = _gameCenter.Friends.Count; i < _gameCenterFriendsCount; i++)
            {
                var friend = _gameCenter.Friends[i];
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

        public void ClearStateChangeDelegate()
        {
            _eventStateChange = null;
        }

        void OnStateChanged()
        {
            
            if(_eventStateChange != null && _gameCenter != null && !_gameCenter.IsConnecting)
            {
                _state = _gameCenter.IsConnected ? LinkState.Connected : LinkState.Disconnected;
                _eventStateChange(_state);
            }
        }

        public void Login(ErrorDelegate cbk)
        {            
            _gameCenter.Login(err => OnLogin(err, cbk));
        }

        static void OnLogin(Error err, ErrorDelegate cbk)
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
            var data = new AttrDic();
            if(!string.IsNullOrEmpty(user.UserId))
            {
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
                bool validVerification = veri != null && !string.IsNullOrEmpty(veri.Url);

                data.SetValue("gc_verification_url", validVerification ? veri.Url : string.Empty);
                data.SetValue("gc_verification_signature", validVerification ? Convert.ToBase64String(veri.Signature) : string.Empty);
                data.SetValue("gc_verification_salt", validVerification ? Convert.ToBase64String(veri.Salt) : string.Empty);
                data.SetValue("gc_verification_time", validVerification ? veri.Time.ToString() : string.Empty);
            }
            return data;
        }

        public void GetFriendsData(List<UserMapping> mappings)
        {
            for(int i = 0, _gameCenterFriendsCount = _gameCenter.Friends.Count; i < _gameCenterFriendsCount; i++)
            {
                var friend = _gameCenter.Friends[i];
                mappings.Add(new UserMapping(friend.UserId, Name));
            }
        }

        public void UpdateUserPhoto(User user, uint photoSize, ErrorDelegate cbk)
        {
            List<string> userIds = user.GetExternalIds(LinkName);
            if(userIds.Count > 0 && !string.IsNullOrEmpty(userIds[0]))
            {
                string linkName = Name;
                _gameCenter.LoadPhoto(userIds[0], photoSize, (path, err) => {
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
