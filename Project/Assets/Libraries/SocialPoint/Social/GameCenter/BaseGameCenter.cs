using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Social
{

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
        ;
        
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

        public abstract bool HasError{ get; }
    }
}
