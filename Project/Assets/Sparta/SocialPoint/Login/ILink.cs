using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.Login
{
    public delegate void StateChangeDelegate(LinkState state);

    public interface ILink : IDisposable
    {
        string Name{ get; }

        LinkState State{ get; }

        LinkMode Mode{ get; }

        void AddStateChangeDelegate(StateChangeDelegate cbk);

        void ClearStateChangeDelegate();

        void Login(ErrorDelegate cbk);

        void Logout();

        void NotifyAppRequestRecipients(AppRequest req, ErrorDelegate cbk);

        void UpdateUser(User user);

        void UpdateLocalUser(LocalUser user);

        AttrDic GetLinkData();

        void GetFriendsData(List<UserMapping> mappings);

        void UpdateUserPhoto(User user, uint photoSize, ErrorDelegate cbk);

        bool IsFriend(User user);
    }

    public static class LinkExtensions
    {
        public static void OnNewLocalUser(this ILink link, LocalUser user)
        {
            link.UpdateLocalUser(user);
        }
    }
}
