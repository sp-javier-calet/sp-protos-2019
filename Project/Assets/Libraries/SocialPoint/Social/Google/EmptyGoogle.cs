using SocialPoint.Base;

namespace SocialPoint.Social
{
    public class EmptyGoogle : BaseGoogle
    {
        #region IGoogle implementation

        public override void Login(ErrorDelegate cbk)
        {
            // TODO
        }

        public override void Logout(ErrorDelegate cbk)
        {
        }

        public override void UpdateAchievement(GoogleAchievement achievement, GoogleAchievementDelegate cbk)
        {
        }

        public override void ShowAchievementsUI()
        {
        }

        public override GoogleUser User
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public override bool IsConnected
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public override bool IsConnecting
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }


        #endregion
    }
}