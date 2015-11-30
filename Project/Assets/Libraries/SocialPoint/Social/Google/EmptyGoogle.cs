using SocialPoint.Base;

namespace SocialPoint.Social
{
    public class EmptyGoogle : BaseGoogle
    {
        #region IGoogle implementation

        public override void Login(ErrorDelegate cbk)
        {
            if(cbk != null)
            {
                cbk(new Error("Empty Google implementation"));
            }
        }

        public override void Logout(ErrorDelegate cbk)
        {
            if(cbk != null)
            {
                cbk(new Error("Empty Google implementation"));
            }
        }

        public override void UpdateAchievement(GoogleAchievement achievement, GoogleAchievementDelegate cbk = null)
        {
            if(cbk != null)
            {
                cbk(achievement, new Error("Empty Google implementation"));
            }
        }

        public override void ResetAchievement(GoogleAchievement achievement, GoogleAchievementDelegate cbk = null)
        {
            if(cbk != null)
            {
                cbk(achievement, new Error("Empty Google implementation"));
            }
        }

        public override void ShowAchievementsUI()
        {
        }

        public override void LoadLeaderboard(GoogleLeaderboard ldb, uint rowCount, GoogleLeaderboardDelegate cbk)
        {
            if(cbk != null)
            {
                cbk(ldb, new Error("Empty Google implementation"));
            }
        }

        public override void UpdateLeaderboard(GoogleLeaderboard ldb, GoogleLeaderboardDelegate cbk = null)
        {
            if(cbk != null)
            {
                cbk(ldb, new Error("Empty Google implementation"));
            }
        }

        public override void ShowLeaderboardsUI(string id = null)
        {
        }

        public override void IncrementEvent(string id, uint quantity = 1)
        {
        }

        public override void ShowViewQuestsUI(GoogleQuestEventDelegate cbk = null)
        {
            if(cbk != null)
            {
                cbk(GoogleQuestEvent.Empty, new Error("Empty Google implementation"));
            }
        }

        public override GoogleUser User
        {
            get
            {
                return null;
            }
        }

        public override bool IsConnected
        {
            get
            {
                return false;
            }
        }


        #endregion
    }
}