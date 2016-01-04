using UnityEngine.SocialPlatforms;
using System.Linq;
using System.Collections.Generic;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Quests;
using SocialPoint.Base;

namespace SocialPoint.Social
{
    public class UnityGoogle : IGoogle
    {
        GoogleUser _user;
        PlayGamesPlatform _platform;
        Dictionary<string, GoogleAchievement> _achievements = null;

        #region IGoogle implementation

        public void Login(ErrorDelegate cbk)
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

            _platform.Authenticate((bool success) => {
                if(success)
                {
                    LoginLoadPlayerData((Error err) => {
                        if(!Error.IsNullOrEmpty(err))
                        {
                            OnLoginEnd(err, cbk);
                        }
                        else
                        {
                            DownloadAchievements((err2) => OnLoginEnd(err2, cbk));
                        }
                    });
                }
                else
                {
                    OnLoginEnd(new Error("Cannot connect to Google Play Games"), cbk);
                }
            });
        }

        void OnLoginEnd(Error err, ErrorDelegate cbk = null)
        {
            if(cbk != null)
            {
                cbk(err);
            }
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
                if(cbk != null)
                {
                    cbk(null);
                }
            }
        }

        public void Logout(ErrorDelegate cbk)
        {
            _platform = null;
            _user = null;
            _achievements = null;

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
                return _platform != null && _platform.IsAuthenticated();
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

            string accessToken = _platform.GetAccessToken();
            Error err = null;
            if(!string.IsNullOrEmpty(accessToken))
            {
                string uri = string.Format("https://www.googleapis.com/games/v1management/achievements/{0}/reset", achi.Id);
                var form = new UnityEngine.WWWForm();
                form.AddField("access_token", _platform.GetAccessToken());
                var www = new UnityEngine.WWW(uri, form);
                while(!www.isDone)
                    ;
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

            DownloadAchievements((err) => {
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
                        _platform.IncrementAchievement(id, steps, (success) => {
                            _achievements[id] = GetAchievement(id);
                            if(cbk != null)
                            {
                                cbk(achi, success ? null : new Error("Error incrementing achievement steps"));
                            }
                        });
                    }
                    else
                    {
                        _platform.ReportProgress(id, 100, (success) => {
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
                (scoredata) => {
                    if(cbk != null)
                    {
                        if(scoredata.Valid)
                        {
                            var leaderboard = new GoogleLeaderboard(scoredata.Id, scoredata.Title, 
                                                  scoredata.PlayerScore.value, ldb.FriendsOnly, ldb.PlayerCentered, ldb.Scope);
                            // Update scores for users
                            var scores = new Dictionary<string, GoogleLeaderboardScoreEntry>();
                            foreach(var score in scoredata.Scores)
                            {
                                var entry = new GoogleLeaderboardScoreEntry();
                                entry.Score = score.value;
                                entry.Rank = score.rank;
                                scores.Add(score.userID, entry);
                                leaderboard.Scores.Add(entry);
                            }

                            // Load user names
                            _platform.LoadUsers(scores.Keys.ToArray(), (users) => {
                                foreach(var user in users)
                                {
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

            _platform.ReportScore(ldb.UserScore, ldb.Id, (success) => {
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
                (QuestUiResult result, IQuest quest, IQuestMilestone milestone) => {
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
                            cbk(GoogleQuestEvent.Empty, new Error((int)result, "Quest view error: " + result.ToString()));
                        }
                        break;
                    }
                });
        }

        void AcceptQuest(IQuest toAccept, GoogleQuestEventDelegate cbk = null)
        {
            _platform.Quests.Accept(toAccept,
                (QuestAcceptStatus status, IQuest quest) => {
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
                (QuestClaimMilestoneStatus status, IQuest quest, IQuestMilestone milestone) => {
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
    }
}
