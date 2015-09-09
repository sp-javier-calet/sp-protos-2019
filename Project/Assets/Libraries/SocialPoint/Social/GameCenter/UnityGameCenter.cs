using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Social
{
    public class UnityGameCenter : BaseGameCenter
    {
        public enum States
        {
            LoggedIn,
            LoggingIn,
            LoggedOut,
            Error
        }
        
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

        public bool ShowLoginWindow { get; private set; }

        public bool PlayerVerification { get; private set; }
        
        private States _state;

        public States State
        {
            get
            {
                return _state;
            }
            private set
            {
                if(_state != value)
                {
                    _state = value;
                    NotifyStateChanged();
                }
            }
        }
        
        private List<GameCenterUser> _friends;

        public override List<GameCenterUser> Friends
        {
            get
            {
                return _friends;
            }
        }
        
        void LoginFinish(Error err, ErrorDelegate cbk = null)
        {
            if(!Error.IsNullOrEmpty(err))
            {
                State = States.Error;
            }
            else
            {
                State = States.LoggedIn;
            }
            if(cbk != null)
            {
                cbk(err);
            }
        }

        void LoginLoadPlayerData(ErrorDelegate cbk = null)
        {            
            if(!UnityEngine.Social.Active.localUser.authenticated)
            {
                _friends.Clear();
                _user = new GameCenterUser();
                State = States.LoggedOut;

                if(cbk != null)
                {
                    cbk(new Error("Could not login."));
                }
            }
            else
            {
                GameCenterUser user = new GameCenterUser(UnityEngine.Social.Active.localUser.id,
                                                         UnityEngine.Social.Active.localUser.userName,
                                                         UnityEngine.Social.Active.localUser.userName,
                                                         UnityEngine.Social.Active.localUser.underage ? GameCenterUser.AgeGroup.Underage : GameCenterUser.AgeGroup.Adult
                );
                if(_user != user)
                {
                    State = States.LoggingIn;
                }
                
                _user = user;

                if(cbk != null)
                {
                    cbk(null);
                }
            }
        }
        
        void LoginDownloadFriends(ErrorDelegate cbk, bool initial = true)
        {
            if((UnityEngine.Social.Active.localUser.friends == null || UnityEngine.Social.Active.localUser.friends.Length == 0))
            {

                UnityEngine.Social.Active.localUser.LoadFriends((bool success) =>
                {
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
            else if(UnityEngine.Social.Active.localUser.friends.Length > 0)
            {
                Friends.Clear();
                for(int k = 0; k < UnityEngine.Social.Active.localUser.friends.Length; k++)
                {
                    if(UnityEngine.Social.Active.localUser.friends[k] != null && UnityEngine.Social.Active.localUser.friends[k] is IUserProfile)
                    {
                        IUserProfile friendData = UnityEngine.Social.Active.localUser.friends[k];
                        
                        Friends.Add(new GameCenterUser(friendData.id,
                                                       friendData.userName,
                                                       friendData.userName
                        )
                        );
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
                UnityEngine.Social.Active.ReportProgress(achiId, Achievements[achiId], (bool success) =>
                {
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
            
            UnityEngine.Social.Active.LoadAchievements((IAchievement[] achievements) =>
            {
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
        
        public UnityGameCenter(bool playerVerification = true)
        {
            _friends = new List<GameCenterUser>();
            _user = new GameCenterUser();
            ShowLoginWindow = false;
            State = States.LoggedOut;
            PlayerVerification = playerVerification;
        }
        
        public override bool IsConnected
        {
            get
            {
                return State == States.LoggedIn;
            }
        }
        
        public override bool IsConnecting
        {
            get
            {
                return State == States.LoggingIn;
            }
        }
        
        public override bool HasError
        {
            get
            {
                return State == States.Error;
            }
        }
        
        public override void Login(ErrorDelegate cbk)
        {
            UnityEngine.Social.Active = new GameCenterPlatform();
            
            if(State != States.LoggedOut && State != States.Error)
            {
                if(cbk != null)
                {
                    Error err = null;
                    if(State == States.LoggingIn)
                    {
                        err = new Error("Currently logging in.");
                    }
                    else
                    {
                        err = new Error("Invalid game center state.");
                    }
                    cbk(err);
                }
                return;
            }
            
            State = States.LoggingIn;

            UnityEngine.Social.Active.localUser.Authenticate((bool success) =>
            {
                if(success)
                {
                    LoginLoadPlayerData((err) => {
                        if(!Error.IsNullOrEmpty(err))
                        {
                            LoginFinish(err, cbk);
                        }
                        else
                        {
                            LoginDownloadFriends((err2) => {
                                if(!Error.IsNullOrEmpty(err2))
                                {
                                    LoginFinish(err2, cbk);
                                }
                                else
                                {
                                    DownloadAchievements((err3) => {
                                        LoginFinish(err3, cbk);
                                    });
                                }
                            });
                        }
                    });
                }
                else
                {
                    LoginFinish(new Error("Could not login"), cbk);
                }
            });
        }
        
        public override void UpdateScore(GameCenterScore score, GameCenterScoreDelegate cbk = null)
        {
            if (State != States.LoggedIn)
            {
                if(cbk != null)
                {
                    cbk(score, new Error("GameCenter is not logged in"));
                }
                return;
            }

            UnityEngine.Social.Active.ReportScore(score.Value, score.Category, (bool success) =>
            {
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
            if (State != States.LoggedIn)
            {
                if(cbk != null)
                {
                    cbk(new Error("GameCenter is not logged in"));
                }
                return;
            }
            UnityEngine.SocialPlatforms.GameCenter.GameCenterPlatform.ResetAllAchievements((bool success) =>
            {
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
            if (State != States.LoggedIn)
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
            if (State != States.LoggedIn)
            {
                if(cbk != null)
                {
                    cbk(null, new Error("GameCenter is not logged in"));
                }
                return;
            }

            string tmpFilePath = Application.temporaryCachePath + "/" + PhotosCacheFolder + "/" + userId + "_" + photoSize.ToString() + ".png";
            UnityEngine.Social.Active.LoadUsers(new string[]{ userId }, (users) =>
            {
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
