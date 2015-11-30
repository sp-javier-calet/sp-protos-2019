using UnityEngine.SocialPlatforms;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Social
{
    public delegate void GoogleAchievementDelegate(GoogleAchievement achi,Error err);
    public delegate void GoogleLeaderboardDelegate(GoogleLeaderboard ldb,Error err);



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

        public string ImageUrl { get; private set; }

        public bool IsIncremental { get; private set; }

        public bool IsUnlocked { get; private set; }

        public GoogleAchievement(string id, int currentSteps)
        {
            Id = id;
            CurrentSteps = currentSteps;
        }

        public void SetInfo(string name, string description, bool unlocked, int steps, bool incremental, string imageUrl)
        {
            Name = name;
            Description = description;
            IsUnlocked = unlocked;
            TotalSteps = steps;
            IsIncremental = incremental;
            ImageUrl = imageUrl;
        }
    }

    public class GoogleLeaderboard
    {
        public string Id { get; private set; }

        public string Title { get; private set; }

        public bool FriendsOnly { get; private set; }

        public bool PlayerCentered { get; private set; }

        public long UserScore { get; private set; }

        public TimeScope Scope { get; private set; }

        public List<GoogleLeaderboardScoreEntry> Scores { get; private set; }

        public GoogleLeaderboard(string id, bool friendsOnly, bool playerCentered, TimeScope scope)
        {
            Id = id;
            FriendsOnly = friendsOnly;
            playerCentered = playerCentered;
            Scope = scope;
            Scores = new List<GoogleLeaderboardScoreEntry>();
        }

        public GoogleLeaderboard(string id, long score)
        {
            Id = id;
            UserScore = score;
            Scores = new List<GoogleLeaderboardScoreEntry>();
        }

        public GoogleLeaderboard(string id, string title, long score, bool friendsOnly, bool playerCentered, TimeScope scope)
        {
            Id = id;
            Title = title;
            UserScore = score;
            FriendsOnly = friendsOnly;
            PlayerCentered = playerCentered;
            Scope = scope;
            Scores = new List<GoogleLeaderboardScoreEntry>();
        }
    }

    public class GoogleLeaderboardScoreEntry
    {
        public string Name { get; set; }

        public long Rank { get; set; }

        public long Score { get; set; }
    }

    public class GoogleQuest
    {
        public string Id { get; private set; }

        public GoogleQuest(string id)
        {
            Id = id;
        }
    }

    public delegate void GoogleQuestEventDelegate(GoogleQuestEvent evt,Error err);
    public class GoogleQuestEvent
    {
        public string QuestId  { get; private set; }

        public string MilestoneId  { get; private set; }

        public EventType Type { get; private set; }

        public enum EventType
        {
            None,
            Accept,
            ClaimMilestone
        }

        GoogleQuestEvent()
        {
            Type = EventType.None;
        }

        public static GoogleQuestEvent Empty = new GoogleQuestEvent();

        public static GoogleQuestEvent CreateAcceptEvent(string questId)
        {
            var evt = new GoogleQuestEvent();
            evt.Type = EventType.Accept;
            evt.QuestId = questId;
            return evt;
        }

        public static GoogleQuestEvent CreateMilestoneEvent(string questId, string milestoneId)
        {
            var evt = new GoogleQuestEvent();
            evt.Type = EventType.ClaimMilestone;
            evt.QuestId = questId;
            evt.MilestoneId = milestoneId;
            return evt;
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

        void ResetAchievement(GoogleAchievement achi, GoogleAchievementDelegate cbk = null);

        IEnumerable<GoogleAchievement> Achievements { get; }

        void ShowAchievementsUI();

        // Photo
        //void LoadPhoto(string playerId, uint size, GameCenterPhotoDelegate cbk);

        // Leaderboards

        void LoadLeaderboard(GoogleLeaderboard ldb, uint rowCount, GoogleLeaderboardDelegate cbk);

        void UpdateLeaderboard(GoogleLeaderboard ldb, GoogleLeaderboardDelegate cbk = null);

        void ShowLeaderboardsUI(string id = null);

        // Quests

        void IncrementEvent(string id, uint quantity = 1);

        void ShowViewQuestsUI(GoogleQuestEventDelegate cbk = null);

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