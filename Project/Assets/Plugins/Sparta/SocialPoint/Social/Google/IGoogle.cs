using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace SocialPoint.Social
{
    public delegate void GoogleAchievementDelegate(GoogleAchievement achi, Error err);
    public delegate void GoogleLeaderboardDelegate(GoogleLeaderboard ldb, Error err);
    public delegate void GoogleQuestEventDelegate(GoogleQuestEvent evt, Error err);
    public delegate void GoogleStateChangeDelegate();

    public sealed class GoogleUser
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

        public Texture2D Image { get; private set; }

        public GoogleUser(string id = "", string displayName = "", string photoUrl = "", AgeGroup age = AgeGroup.Unknown)
        {
            UserId = id;
            Name = displayName;
            PhotoUrl = photoUrl;
            Age = age;
        }
    }

    public sealed class GoogleAchievement
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

    public sealed class GoogleLeaderboard
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
            PlayerCentered = playerCentered;
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

    public sealed class GoogleLeaderboardScoreEntry
    {
        public string Name { get; set; }

        public long Rank { get; set; }

        public long Score { get; set; }
    }

    public sealed class GoogleQuest
    {
        public string Id { get; private set; }

        public GoogleQuest(string id)
        {
            Id = id;
        }
    }

    public sealed class GoogleQuestEvent
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
        event GoogleStateChangeDelegate StateChangeEvent;

        Action<string, AttrDic, ErrorDelegate> TrackEvent { get; set; }

        // Login

        GoogleUser User{ get; }

        bool IsConnected{ get; }

        bool IsConnecting{ get; }

        bool HasCancelledLogin { get; }

        string AccessToken{ get; }

        List<GoogleUser> Friends { get; }

        /// <summary>
        /// Starts login with Google Play Games
        /// </summary>
        void Login(ErrorDelegate cbk, bool silent = false);

        /// <summary>
        /// Clean Login information
        /// </summary>
        void Logout(ErrorDelegate cbk);


        // Achievements

        /// <summary>
        /// Update achievement steps
        /// </summary>
        void UpdateAchievement(GoogleAchievement achievement, GoogleAchievementDelegate cbk = null);

        /// <summary>
        /// Reset achievement status
        /// </summary>
        void ResetAchievement(GoogleAchievement achi, GoogleAchievementDelegate cbk = null);

        IEnumerable<GoogleAchievement> Achievements { get; }

        /// <summary>
        /// Show native Achievements view
        /// </summary>
        void ShowAchievementsUI();


        // Leaderboards

        /// <summary>
        /// Async loading of Leaderboard data
        /// </summary>
        void LoadLeaderboard(GoogleLeaderboard ldb, uint rowCount, GoogleLeaderboardDelegate cbk);

        /// <summary>
        /// Reports user score to a Leaderboard.
        /// </summary>
        void UpdateLeaderboard(GoogleLeaderboard ldb, GoogleLeaderboardDelegate cbk = null);

        void ShowLeaderboardsUI(string id = null);


        // Quests

        /// <summary>
        /// Increment a quest event by id
        /// </summary>
        void IncrementEvent(string id, uint quantity = 1);

        /// <summary>
        /// Show native Quest view and manage user actions
        /// </summary>
        void ShowViewQuestsUI(GoogleQuestEventDelegate cbk = null);

        Texture2D GetUserPhoto(string userID);
    }

    public static class GoogleExtensions
    {
        [Obsolete("User AccessToken property")]
        public static string GetAccessToken(this IGoogle google)
        {
            return google.AccessToken;
        }
    }
}