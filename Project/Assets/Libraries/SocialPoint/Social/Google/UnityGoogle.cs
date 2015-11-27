using UnityEngine.SocialPlatforms;
using System.Linq;
using System.Collections.Generic;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Quests;
using SocialPoint.Base;

namespace SocialPoint.Social
{
    public class UnityGoogle : BaseGoogle
    {
        GoogleUser _user;
        PlayGamesPlatform _platform;
        Dictionary<string, GoogleAchievement> _achievements = null;

        #region IGoogle implementation

        public override void Login(ErrorDelegate cbk)
        {
            if(IsConnected)
            {
                if(cbk != null)
                {
                    cbk(null);
                }
                return;
            }

            _user = new GoogleUser();

            // FIXME recommended for debugging:
            PlayGamesPlatform.DebugLogEnabled = true;

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
                //_friends.Clear();
                _user = new GoogleUser();
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

        public override void Logout(ErrorDelegate cbk)
        {
            // Logout is possible?
            _platform = null;
            _user = null;
            _achievements = null;

            if(cbk != null)
            {
                cbk(null);
            }
        }


        public override GoogleUser User
        {
            get
            {
                return _user;
            }
        }

        public override bool IsConnected
        {
            get
            {
                return _platform != null && _platform.IsAuthenticated();
            }
        }

        public override bool IsConnecting
        {
            get
            {
                return _platform != null && !_platform.IsAuthenticated(); // FIXME
            }
        }


        #region Achievements

        public override void ResetAchievement(GoogleAchievement achi, GoogleAchievementDelegate cbk = null)
        {
            string uri = string.Format("https://www.googleapis.com/games/v1management/achievements/{0}/reset", achi.Id);
            var form = new UnityEngine.WWWForm();
            form.AddField("access_token", _platform.GetAccessToken());
            var www = new UnityEngine.WWW(uri, form);
            while(!www.isDone)
                ;
            if(cbk != null)
            {
                cbk(achi, string.IsNullOrEmpty(www.error) ? null : new Error(www.error));
            }
        }

        public override void UpdateAchievement(GoogleAchievement achi, GoogleAchievementDelegate cbk = null)
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

        public override IEnumerable<GoogleAchievement> Achievements
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

        public override void ShowAchievementsUI()
        {
            if(_platform != null)
            {
                _platform.ShowAchievementsUI();
            }
        }

        #endregion

        #region Leaderboards

        public override void LoadLeaderboard(GoogleLeaderboard ldb, GoogleLeaderboardDelegate cbk = null)
        {
            if(!IsConnected)
            {
                if(cbk != null)
                {
                    cbk(ldb, new Error("Google is not logged in"));
                }
                return;
            }

            var board = _platform.CreateLeaderboard();
            board.id = ldb.Id;
            board.userScope = ldb.FriendsOnly ? UserScope.FriendsOnly : UserScope.Global;

            _platform.LoadScores(board.id,
                LeaderboardStart.PlayerCentered, 10, LeaderboardCollection.Public, LeaderboardTimeSpan.AllTime, // TODO FILTER
                (scoredata) => {
                    if(cbk != null)
                    {
                        if(scoredata.Valid)
                        {
                            var leaderboard = new GoogleLeaderboard(scoredata.Id, scoredata.Title, 
                                                  scoredata.PlayerScore.value, board.userScope == UserScope.FriendsOnly);
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
            
            /*(success) => {
                if(cbk != null)
                {
                    if(success)
                    {
                        var leaderboard = new GoogleLeaderboard(board.id, board.title, 
                                              board.localUserScore.value, board.userScope == UserScope.FriendsOnly);
                        // Update scores for users
                        var scores = new Dictionary<string, GoogleLeaderboardScoreEntry>();
                        foreach(IScore score in board.scores)
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
            });*/
        }

        public override void UpdateLeaderboard(GoogleLeaderboard ldb, GoogleLeaderboardDelegate cbk = null)
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

        public override void ShowLeaderboardsUI(string id = null)
        {
            if(_platform != null)
            {
                _platform.ShowLeaderboardUI(id);
            }
        }

        #endregion


        #region Quests

        public override void IncrementEvent(string id, uint quantity = 1)
        {
            if(IsConnected && !string.IsNullOrEmpty(id))
            {
                _platform.Events.IncrementEvent(id, quantity);
            }
        }

        public override void ShowViewQuestsUI(GoogleQuestEventDelegate cbk = null)
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

                    case QuestUiResult.UserCanceled:
                        // Do Nothing
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
