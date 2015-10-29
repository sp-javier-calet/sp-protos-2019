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

    public class GameCenterUserVerification
    {
        public string Url { get; private set; }
        
        public byte[] Signature { get; private set; }
        
        public byte[] Salt { get; private set; }
        
        public UInt64 Time { get; private set; }
        
        public override string ToString()
        {
            return string.Format("[GameCenterUserVerification: Url={0}, Signature={1}, Salt={2}, Time={3}]", Url, Signature.ToString(), Salt.ToString(), Time.ToString());
        }
        
        public GameCenterUserVerification()
        {
        }
        
        public GameCenterUserVerification(string url, byte[] signature, byte[] salt, UInt64 time)
        {
            Url = url;
            Signature = signature;
            Salt = salt;
            Time = time;
        }
    }
    
    public class GameCenterUser
    {
        public enum AgeGroup
        {
            Unknown,
            Underage,
            Adult
        }
        
        public string UserId { get; private set; }
        
        public string Alias { get; private set; }
        
        public string DisplayName { get; private set; }
        
        public AgeGroup Age { get; private set; }
        
        public GameCenterUserVerification Verification { get; set; }
        
        public GameCenterUser(string id = "", string alias = "", string displayName = "", AgeGroup age = AgeGroup.Unknown)
        {
            UserId = id;
            Alias = alias;
            DisplayName = displayName;
            Age = age;
            Verification = new GameCenterUserVerification();
        }
        
        public static bool operator ==(GameCenterUser lu, GameCenterUser ru)
        {
            if(System.Object.ReferenceEquals(lu, null))
            {
                if(System.Object.ReferenceEquals(ru, null))
                {
                    return true;
                }
                return false;
            }
            else if(System.Object.ReferenceEquals(ru, null))
            {
                return false;
            }
            
            return (lu.UserId == ru.UserId);
        }
        
        public static bool operator !=(GameCenterUser lu, GameCenterUser ru)
        {
            return !(lu == ru);
        }
        
        public override bool Equals(System.Object obj)
        {
            if(obj == null)
            {
                return false;
            }
            
            GameCenterUser p = obj as GameCenterUser;
            if((System.Object)p == null)
            {
                return false;
            }
            
            return this == p;
        }
        
        public override int GetHashCode()
        {
            return 0;
        }

    }
    
    public class GameCenterScore
    {        
        public string Category { get; private set; }
        
        public Int64 Value { get; private set; }
        
        public GameCenterScore(string category, Int64 value)
        {
            Category = category;
            Value = value;
        }
    }
    
    public class GameCenterAchievement
    {        
        public string Id { get; private set; }
        
        public Double Percent { get; private set; }
        
        public GameCenterAchievement(string id, Double percent)
        {
            Id = id;
            Percent = percent;
        }
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

    }
}
