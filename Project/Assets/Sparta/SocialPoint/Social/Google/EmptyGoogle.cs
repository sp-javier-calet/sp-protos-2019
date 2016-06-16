using SocialPoint.Base;
using System.Collections.Generic;

namespace SocialPoint.Social
{
    public class EmptyGoogle : IGoogle
    {
        private bool _isConnected = false;
        private GoogleUser _user;
        private List<GoogleUser> _friends = new List<GoogleUser>();

        #region IGoogle implementation

        public EmptyGoogle()
        {
            _user = new GoogleUser("DebugGoogleUserID", "DebugGoogleUserName");
        }

        public event GoogleStateChangeDelegate StateChangeEvent;
		public event TrackEventDelegate TrackEvent;

        public void Login(ErrorDelegate cbk, bool silent = false)
        {
            _isConnected = true;
            if(cbk != null)
            {
                cbk(new Error("Empty Google implementation"));
            }
            if(StateChangeEvent != null)
            {
                StateChangeEvent();
            }
        }

        public void Logout(ErrorDelegate cbk)
        {
            _isConnected = false;
            if(cbk != null)
            {
                cbk(new Error("Empty Google implementation"));
            }
            if(StateChangeEvent != null)
            {
                StateChangeEvent();
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
                return _user;
            }
        }

        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }

        public bool IsConnecting
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

        public string AccessToken
        {
            get
            {
                return string.Empty;
            }
        }

        public UnityEngine.Texture2D GetUserPhoto(string userID)
        {
            throw new System.NotImplementedException();
        }

        public List<GoogleUser> Friends
        {
            get
            {
                return _friends;
            }
        }
        #endregion
    }
}