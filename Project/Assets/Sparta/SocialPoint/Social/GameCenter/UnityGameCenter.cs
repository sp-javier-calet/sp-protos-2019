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
    public class UnityGameCenter : BaseGameCenter
    {

        private readonly static string PhotosCacheFolder = "GameCenter";
        private GameCenterUser _user;

        public override GameCenterUser User
        {
            get
            {
                return _user;
            }
        }

        public Dictionary<string, double> Achievements { get; private set; }

        private bool _connecting = false;
        private GameCenterPlatform _platform;
        private List<GameCenterUser> _friends;

        SocialPointGameCenterVerification _verification;

        public override List<GameCenterUser> Friends
        {
            get
            {
                return _friends;
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

        void DoUpdateAchievement(GameCenterAchievement achi, GameCenterAchievementDelegate cbk = null)
        {
            if(Achievements == null)
            {
                if(cbk != null)
                {
                    cbk(achi, new Error("Failed to download the list of existing achievements"));
                }
                return;
            }
            string achiId = achi.Id;
            
            if(!Achievements.ContainsKey(achiId))
            {
                Achievements.Add(achiId, 0);
            }
            
            if(Achievements[achiId] < achi.Percent && achi.Percent <= 100)
            {
                _platform.ReportProgress(achiId, Achievements[achiId], (bool success) => {
                    if(cbk != null)
                    {
                        Error err = null;
                        if(!success)
                        {
                            err = new Error(string.Format("Error updating chievement '{0}'.", achiId));
                        }
                        cbk(achi, err);
                    }
                });
            }
            else if(cbk != null)
            {
                cbk(achi, null);
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
            
            _platform.LoadAchievements((IAchievement[] achievements) => {
                if(achievements != null)
                {
                    Achievements = new Dictionary<string, double>();
                    for(int k = 0; k < achievements.Length; k++)
                    {
                        if(achievements[k] != null && achievements[k] is IAchievement)
                        {
                            IAchievement achievementData = achievements[k];
                            Achievements.Add(achievementData.id, achievementData.percentCompleted);
                        }
                    }
                }
                if(cbk != null)
                {
                    Error err = null;
                    if(Achievements == null)
                    {
                        err = new Error("Could not download achievements.");
                    }
                    cbk(err);
                }
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
            _verification.Callback = (Error error, GameCenterUserVerification ver) => {
                if(Error.IsNullOrEmpty(error))
                {
                    _user.Verification = ver;
                    cbk(error);
                }
                else
                {
                    cbk(error);
                }
            };
        }


        public override bool IsConnected
        {
            get
            {
                return _platform.localUser.authenticated && !_connecting;
            }
        }

        public override bool IsConnecting
        {
            get
            {
                return _connecting;
            }
        }

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

        public override void UpdateScore(GameCenterScore score, GameCenterScoreDelegate cbk = null)
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

        public override void ResetAchievements(ErrorDelegate cbk = null)
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
                    cbk(err);
                }
            });
        }

        public override void UpdateAchievement(GameCenterAchievement achi, GameCenterAchievementDelegate cbk = null)
        {
            if(!IsConnected)
            {
                if(cbk != null)
                {
                    cbk(achi, new Error("GameCenter is not logged in"));
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

        public override void LoadPhoto(string userId, uint photoSize, GameCenterPhotoDelegate cbk = null)
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
    }
}
