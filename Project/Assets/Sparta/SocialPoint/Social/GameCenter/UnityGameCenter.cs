using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;

using SocialPoint.Base;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.Social
{
    public delegate void GameCenterValidationDelegate(Error error, GameCenterUserVerification ver);
    public class UnityGameCenter : IGameCenter
    {

        private readonly static string PhotosCacheFolder = "GameCenter";
        private GameCenterUser _user;

        public GameCenterUser User
        {
            get
            {
                return _user;
            }
        }

        public IEnumerable<GameCenterAchievement> Achievements
        {
            get
            {
                return _achievements;
            }
        }

        private List<GameCenterAchievement> _achievements;
        private bool _connecting = false;
        private GameCenterPlatform _platform;
        private List<GameCenterUser> _friends;

        SocialPointGameCenterVerification _verification;

        public List<GameCenterUser> Friends
        {
            get
            {
                return _friends;
            }
        }

        public event Action StateChangeEvent;

        protected void NotifyStateChanged()
        { 
            if(StateChangeEvent != null)
            {
                StateChangeEvent();
            }
        }

        void OnLoginEnd(Error err, ErrorDelegate cbk = null)
        {
            _connecting = false;
            NotifyStateChanged();
            if(!Error.IsNullOrEmpty(err))
            {
                Debug.Log("Game Center login ended in error: " + err);
            }
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
                _friends.Clear();
                _user = new GameCenterUser();
                if(cbk != null)
                {
                    cbk(new Error("Could not login."));
                }
            }
            else
            {
                var user = new GameCenterUser(localUser.id,
                               localUser.userName,
                               localUser.userName,
                               localUser.underage ? GameCenterUser.AgeGroup.Underage : GameCenterUser.AgeGroup.Adult
                           );
                _user = user;

                RequestGameCenterVerification(cbk);
            }
        }

        void LoginDownloadFriends(ErrorDelegate cbk, bool initial = true)
        {
            var localUser = _platform.localUser;
            if((localUser.friends == null || localUser.friends.Length == 0) && initial)
            {

                localUser.LoadFriends((bool success) => {
                    if(success)
                    {
                        LoginDownloadFriends(cbk, false);
                    }
                    else
                    {
                        if(cbk != null)
                        {
                            cbk(new Error("Could not load friends."));
                        }
                    }
                });
                return;
            }
            else
            {
                Friends.Clear();
                if(localUser.friends != null)
                {
                    for(int k = 0; k < localUser.friends.Length; k++)
                    {
                        if(localUser.friends[k] != null && localUser.friends[k] is IUserProfile)
                        {
                            IUserProfile friendData = localUser.friends[k];
                            
                            Friends.Add(new GameCenterUser(friendData.id,
                                friendData.userName,
                                friendData.userName));
                        }
                    }
                }
            }
            if(cbk != null)
            {
                cbk(null);
            }
        }

        void DownloadAchievements(ErrorDelegate cbk)
        {
            if(Achievements != null)
            {
                if(cbk != null)
                {
                    cbk(null);
                }
                return;
            }
            _platform.LoadAchievementDescriptions((IAchievementDescription[] descs) => {
                _platform.LoadAchievements((IAchievement[] achis) => {
                    if(achis != null)
                    {
                        _achievements = new List<GameCenterAchievement>();
                        foreach(var d in descs)
                        {
                            var percent = 0.0f;
                            foreach(var a in achis)
                            {
                                if(a.id == d.id)
                                {
                                    percent = (float)a.percentCompleted;
                                    break;
                                }
                            }
                            _achievements.Add(new GameCenterAchievement(d.id, percent));
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
            });
        }

        public UnityGameCenter(Transform parent = null)
        {
            _friends = new List<GameCenterUser>();
            _user = new GameCenterUser();
            var go = new GameObject(GetType().ToString());
            if(parent == null)
            {
                GameObject.DontDestroyOnLoad(go);
            }
            else
            {
                go.transform.SetParent(parent);
            }
            _platform = new GameCenterPlatform();
            _verification = go.AddComponent<SocialPointGameCenterVerification>();
        }

        private void RequestGameCenterVerification(ErrorDelegate cbk)
        {
            _verification.LoadData((Error error, GameCenterUserVerification ver) => {
                if(Error.IsNullOrEmpty(error))
                {
                    _user.Verification = ver;
                    cbk(error);
                }
                else
                {
                    cbk(error);
                }
            });
        }


        public bool IsConnected
        {
            get
            {
                return _platform.localUser.authenticated && !_connecting;
            }
        }

        public bool IsConnecting
        {
            get
            {
                return _connecting;
            }
        }

        public void Login(ErrorDelegate cbk=null)
        {
            if(IsConnected)
            {
                if(cbk != null)
                {
                    cbk(null);
                }
                return;
            }
            _connecting = true;
            _platform.localUser.Authenticate((bool success) => {
                if(success)
                {
                    LoginLoadPlayerData((err) => {
                        if(!Error.IsNullOrEmpty(err))
                        {
                            OnLoginEnd(err, cbk);
                        }
                        else
                        {
                            LoginDownloadFriends((err2) => {
                                if(!Error.IsNullOrEmpty(err2))
                                {
                                    OnLoginEnd(err2, cbk);
                                }
                                else
                                {
                                    DownloadAchievements((err3) => {
                                        OnLoginEnd(err3, cbk);
                                    });
                                }
                            });
                        }
                    });
                }
                else
                {
                    OnLoginEnd(new Error("Could not login"), cbk);
                }
            });
        }

        public void UpdateScore(GameCenterScore score, GameCenterScoreDelegate cbk = null)
        {
            if(!IsConnected)
            {
                if(cbk != null)
                {
                    cbk(score, new Error("GameCenter is not logged in"));
                }
                return;
            }                

            _platform.ReportScore(score.Value, score.Category, (bool success) => {
                if(cbk != null)
                {
                    Error err = null;
                    if(!success)
                    {
                        err = new Error(string.Format("Could not update score in '{0}'.", score.Category));
                    }
                    cbk(score, err);
                }
            });
        }

        public void ResetAchievements(ErrorDelegate cbk = null)
        {
            if(!IsConnected)
            {
                if(cbk != null)
                {
                    cbk(new Error("GameCenter is not logged in"));
                }
                return;
            }
            UnityEngine.SocialPlatforms.GameCenter.GameCenterPlatform.ResetAllAchievements((bool success) => {
                if(cbk != null)
                {
                    Error err = null;
                    if(!success)
                    {
                        err = new Error("Could not reset achievements.");
                    }
                    else
                    {
                        foreach(var achi in _achievements)
                        {
                            achi.Percent = 0.0f;
                        }
                    }
                    cbk(err);
                }
            });
        }

        public void UpdateAchievement(GameCenterAchievement achi, GameCenterAchievementDelegate cbk = null)
        {
            if(!IsConnected)
            {
                if(cbk != null)
                {
                    cbk(achi, new Error("GameCenter is not logged in"));
                }
                return;
            }
            if(Achievements == null)
            {
                if(cbk != null)
                {
                    cbk(achi, new Error("Failed to download the list of existing achievements"));
                }
                return;
            }
            _platform.ReportProgress(achi.Id, achi.Percent, (bool success) => {
                if(cbk != null)
                {
                    Error err = null;
                    if(!success)
                    {
                        err = new Error(string.Format("Error updating achievement '{0}'.", achi.Id));
                    }
                    else
                    {
                        bool found = false;
                        foreach(var a in _achievements)
                        {
                            if(a.Id == achi.Id)
                            {
                                found = true;
                                a.Percent = achi.Percent;
                                break;
                            }
                        }
                        if(!found)
                        {
                            _achievements.Add(achi);
                        }
                    }
                    cbk(achi, err);
                }
            });
        }

        public void LoadPhoto(string userId, uint photoSize, GameCenterPhotoDelegate cbk = null)
        {
            if(!IsConnected)
            {
                if(cbk != null)
                {
                    cbk(null, new Error("GameCenter is not logged in"));
                }
                return;
            }

            string tmpFilePath = Application.temporaryCachePath + "/" + PhotosCacheFolder + "/" + userId + "_" + photoSize.ToString() + ".png";
            _platform.LoadUsers(new string[]{ userId }, (users) => {
                Error err = null;
                if(users == null || users.Length == 0)
                {
                    err = new Error(string.Format("User with id {0} not found.", userId));
                }
                else
                {
                    IUserProfile u = users[0] as IUserProfile;
                    if(u.image != null)
                    {
                        Texture2D newTexture = Texture2D.Instantiate(u.image) as Texture2D;
                        newTexture.Resize((int)photoSize, (int)photoSize);
                        err = ImageUtils.SaveTextureToFile(newTexture, tmpFilePath);
                    }
                    else
                    {
                        tmpFilePath = null;
                        err = new Error(string.Format("User with id {0} does not have an image.", userId));
                    }
                }
                
                if(cbk != null)
                {
                    cbk(tmpFilePath, err);
                }
                
            });
        }


        public void ShowAchievementsUI()
        {
            _platform.ShowAchievementsUI();
        }

        public void ShowLeaderboardUI(string id = null)
        {
            GameCenterPlatform.ShowLeaderboardUI(id, TimeScope.AllTime);
        }
    }
}
