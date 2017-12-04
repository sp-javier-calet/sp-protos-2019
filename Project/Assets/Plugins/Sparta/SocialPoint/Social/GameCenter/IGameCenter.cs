using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Social
{
    public delegate void GameCenterScoreDelegate(GameCenterScore score, Error err);
    
    public delegate void GameCenterAchievementDelegate(GameCenterAchievement achi, Error err);
    
    public delegate void GameCenterPhotoDelegate(string path, Error err);


    public static class GameCenterErrors
    {
        public const int LoginCancelled = 2;
    }

    public sealed class GameCenterUserVerification : ICloneable
    {
        public string Url { get; private set; }

        public byte[] Signature { get; private set; }

        public byte[] Salt { get; private set; }

        public UInt64 Time { get; private set; }

        public override string ToString()
        {
            return string.Format("[GameCenterUserVerification: Url={0}, Signature={1}, Salt={2}, Time={3}]", Url, Signature, Salt, Time);
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

        public object Clone()
        {
            return new GameCenterUserVerification(Url, Signature, Salt, Time);
        }
    }

    public sealed class GameCenterUser : ICloneable
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

        public object Clone()
        {
            var user = new GameCenterUser(UserId, Alias, DisplayName, Age);
            user.Verification = (GameCenterUserVerification)Verification.Clone();
            return user;
        }

        public static bool operator ==(GameCenterUser lu, GameCenterUser ru)
        {
            if(Object.ReferenceEquals(lu, null))
            {
                return Object.ReferenceEquals(ru, null);
            }
            return !Object.ReferenceEquals(ru, null) && (lu.UserId == ru.UserId);
            
        }

        public static bool operator !=(GameCenterUser lu, GameCenterUser ru)
        {
            return !(lu == ru);
        }

        public override bool Equals(Object obj)
        {
            if(obj == null)
            {
                return false;
            }
            
            var p = obj as GameCenterUser;
            if((Object)p == null)
            {
                return false;
            }
            
            return this == p;
        }

        public override int GetHashCode()
        {
            return UserId.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[GameCenterUser: UserId={0}, Alias={1}, DisplayName={2}, Age={3}, Verification={4}]", UserId, Alias, DisplayName, Age, Verification);
        }
    }

    public sealed class GameCenterScore : ICloneable
    {
        public string Category { get; private set; }

        public Int64 Value { get; private set; }

        public GameCenterScore(string category, Int64 value)
        {
            Category = category;
            Value = value;
        }

        public object Clone()
        {
            return new GameCenterScore(Category, Value);
        }

        public override string ToString()
        {
            return string.Format("[GameCenterScore: Category={0}, Value={1}]", Category, Value);
        }
    }

    public sealed class GameCenterAchievement : ICloneable
    {
        public string Id { get; private set; }

        /**
         Completion percent for this achievement. Percent must be in range [0,100]
         */
        public float Percent { get; set; }

        public int Points { get; private set; }

        public bool Hidden { get; private set; }

        public string Title { get; private set; }

        public string AchievedDescription { get; private set; }

        public string UnachievedDescription { get; private set; }

        public bool IsUnlocked
        {
            get
            {
                return Percent >= 100;
            }
        }

        public GameCenterAchievement(string id, float percent)
        {
            Id = id;
            Percent = percent;
            Hidden = false;
            Points = 0;
            Title = string.Empty;
            UnachievedDescription = string.Empty;
            AchievedDescription = string.Empty;
        }

        public GameCenterAchievement(string id, float percent, int points, bool hidden, string title, string noDesc, string yesDesc)
        {
            Id = id;
            Percent = percent;
            Points = points;
            Hidden = hidden;
            Title = title;
            UnachievedDescription = noDesc;
            AchievedDescription = yesDesc;
        }

        public object Clone()
        {
            return new GameCenterAchievement(Id, Percent, Points, Hidden, Title, UnachievedDescription, AchievedDescription);
        }

        public override string ToString()
        {
            return string.Format("[GameCenterAchievement: Id={0}, Percent={1}, Points={2}, Hidden={3}, Title={4}, AchievedDescription={5}, IsUnlocked={6}]", Id, Percent, Points, Hidden, Title, AchievedDescription, IsUnlocked);
        }
    }

    public interface IGameCenter
    {
        event Action StateChangeEvent;

        IEnumerable<GameCenterAchievement> Achievements { get; }

        /**
         Update a score
         @param score the score info to send
         @param callback called when the request was sent
         */
        void UpdateScore(GameCenterScore score, GameCenterScoreDelegate cbk = null);

        /**
         Update an achievement
         @param achievement 
         @param callback called when the request was sent
         */
        void UpdateAchievement(GameCenterAchievement achievement, GameCenterAchievementDelegate cbk = null);

        /**
         Remove all achievements

         Note that after reseting achievements, UpdateAchievement() calls might start failing (it does in Unity 5.3.3) 
         If this happens you need login again to GameCenter restarting the app.
         
         @param callback called when the request was sent
         */
        void ResetAchievements(ErrorDelegate cbk = null);

        /**
         Start the game center login
         @param callback - called when login is finished
         */
        void Login(ErrorDelegate cbk = null);

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
         * @return if the achievement is beeing updated
         */
        bool IsAchievementUpdating(string achiId);

        /// <summary>
        /// Show native Achievements view
        /// </summary>
        void ShowAchievementsUI();

        /// <summary>
        /// Show native Leaderboards view
        /// </summary>
        void ShowLeaderboardUI(string id = null);

    }

}
