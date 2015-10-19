using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Social
{
    public class EmptyGameCenter : BaseGameCenter
    {        
        private bool _isConnected = false;
        private GameCenterUser _user;
        private List<GameCenterUser> _friends = new List<GameCenterUser>();
        
        public EmptyGameCenter(string userName)
        {
            _user = new GameCenterUser(userName);
        }
        
        #region implemented abstract members of IGameCenter
        
        public override void UpdateScore(GameCenterScore score, GameCenterScoreDelegate cbk)
        {
            if(cbk != null)
            {
                cbk(score, null);
            }
        }
        
        public override void UpdateAchievement(GameCenterAchievement achievement, GameCenterAchievementDelegate cbk)
        {
            if(cbk != null)
            {
                cbk(achievement, null);
            }
        }
        
        public override void ResetAchievements(ErrorDelegate cbk)
        {
            if(cbk != null)
            {
                cbk(null);
            }
        }
        
        public override void Login(ErrorDelegate cbk)
        {
            _isConnected = true;
            if(cbk != null)
            {
                cbk(null);
            }
            NotifyStateChanged();
        }
        
        public override void LoadPhoto(string playerId, uint size, GameCenterPhotoDelegate cbk)
        {
            if(cbk != null)
            {
                cbk(null, null);
            }
        }
        
        public override GameCenterUser User
        {
            get
            {
                return _user;
            }
        }
        
        public override List<GameCenterUser> Friends
        {
            get
            {
                return _friends;
            }
        }
        
        public override bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }
        
        public override bool IsConnecting
        {
            get
            {
                return false;
            }
        }

        
        #endregion
    }
}
