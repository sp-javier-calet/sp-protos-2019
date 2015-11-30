using SocialPoint.Base;

namespace SocialPoint.Social
{
    public abstract class BaseGoogle : IGoogle
    {
        public abstract void Login(ErrorDelegate cbk);

        public abstract void Logout(ErrorDelegate cbk);

        public abstract void UpdateAchievement(GoogleAchievement achievement, GoogleAchievementDelegate cbk = null);

        public abstract void ResetAchievement(GoogleAchievement achi, GoogleAchievementDelegate cbk = null);

        public abstract GoogleUser User{ get; }

        public abstract bool IsConnected{ get; }

        public abstract bool IsConnecting{ get; }

        public abstract void ShowAchievementsUI();

        public abstract void LoadLeaderboard(GoogleLeaderboard ldb, uint rowCount, GoogleLeaderboardDelegate cbk);

        public abstract void UpdateLeaderboard(GoogleLeaderboard ldb, GoogleLeaderboardDelegate cbk = null);

        public abstract void ShowLeaderboardsUI(string id = null);

        public abstract void IncrementEvent(string id, uint quantity = 1);

        public abstract void ShowViewQuestsUI(GoogleQuestEventDelegate cbk = null);

        public virtual System.Collections.Generic.IEnumerable<GoogleAchievement> Achievements
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
