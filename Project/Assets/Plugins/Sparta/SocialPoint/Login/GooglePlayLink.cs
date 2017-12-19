using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Social;

namespace SocialPoint.Login
{
    public sealed class GooglePlayLink : ILink
    {
        public LinkState State
        {
            get
            {
                return _state;
            }
        }

        LinkState _state;

        readonly IGoogle _googlePlay;

        event StateChangeDelegate _eventStateChange;

        public readonly static string LinkName = "gp";
        public bool _loginSilent;

        public LinkMode Mode
        {
            get;
            private set;
        }

        public GooglePlayLink(IGoogle googlePlay, LinkMode mode, bool silent = false)
        {
            _googlePlay = googlePlay;
            Mode = mode;
            _loginSilent = silent;
            Init();
        }

        void Init()
        {
            _state = _googlePlay.IsConnected ? LinkState.Connected : LinkState.Disconnected;
            _googlePlay.StateChangeEvent += OnStateChanged;
        }

        void GetUserIdsFromLinkData(Attr linkData, ref List<string> userIds)
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
                GetUserIdsFromLinkData(linkData.AsDic.Get(Name), ref userIds);
            }
        }

        public string Name
        {
            get
            {
                return LinkName;
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
            _state = _googlePlay.IsConnected ? LinkState.Connected : LinkState.Disconnected;
            if(_eventStateChange != null && _googlePlay != null)
            {
                if(_googlePlay.IsConnected)
                {
                    _eventStateChange(_state);
                }
                else
                {
                    _eventStateChange(_state);
                }
            }
        }

        public void Login(ErrorDelegate cbk)
        {
            _googlePlay.Login(err => OnLogin(err, cbk), _loginSilent);
        }

        static void OnLogin(Error err, ErrorDelegate cbk)
        {
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
            if(_googlePlay.IsConnected && _googlePlay.User != null)
            {
                GoogleUser gUser = GetGooglePlayUser(user);
                if(gUser != null)
                {
                    user.AddName(gUser.Name);
                }
            }
        }

        public void UpdateLocalUser(LocalUser user)
        {
            if(_googlePlay.IsConnected && _googlePlay.User != null)
            {
                user.AddLink(_googlePlay.User.UserId, Name);
                user.AddName(_googlePlay.User.Name, Name);
            }
        }

        public AttrDic GetLinkData()
        {
            GoogleUser user = _googlePlay.User;
            var data = new AttrDic();

            if(!string.IsNullOrEmpty(user.UserId))
            {
                string accessToken = _googlePlay.AccessToken;
            
                data.SetValue("gp_external_id", user.UserId);
                data.SetValue("gp_user_name", user.Name);
                data.SetValue("gp_access_token", accessToken);
            }
            return data;
        }

        public void GetFriendsData(List<UserMapping> mappings)
        {
            if(_googlePlay.Friends == null)
            {
                return;
            }
            var enumerator = _googlePlay.Friends.GetEnumerator();
            while(enumerator.MoveNext())
            {
                mappings.Add(new UserMapping(enumerator.Current.UserId, Name));
            }
            enumerator.Dispose();
        }

        public void UpdateUserPhoto(User user, uint photoSize, ErrorDelegate cbk)
        {
            Log.e("Not Implementing Update User Photo Yet");
        }

        public bool IsFriend(User user)
        {
            Log.e("Not Implementing Is Friend Yet");
            return false;
        }

        public void Logout()
        {
            return;
        }

        GoogleUser GetGooglePlayUser(User user)
        {
            List<string> userIds = user.GetExternalIds(LinkName);
            if(userIds.Contains(_googlePlay.User.UserId))
            {
                return _googlePlay.User;
            }
            
            var itr = _googlePlay.Friends.GetEnumerator();
            while(itr.MoveNext())
            {
                GoogleUser friend = itr.Current;
                if(userIds.Contains(friend.UserId))
                {
                    itr.Dispose();
                    return friend;
                }
            }
            itr.Dispose();
            
            return null;
        }

        public void Dispose()
        {
            _googlePlay.StateChangeEvent -= OnStateChanged;
        }
    }
}