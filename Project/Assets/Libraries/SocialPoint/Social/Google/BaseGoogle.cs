using SocialPoint.Base;

namespace SocialPoint.Social
{
    public abstract class BaseGoogle : IGoogle
    {
        public abstract void Login(ErrorDelegate cbk);

        public abstract void Logout(ErrorDelegate cbk);

        public abstract void UpdateAchievement(GoogleAchievement achievement, GoogleAchievementDelegate cbk = null);

        public abstract GoogleUser User{ get; }

        public abstract bool IsConnected{ get; }

        public abstract bool IsConnecting{ get; }

        public abstract void ShowAchievementsUI();

        public virtual System.Collections.Generic.IEnumerable<GoogleAchievement> Achievements
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
