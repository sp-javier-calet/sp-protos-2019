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

        public override void UpdateAchievement(GoogleAchievement achievement, GoogleAchievementDelegate cbk = null)
        {
        }

        public override void ResetAchievement(GoogleAchievement achievement, GoogleAchievementDelegate cbk = null)
        {
        }

        public override void ShowAchievementsUI()
        {
        }

        public override void LoadLeaderboard(GoogleLeaderboard ldb, uint rowCount, GoogleLeaderboardDelegate cbk)
        {
        }

        public override void UpdateLeaderboard(GoogleLeaderboard ldb, GoogleLeaderboardDelegate cbk = null)
        {
        }

        public override void ShowLeaderboardsUI(string id = null)
        {
        }

        public override void IncrementEvent(string id, uint quantity = 1)
        {
        }

        public override void ShowViewQuestsUI(GoogleQuestEventDelegate cbk = null)
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