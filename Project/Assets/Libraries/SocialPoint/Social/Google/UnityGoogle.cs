using UnityEngine.SocialPlatforms;
using System.Linq;
using System.Collections.Generic;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
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
                    googleAchievement.TotalSteps, googleAchievement.IsIncremental);
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
            _platform.ShowAchievementsUI();
        }

        #endregion


        #region Quests

        #endregion


        #endregion
    }
}
