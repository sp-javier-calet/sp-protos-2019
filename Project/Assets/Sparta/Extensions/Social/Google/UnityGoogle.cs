using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Quests;
using SocialPoint.Base;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace SocialPoint.Social
{
    public class UnityGoogle : MonoBehaviour, IGoogle
    {
        public event GoogleStateChangeDelegate StateChangeEvent;

        protected void NotifyStateChanged()
        {
            if(StateChangeEvent != null)
            {
                StateChangeEvent();
            }
        }

        GoogleUser _user;
        PlayGamesPlatform _platform;
        Dictionary<string, GoogleAchievement> _achievements;
        bool _loginSuccess;
        ErrorDelegate _loginCallback;
        List<GoogleUser> _friends;
        bool _connecting;
        string _accessToken;

        public List<GoogleUser> Friends
        {
            get
            {
                return _friends;
            }
        }

        #region IGoogle implementation

        public void Login(ErrorDelegate cbk, bool silent = false)
        {
            if(IsConnected)
            {
                if(cbk != null)
                {
                    cbk(null);
                }
                return;
            }

            // Use Activate() instead to override Social.Active
            _platform = PlayGamesPlatform.Instance;
            _connecting = true;
            _loginCallback = cbk;
            _platform.Authenticate(success => {
                _loginSuccess = success;
                DispatchMainThread(UpdateAfterLogin);
            }, silent);
        }

        void UpdateAfterLogin()
        {
            if(_loginCallback != null)
            {
                OnLogin();
            }
        }

        void LoadDescriptionAchievements()
        {
            DebugUtils.Log("Intentamos cargar los achivements");
            _platform.LoadAchievementDescriptions(descriptions => {
                if(descriptions.Length > 0)
                {
                    DebugUtils.Log("Got " + descriptions.Length + " achievement descriptions");
                    var achievementDescriptions = new StringBuilder();
                    achievementDescriptions.Append("Achievement Descriptions:\n");
                    for(int i = 0, descriptionsLength = descriptions.Length; i < descriptionsLength; i++)
                    {
                        IAchievementDescription ad = descriptions[i];
                        achievementDescriptions.Append("\t").Append(ad.id).Append(" ").Append(ad.title).Append(" ").Append(ad.unachievedDescription).AppendLine();
                    }
                    DebugUtils.Log(achievementDescriptions.ToString());
                }
                else
                {
                    DebugUtils.Log("Failed to load achievement descriptions");
                }
            });
        }

        void OnLogin()
        {
            if(_loginSuccess)
            {
                LoginLoadPlayerData(err => {
                    if(!Error.IsNullOrEmpty(err))
                    {
                        DispatchMainThread(() => OnLoginEnd(err, _loginCallback));
                    }
                    else
                    {
                        DownloadAchievements(err2 => DispatchMainThread(() => OnLoginEnd(err2, _loginCallback)));
                    }
                });
            }
            else
            {
                DispatchMainThread(() => OnLoginEnd(new Error("Cannot connect to Google Play Games"), _loginCallback));
            }
        }

        void OnLoginEnd(Error err, ErrorDelegate cbk = null)
        {
            _connecting = false;
            NotifyStateChanged();
            if(cbk != null)
            {
                cbk(err);
            }
            _loginCallback = null;
        }

        void LoginLoadPlayerData(ErrorDelegate cbk = null)
        {
            var localUser = _platform.localUser;
            if(!localUser.authenticated)
            {
                _user = null;
                if(cbk != null)
                {
                    cbk(new Error("Could not login."));
                }
            }
            else
            {
                _user = new GoogleUser(localUser.id,
                    localUser.userName,
                    _platform.GetUserImageUrl(),
                    localUser.underage ? GoogleUser.AgeGroup.Underage : GoogleUser.AgeGroup.Adult
                );

                _platform.GetServerAuthCode((result, token) => {
                    if(result != CommonStatusCodes.Success)
                    {
                        if(cbk != null)
                        {
                            cbk(new Error("Got " + result + " when requesting access token."));
                        }
                        return;
                    }
                    _accessToken = token;
                    _platform.LoadFriends(_platform.localUser, bolean => {
                        _friends = new List<GoogleUser>();
                        IUserProfile[] friends = _platform.GetFriends();
                        for(int i = 0, friendsLength = friends.Length; i < friendsLength; i++)
                        {
                            IUserProfile friend = friends[i];
                            _friends.Add(new GoogleUser(friend.id, friend.userName, string.Empty));
                        }

                        if(cbk != null)
                        {
                            cbk(null);
                        }
                    });
                });
            }
        }

        public void Logout(ErrorDelegate cbk)
        {
            _platform.SignOut();
            _platform = null;
            _user = null;
            _achievements = null;

            NotifyStateChanged();

            if(cbk != null)
            {
                cbk(null);
            }
        }

        public GoogleUser User
        {
            get
            {
                return _user;
            }
        }

        public bool IsConnected
        {
            get
            {
                return !_connecting && _platform != null && _platform.IsAuthenticated();
            }
        }

        public bool IsConnecting
        {
            get
            {
                return _connecting;
            }
        }

        #region Achievements

        public void ResetAchievement(GoogleAchievement achi, GoogleAchievementDelegate cbk = null)
        {
            if(!IsConnected)
            {
                if(cbk != null)
                {
                    cbk(achi, new Error("Google is not logged in"));
                }
                return;
            }

            string accessToken = AccessToken;
            Error err = null;
            if(!string.IsNullOrEmpty(accessToken))
            {
                string uri = string.Format("https://www.googleapis.com/games/v1management/achievements/{0}/reset", achi.Id);
                var form = new WWWForm();
                form.AddField("access_token", accessToken);
                var www = new WWW(uri, form);
                while(!www.isDone)
                {
                }
                if(!string.IsNullOrEmpty(www.error))
                {
                    err = new Error(www.error);
                }
            }
            else
            {
                err = new Error("Invalid access token. " + accessToken);
            }

            if(cbk != null)
            {
                cbk(achi, err);
            }
        }

        public Texture2D GetUserPhoto(string userID)
        {
            IUserProfile[] users = _platform.GetFriends();
            for(int i = 0, usersLength = users.Length; i < usersLength; i++)
            {
                IUserProfile user = users[i];
                if(user.id == userID)
                {
                    return user.image;
                }
            }
            return null;
        }

        public void UpdateAchievement(GoogleAchievement achi, GoogleAchievementDelegate cbk = null)
        {
            if(!IsConnected)
            {
                if(cbk != null)
                {
                    cbk(achi, new Error("Google is not logged in"));
                }
                return;
            }

            DownloadAchievements(err => {
                if(!Error.IsNullOrEmpty(err))
                {
                    if(cbk != null)
                    {
                        cbk(achi, err);
                    }
                }
                else
                {
                    DoUpdateAchievement(achi, cbk);
                }
            });
        }

        void DoUpdateAchievement(GoogleAchievement achi, GoogleAchievementDelegate cbk = null)
        {
            if(_achievements == null)
            {
                if(cbk != null)
                {
                    cbk(achi, new Error("Failed to download the list of existing achievements"));
                }
                return;
            }

            var id = achi.Id;
            var currStatus = GetAchievement(id);
            if(currStatus != null && !currStatus.IsUnlocked)
            {
                int steps = achi.CurrentSteps - currStatus.CurrentSteps;
                if(steps > 0)
                {
                    if(currStatus.IsIncremental)
                    {
                        // TODO SetStepAtLeast
                        _platform.IncrementAchievement(id, steps, success => {
                            _achievements[id] = GetAchievement(id);
                            if(cbk != null)
                            {
                                cbk(achi, success ? null : new Error("Error incrementing achievement steps"));
                            }
                        });
                    }
                    else
                    {
                        _platform.ReportProgress(id, 100, success => {
                            _achievements[id] = GetAchievement(id);
                            if(cbk != null)
                            {
                                cbk(achi, success ? null : new Error("Error unlocking achievement"));
                            }
                        });
                    }
                }
            }
        }

        public string AccessToken
        {
            get
            {
                return _accessToken;
            }
        }

        public IEnumerable<GoogleAchievement> Achievements
        {
            get
            {
                return _achievements != null ? _achievements.Values : Enumerable.Empty<GoogleAchievement>();
            }
        }

        GoogleAchievement GetAchievement(string id)
        {
            var googleAchievement = _platform.GetAchievement(id); 
            GoogleAchievement achi = null;
            if(googleAchievement != null)
            {
                achi = new GoogleAchievement(googleAchievement.Id, googleAchievement.CurrentSteps);
                achi.SetInfo(googleAchievement.Name, googleAchievement.Description, googleAchievement.IsUnlocked,
                    googleAchievement.TotalSteps, googleAchievement.IsIncremental,
                    googleAchievement.IsUnlocked ? googleAchievement.UnlockedImageUrl : googleAchievement.RevealedImageUrl);
                
            }
            return achi;
        }

        void DownloadAchievements(ErrorDelegate cbk)
        {
            if(_achievements != null)
            {
                if(cbk != null)
                {
                    cbk(null);
                }
                return;
            }

            _platform.LoadAchievements(achievements => {
                if(achievements != null)
                {
                    _achievements = new Dictionary<string, GoogleAchievement>();
                    for(int k = 0; k < achievements.Length; k++)
                    {
                        IAchievement achievementData = achievements[k];
                        _achievements.Add(achievementData.id, GetAchievement(achievementData.id));
                    }
                }
                if(cbk != null)
                {
                    Error err = null;
                    if(_achievements == null)
                    {
                        err = new Error("Could not download achievements.");
                    }
                    cbk(err);
                }
            });
        }

        public void ShowAchievementsUI()
        {
            if(_platform != null)
            {
                _platform.ShowAchievementsUI();
            }
        }

        #endregion

        #region Leaderboards

        public void LoadLeaderboard(GoogleLeaderboard ldb, uint rowCount, GoogleLeaderboardDelegate cbk)
        {
            if(!IsConnected)
            {
                if(cbk != null)
                {
                    cbk(ldb, new Error("Google is not logged in"));
                }
                return;
            }

            _platform.LoadScores(ldb.Id,
                ldb.PlayerCentered ? LeaderboardStart.PlayerCentered : LeaderboardStart.TopScores, 
                (int)rowCount, 
                ldb.FriendsOnly ? LeaderboardCollection.Social : LeaderboardCollection.Public,
                (LeaderboardTimeSpan)ldb.Scope,
                scoredata => {
                    if(cbk != null)
                    {
                        if(scoredata.Valid)
                        {
                            var leaderboard = new GoogleLeaderboard(scoredata.Id, scoredata.Title, 
                                                  scoredata.PlayerScore.value, ldb.FriendsOnly, ldb.PlayerCentered, ldb.Scope);
                            // Update scores for users
                            var scores = new Dictionary<string, GoogleLeaderboardScoreEntry>();
                            for(int i = 0, scoredataScoresLength = scoredata.Scores.Length; i < scoredataScoresLength; i++)
                            {
                                var score = scoredata.Scores[i];
                                var entry = new GoogleLeaderboardScoreEntry();
                                entry.Score = score.value;
                                entry.Rank = score.rank;
                                scores.Add(score.userID, entry);
                                leaderboard.Scores.Add(entry);
                            }

                            // Load user names
                            _platform.LoadUsers(scores.Keys.ToArray(), users => {
                                for(int i = 0, usersLength = users.Length; i < usersLength; i++)
                                {
                                    var user = users[i];
                                    scores[user.id].Name = user.userName;
                                }

                                if(cbk != null)
                                {
                                    cbk(leaderboard, null);
                                }
                            });
                        }
                        else
                        {
                            cbk(null, new Error("Couldn't load leaderboad scores"));    
                        }
                    }
                });
        }

        public void UpdateLeaderboard(GoogleLeaderboard ldb, GoogleLeaderboardDelegate cbk = null)
        {
            if(!IsConnected)
            {
                if(cbk != null)
                {
                    cbk(ldb, new Error("Google is not logged in"));
                }
                return;
            }

            _platform.ReportScore(ldb.UserScore, ldb.Id, success => {
                if(cbk != null)
                {
                    cbk(ldb, success ? null : new Error("Couldn't update leaderboard"));
                }
            });
        }

        public void ShowLeaderboardsUI(string id = null)
        {
            if(_platform != null)
            {
                _platform.ShowLeaderboardUI(id);
            }
        }

        #endregion


        #region Quests

        public void IncrementEvent(string id, uint quantity = 1)
        {
            if(IsConnected && !string.IsNullOrEmpty(id))
            {
                _platform.Events.IncrementEvent(id, quantity);
            }
        }

        public void ShowViewQuestsUI(GoogleQuestEventDelegate cbk = null)
        {
            if(!IsConnected)
            {
                if(cbk != null)
                {
                    cbk(GoogleQuestEvent.Empty, new Error("Google is not logged in"));
                }
                return;
            }

            _platform.Quests.ShowAllQuestsUI(
                (result, quest, milestone) => {
                    switch(result)
                    {
                    case QuestUiResult.UserRequestsQuestAcceptance:
                        AcceptQuest(quest, cbk);
                        break;

                    case QuestUiResult.UserRequestsMilestoneClaiming:
                        ClaimMilestone(milestone, cbk);
                        break;

                    default:
                        if(cbk != null)
                        {
                            cbk(GoogleQuestEvent.Empty, new Error((int)result, "Quest view error: " + result));
                        }
                        break;
                    }
                });
        }

        void AcceptQuest(IQuest toAccept, GoogleQuestEventDelegate cbk = null)
        {
            _platform.Quests.Accept(toAccept,
                (status, quest) => {
                    switch(status)
                    {
                    case QuestAcceptStatus.Success:
                        if(cbk != null)
                        {
                            cbk(GoogleQuestEvent.CreateAcceptEvent(quest.Id), null);
                        }
                        break;

                    default:
                        if(cbk != null)
                        {
                            cbk(GoogleQuestEvent.Empty, new Error((int)status, "Error accepting quest"));
                        }

                        break;
                    }
                });
        }

        void ClaimMilestone(IQuestMilestone toClaim, GoogleQuestEventDelegate cbk = null)
        {
            _platform.Quests.ClaimMilestone(toClaim,
                (status, quest, milestone) => {
                    switch(status)
                    {
                    case  QuestClaimMilestoneStatus.Success:
                        if(cbk != null)
                        {
                            cbk(GoogleQuestEvent.CreateMilestoneEvent(quest.Id, milestone.Id), null);
                        }
                        break;

                    default:
                        if(cbk != null)
                        {
                            cbk(GoogleQuestEvent.Empty, new Error((int)status, "Error claiming quest milestone"));
                        }
                        break;
                    }
                });
        }

        #endregion

        #endregion

        #region Dispatch

        Action _dispatched;

        void LateUpdate()
        {
            DispatchPending();
        }

        
        void DispatchPending()
        {
            if(_dispatched != null)
            {
                _dispatched();
                _dispatched = null;
            }
        }

        void DispatchMainThread(Action action)
        {
            /* System events have to be dispatched to the current Unity Main Thread 
             * since this changes between Development and Production builds. */
            _dispatched += action;
        }

        #endregion
    }
}
