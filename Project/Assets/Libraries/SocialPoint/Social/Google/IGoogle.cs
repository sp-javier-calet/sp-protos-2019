using SocialPoint.Base;

namespace SocialPoint.Social
{
    public delegate void GooglePlayAchievementDelegate(GooglePlayAchievement achi,Error err);

    public class GooglePlayUser
    {
    }

    public class GooglePlayAchievement
    {
        string Id;
    }

    public interface IGoogle
    {
        void Login(ErrorDelegate cbk);

        void Logout(ErrorDelegate cbk);

        // Achievements

        void UpdateAchievement(GooglePlayAchievement achievement, GooglePlayAchievementDelegate cbk);

        void ResetAchievements(ErrorDelegate cbk);

        GooglePlayUser User{ get; }

        /**
         * @return if game center is logged in
         */
        bool IsConnected{ get; }

        /**
         * @return if game center is logging in
         */
        bool IsConnecting{ get; }
    }
}