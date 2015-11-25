using SocialPoint.Base;

namespace SocialPoint.Social
{
    public abstract class BaseGoogle : IGoogle
    {
        public abstract void Login(ErrorDelegate cbk);

        public abstract void Logout(ErrorDelegate cbk);

        public abstract void UpdateAchievement(GooglePlayAchievement achievement, GooglePlayAchievementDelegate cbk);

        public abstract void ResetAchievements(ErrorDelegate cbk);

        public abstract GooglePlayUser User{ get; }

        public abstract bool IsConnected{ get; }

        public abstract bool IsConnecting{ get; }
    }
}
