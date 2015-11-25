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