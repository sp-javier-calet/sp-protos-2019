using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Social
{
    public delegate void GameCenterScoreDelegate(GameCenterScore score, Error err);
    
    public delegate void GameCenterAchievementDelegate(GameCenterAchievement achi, Error err);
    
    public delegate void GameCenterPhotoDelegate(string path, Error err);


    public static class GameCenterErrors
    {
        public const int LoginCancelled = 2;
    }

    public interface IGameCenter
    {
        event Action StateChangeEvent;

        /**
         Update a score
         @param score the score info to send
         @param callback called when the request was sent
         */
        void UpdateScore(GameCenterScore score, GameCenterScoreDelegate cbk);
        
        /**
         Update an achievement
         @param achievement 
         @param callback called when the request was sent
         */
        void UpdateAchievement(GameCenterAchievement achievement, GameCenterAchievementDelegate cbk);
        
        /**
         Remove all achievements
         @param callback called when the request was sent
         */
        void ResetAchievements(ErrorDelegate cbk);
        
        /**
         Start the facebook login
         @param callback - called when login is finished
         */
        void Login(ErrorDelegate cbk);
        
        /**
         Load a photo of a player
         */
        void LoadPhoto(string playerId, uint size, GameCenterPhotoDelegate cbk);

        /**
         @return The the user logged in
         */
        GameCenterUser User{ get; }
        
        /**
         @return a set of friends retreived by the connection
         */
        List<GameCenterUser> Friends{ get; }
        
        /**
         * @return if game center is logged in
         */
        bool IsConnected{ get; }
        
        /**
         * @return if game center is logging in
         */
        bool IsConnecting{ get; }
        
        /**
         * @return if game center has an error
         */
        bool HasError{ get; }
    }
}
