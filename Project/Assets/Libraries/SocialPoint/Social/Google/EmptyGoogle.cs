using SocialPoint.Base;
using System.Collections.Generic;

namespace SocialPoint.Social
{
    public class EmptyGoogle : IGoogle
    {
        #region IGoogle implementation

        public void Login(ErrorDelegate cbk)
        {
            if(cbk != null)
            {
                cbk(new Error("Empty Google implementation"));
            }
        }

        public void Logout(ErrorDelegate cbk)
        {
            if(cbk != null)
            {
                cbk(new Error("Empty Google implementation"));
            }
        }

        public void UpdateAchievement(GoogleAchievement achievement, GoogleAchievementDelegate cbk = null)
        {
            if(cbk != null)
            {
                cbk(achievement, new Error("Empty Google implementation"));
            }
        }

        public void ResetAchievement(GoogleAchievement achievement, GoogleAchievementDelegate cbk = null)
        {
            if(cbk != null)
            {
                cbk(achievement, new Error("Empty Google implementation"));
            }
        }

        public void ShowAchievementsUI()
        {
        }

        public void LoadLeaderboard(GoogleLeaderboard ldb, uint rowCount, GoogleLeaderboardDelegate cbk)
        {
            if(cbk != null)
            {
                cbk(ldb, new Error("Empty Google implementation"));
            }
        }

        public void UpdateLeaderboard(GoogleLeaderboard ldb, GoogleLeaderboardDelegate cbk = null)
        {
            if(cbk != null)
            {
                cbk(ldb, new Error("Empty Google implementation"));
            }
        }

        public void ShowLeaderboardsUI(string id = null)
        {
        }

        public void IncrementEvent(string id, uint quantity = 1)
        {
        }

        public void ShowViewQuestsUI(GoogleQuestEventDelegate cbk = null)
        {
            if(cbk != null)
            {
                cbk(GoogleQuestEvent.Empty, new Error("Empty Google implementation"));
            }
        }

        public GoogleUser User
        {
            get
            {
                return null;
            }
        }

        public bool IsConnected
        {
            get
            {
                return false;
            }
        }

        public IEnumerable<GoogleAchievement> Achievements
        {
            get
            {
                return null;
            }
        }

        #endregion
    }
}