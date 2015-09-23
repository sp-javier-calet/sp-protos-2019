using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Social
{   
    
    public abstract class BaseGameCenter : IGameCenter
    {
        public event Action StateChangeEvent;
        
        protected void NotifyStateChanged()
        { 
            if(StateChangeEvent != null)
            {
                StateChangeEvent();
            }
        }

        public abstract void UpdateScore(GameCenterScore score, GameCenterScoreDelegate cbk);
        public abstract void UpdateAchievement(GameCenterAchievement achievement, GameCenterAchievementDelegate cbk);
        public abstract void ResetAchievements(ErrorDelegate cbk);
        public abstract void Login(ErrorDelegate cbk);
        public abstract void LoadPhoto(string playerId, uint size, GameCenterPhotoDelegate cbk);
        public abstract GameCenterUser User{ get; }
        public abstract List<GameCenterUser> Friends{ get; }
        public abstract bool IsConnected{ get; }
        public abstract bool IsConnecting{ get; }
    }
}
