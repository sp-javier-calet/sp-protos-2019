using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Social
{
    public delegate void GoogleAchievementDelegate(GoogleAchievement achi,Error err);



    public class GoogleUser
    {
        public enum AgeGroup
        {
            Unknown,
            Underage,
            Adult
        }

        public string UserId { get; private set; }

        public string Name { get; private set; }

        public string PhotoUrl { get; private set; }

        public AgeGroup Age { get; private set; }

        public GoogleUser(string id = "", string displayName = "", string photoUrl = "", AgeGroup age = AgeGroup.Unknown)
        {
            UserId = id;
            Name = displayName;
            PhotoUrl = photoUrl;
            Age = age;
        }
    }

    public class GoogleAchievement
    {
        public string Id { get; private set; }

        public int CurrentSteps { get; set; }

        public int TotalSteps { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public bool IsIncremental { get; private set; }

        public bool IsUnlocked { get; private set; }

        public GoogleAchievement(string id, int currentSteps)
        {
            Id = id;
            CurrentSteps = currentSteps;
        }

        public void SetInfo(string name, string description, bool unlocked, int steps, bool incremental)
        {
            Name = name;
            Description = description;
            IsUnlocked = unlocked;
            TotalSteps = steps;
            IsIncremental = incremental;
        }
    }

    public interface IGoogle
    {
        // Login

        void Login(ErrorDelegate cbk);

        void Logout(ErrorDelegate cbk);

        GoogleUser User{ get; }

        bool IsConnected{ get; }

        bool IsConnecting{ get; }

        // Achievements

        void UpdateAchievement(GoogleAchievement achievement, GoogleAchievementDelegate cbk = null);

        IEnumerable<GoogleAchievement> Achievements { get; }

        void ShowAchievementsUI();

        // Photo
        //void LoadPhoto(string playerId, uint size, GameCenterPhotoDelegate cbk);


        // Quests

    }

    /*
     * public interface ISocialLogin
    {
        // Login

        void Login(ErrorDelegate cbk);

        void Logout(ErrorDelegate cbk);

        GoogleUser User{ get; }

        bool IsConnected{ get; }

        bool IsConnecting{ get; }
    }

    public interface ISocialAchievements
    {
        // Achievements

        void UpdateAchievement(GoogleAchievement achievement, GoogleAchievementDelegate cbk);

        void ResetAchievements(ErrorDelegate cbk);

        void ShowAchievementsUI();
    }

    public interface ISocialQuests
    {
        // Quests

    }

    public interface IGoogle : ISocialLogin, ISocialAchievements, ISocialQuests
    {

    }
*/
}