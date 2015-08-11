using System;
using System.Collections.Generic;

using SocialPoint.Attributes;
using SocialPoint.Utils;

namespace SocialPoint.Login
{
    public delegate void StateChangeDelegate(LinkState state);

    public delegate void ResultDelegate(Error err);

    public abstract class ILink
    {
        public abstract string Name{ get; }

        public virtual void OnNewLocalUser(LocalUser user)
        {
            UpdateLocalUser(user);
        }

        public abstract void AddStateChangeDelegate(StateChangeDelegate cbk);

        public abstract void Login(ResultDelegate cbk);

        public abstract void Logout();

        public abstract void NotifyAppRequestRecipients(AppRequest req, ResultDelegate cbk);

        public abstract void UpdateUser(User user);

        public abstract void UpdateLocalUser(LocalUser user);

        public abstract AttrDic GetLinkData();

        public abstract void GetFriendsData(ref List<UserMapping> mappings);

        public abstract void UpdateUserPhoto(User user, uint photoSize, ResultDelegate cbk);

        public abstract bool IsFriend(User user);
    }
}