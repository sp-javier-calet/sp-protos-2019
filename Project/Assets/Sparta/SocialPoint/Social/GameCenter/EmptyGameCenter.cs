using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Social
{
    public sealed class EmptyGameCenter : IGameCenter
    {
        bool _isConnected;
        GameCenterUser _user;
        List<GameCenterUser> _friends = new List<GameCenterUser>();

        public event Action StateChangeEvent;

        List<GameCenterAchievement> _achievements = new List<GameCenterAchievement>();

        public IEnumerable<GameCenterAchievement> Achievements
        {
            get
            {
                return _achievements;
            }
        }

        public EmptyGameCenter(string userName)
        {
            _user = new GameCenterUser(userName);
        }

        void NotifyStateChanged()
        { 
            if(StateChangeEvent != null)
            {
                StateChangeEvent();
            }
        }

        #region implemented abstract members of IGameCenter

        public void UpdateScore(GameCenterScore score, GameCenterScoreDelegate cbk = null)
        {
            if(cbk != null)
            {
                cbk(score, null);
            }
        }

        public void UpdateAchievement(GameCenterAchievement achievement, GameCenterAchievementDelegate cbk = null)
        {
            if(cbk != null)
            {
                cbk(achievement, null);
            }
        }

        public void ResetAchievements(ErrorDelegate cbk = null)
        {
            if(cbk != null)
            {
                cbk(null);
            }
        }

        public void Login(ErrorDelegate cbk = null)
        {
            _isConnected = true;
            if(cbk != null)
            {
                cbk(null);
            }
            NotifyStateChanged();
        }

        public void LoadPhoto(string playerId, uint size, GameCenterPhotoDelegate cbk)
        {
            if(cbk != null)
            {
                cbk(null, null);
            }
        }

        public GameCenterUser User
        {
            get
            {
                return _user;
            }
        }

        public List<GameCenterUser> Friends
        {
            get
            {
                return _friends;
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

        public bool IsAchievementUpdating(string achiId)
        {
            return false;
        }

        public void ShowAchievementsUI()
        {
        }

        public void ShowLeaderboardUI(string id = null)
        {
        }

        
        #endregion
    }
}
