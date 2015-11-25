using System;
using System.Collections.Generic;
using GooglePlayGames;
using SocialPoint.Base;

namespace SocialPoint.Social
{
    public class UnityGoogle : BaseGoogle
    {

        GooglePlayUser _user;
        PlayGamesPlatform _platform;

        #region IGoogle implementation

        public override void Login(ErrorDelegate cbk)
        {
            if(IsConnected)
            {
                if(cbk != null)
                {
                    cbk(null);
                }
                return;
            }
            _user = new GooglePlayUser();
            _platform = PlayGamesPlatform.Activate();

            // TODO
        }

        public override void Logout(ErrorDelegate cbk)
        {
        }

        public override void UpdateAchievement(GooglePlayAchievement achievement, GooglePlayAchievementDelegate cbk)
        {
        }

        public override void ResetAchievements(ErrorDelegate cbk)
        {
        }

        public override GooglePlayUser User
        {
            get
            {
                return _user;
            }
        }

        public override bool IsConnected
        {
            get
            {
                return _platform != null && _platform.IsAuthenticated();
            }
        }

        public override bool IsConnecting
        {
            get
            {
                return false;
            }
        }

        #endregion
    }
}
