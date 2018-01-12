using System;
using System.Collections.Generic;
using AOT;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;

namespace SocialPoint.Social
{
    public sealed class IosGameCenter : IGameCenter
    {
        static readonly string PhotosCacheFolder = "GameCenter";
        GameCenterUser _user;

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

        public GameCenterUserVerification Verification { get; private set; }

        List<GameCenterAchievement> _achievements;
        bool _connecting;
        bool _connected;
        GameCenterPlatform _platform;
        List<GameCenterUser> _friends;
        HashSet<string> _achievementsUpdating;

        public List<GameCenterUser> Friends
        {
            get
            {
                return _friends;
            }
        }

        public event Action StateChangeEvent;

        void NotifyStateChanged()
        {
            if (StateChangeEvent != null)
            {
                StateChangeEvent();
            }
        }

        void OnLoginEnd(Error err, ErrorDelegate cbk = null)
        {
            _connecting = false;

            if (!Error.IsNullOrEmpty(err))
            {
#if !UNITY_EDITOR
                Log.e("Game Center login ended in error: " + err);
#endif
            }
            else
            {
                _connected = true;
            }

            NotifyStateChanged();
            if (cbk != null)
            {
                cbk(err);
            }
        }

        void LoginLoadPlayerData(ErrorDelegate cbk = null)
        {
            var localUser = _platform.localUser;
            if (!localUser.authenticated)
            {
                _friends.Clear();
                _user = new GameCenterUser();
                if (cbk != null)
                {
                    cbk(new Error("Could not login - LoginLoadPlayerData localUser.authenticated false"));
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

                GenerateUserVerification(cbk);
            }
        }

        void LoginDownloadFriends(ErrorDelegate cbk, bool initial = true)
        {
            var localUser = _platform.localUser;
            if ((localUser.friends == null || localUser.friends.Length == 0) && initial)
            {
                localUser.LoadFriends(success => {
                    if (success)
                    {
                        LoginDownloadFriends(cbk, false);
                    }
                    else
                    {
                        if (cbk != null)
                        {
                            cbk(new Error("Could not load friends."));
                        }
                    }
                });
                return;
            }
            Friends.Clear();
            if (localUser.friends != null)
            {
                for (int k = 0; k < localUser.friends.Length; k++)
                {
                    var friendData = localUser.friends[k];
                    if (friendData != null)
                    {
                        Friends.Add(new GameCenterUser(friendData.id,
                            friendData.userName,
                            friendData.userName));
                    }
                }
            }
            if (cbk != null)
            {
                cbk(null);
            }
        }

        void DownloadAchievements(ErrorDelegate cbk)
        {
            if (Achievements != null)
            {
                if (cbk != null)
                {
                    cbk(null);
                }
                return;
            }
            _platform.LoadAchievementDescriptions(descs => _platform.LoadAchievements(achis => {
                if (achis != null)
                {
                    _achievements = new List<GameCenterAchievement>();
                    for (int i = 0, descsLength = descs.Length; i < descsLength; i++)
                    {
                        var d = descs[i];
                        var percent = 0.0f;
                        for (int j = 0, achisLength = achis.Length; j < achisLength; j++)
                        {
                            var a = achis[j];
                            if (a.id == d.id)
                            {
                                percent = (float)a.percentCompleted;
                                break;
                            }
                        }
                        _achievements.Add(new GameCenterAchievement(d.id, percent, d.points, d.hidden, d.title, d.unachievedDescription, d.achievedDescription));
                    }
                }
                if (cbk != null)
                {
                    Error err = null;
                    if (_achievements == null)
                    {
                        err = new Error("Could not download achievements.");
                    }
                    cbk(err);
                }
            }));
        }

        public IosGameCenter(bool showAchievements = true)
        {
            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                throw new InvalidOperationException("This class works only on the iOS platform.");
            }
            _friends = new List<GameCenterUser>();
            _achievementsUpdating = new HashSet<string>();
            _user = new GameCenterUser();
            _platform = new GameCenterPlatform();
            GameCenterPlatform.ShowDefaultAchievementCompletionBanner(showAchievements);
        }

        public bool IsConnected
        {
            get
            {
                return _platform.localUser.authenticated && !_connecting && _connected;
            }
        }

        public bool IsConnecting
        {
            get
            {
                return _connecting;
            }
        }

        public bool IsAchievementUpdating(string achiId)
        {
            return _achievementsUpdating.Contains(achiId);
        }

        public void Login(ErrorDelegate cbk = null)
        {
            if (IsConnected)
            {
                if (cbk != null)
                {
                    cbk(null);
                }
                return;
            }
            _connecting = true;
            _connected = false;
            _platform.localUser.Authenticate((success, error) => {
                if (success)
                {
                    LoginLoadPlayerData(err => {
                        if (!Error.IsNullOrEmpty(err))
                        {
                            OnLoginEnd(err, cbk);
                        }
                        else
                        {
                            LoginDownloadFriends(err2 => {
                                if (!Error.IsNullOrEmpty(err2))
                                {
                                    OnLoginEnd(err2, cbk);
                                }
                                else
                                {
                                    DownloadAchievements(err3 => OnLoginEnd(err3, cbk));
                                }
                            });
                        }
                    });
                }
                else
                {
                    OnLoginEnd(new Error(string.Format("Could not login - GameCenterPlatform.localUser.Authenticate failed: {0}", error)), cbk);
                }
            });
        }

        public void UpdateScore(GameCenterScore score, GameCenterScoreDelegate cbk = null)
        {
            if (!IsConnected)
            {
                if (cbk != null)
                {
                    cbk(score, new Error("GameCenter is not logged in"));
                }
                return;
            }

            _platform.ReportScore(score.Value, score.Category, success => {
                if (cbk != null)
                {
                    Error err = null;
                    if (!success)
                    {
                        err = new Error(string.Format("Could not update score in '{0}'.", score.Category));
                    }
                    cbk(score, err);
                }
            });
        }

        public void ResetAchievements(ErrorDelegate cbk = null)
        {
            if (!IsConnected)
            {
                if (cbk != null)
                {
                    cbk(new Error("GameCenter is not logged in"));
                }
                return;
            }
            GameCenterPlatform.ResetAllAchievements(success => {
                if (!success)
                {
                    if (cbk != null)
                    {
                        var err = new Error("Could not reset achievements.");
                        cbk(err);
                    }
                }
                else
                {
                    for (int i = 0, _achievementsCount = _achievements.Count; i < _achievementsCount; i++)
                    {
                        var achi = _achievements[i];
                        achi.Percent = 0.0f;
                    }
                    _platform.LoadAchievements(achis => {
                        if (cbk != null)
                        {
                            cbk(null);
                        }
                    });
                }
            });
        }

        public void UpdateAchievement(GameCenterAchievement achi, GameCenterAchievementDelegate cbk = null)
        {
            if (!IsConnected)
            {
                if (cbk != null)
                {
                    cbk(achi, new Error("GameCenter is not logged in"));
                }
                return;
            }
            if (Achievements == null)
            {
                if (cbk != null)
                {
                    cbk(achi, new Error("Failed to download the list of existing achievements"));
                }
                return;
            }

            GameCenterAchievement achievement = GetAchievementFromId(achi.Id);
            Error err = null;

            if (achievement == null)
            {
                if (cbk != null)
                {
                    cbk(achi, new Error("Achievement not found"));
                }
                return;
            }

            if (achievement.IsUnlocked)
            {
                if (cbk != null)
                {
                    cbk(achievement, err);
                }
                return;
            }

            var achiId = achi.Id;
            var achiPercent = achi.Percent;

            if (_achievementsUpdating.Contains(achiId))
            {
                if (cbk != null)
                {
                    cbk(achievement, err);
                }
                return;
            }

            _achievementsUpdating.Add(achiId);

            _platform.ReportProgress(achiId, achiPercent, success => {
                if (cbk != null)
                {
                    if (!success)
                    {
                        err = new Error(string.Format("Error updating achievement '{0}'.", achiId));
                    }
                    else
                    {
                        achievement.Percent = Mathf.Min(achiPercent, 100.0f);
                    }
                    cbk(achievement, err);
                }
                _achievementsUpdating.Remove(achiId);
            });
        }

        GameCenterAchievement GetAchievementFromId(string achiId)
        {
            for (int i = 0, achievementsCount = _achievements.Count; i < achievementsCount; i++)
            {
                if (_achievements[i].Id == achiId)
                {
                    return _achievements[i];
                }
            }
            return null;
        }

        public void LoadPhoto(string userId, uint photoSize, GameCenterPhotoDelegate cbk)
        {
            if (!IsConnected)
            {
                if (cbk != null)
                {
                    cbk(null, new Error("GameCenter is not logged in"));
                }
                return;
            }

            string tmpFilePath = Application.temporaryCachePath + "/" + PhotosCacheFolder + "/" + userId + "_" + photoSize + ".png";
            _platform.LoadUsers(new[] { userId }, users => {
                Error err;
                if (users == null || users.Length == 0)
                {
                    err = new Error(string.Format("User with id {0} not found.", userId));
                }
                else
                {
                    var u = users[0];
                    if (u.image != null)
                    {
                        var newTexture = Texture2D.Instantiate(u.image);
                        newTexture.Resize((int)photoSize, (int)photoSize);
                        err = ImageUtils.SaveTextureToFile(newTexture, tmpFilePath);
                    }
                    else
                    {
                        tmpFilePath = null;
                        err = new Error(string.Format("User with id {0} does not have an image.", userId));
                    }
                }

                if (cbk != null)
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

        void GenerateUserVerification(ErrorDelegate cbk)
        {
            if(_verificationCallback != null) {
                if(cbk != null) {
                    cbk(new Error("User verification generation is already in process."));
                }
                return;
            }
            _verificationCallback = (uv, err) => {
                Verification = uv;
                if (cbk != null)
                {
                    cbk(err);
                }
            };
            // native callbacks need to be static
            SPUnityGameCenter_GenerateUserVerification(GenerateUserVerificationCallback);
        }

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void SPUnityGameCenter_GenerateUserVerification(Action<string, string> callback);

        static Action<GameCenterUserVerification, Error> _verificationCallback;

        [MonoPInvokeCallback(typeof(Action<string, string>))]
        static void GenerateUserVerificationCallback(string verification, string errorString)
        {
            var error = Error.FromString(errorString);
            GameCenterUserVerification uv = null;
            if (!string.IsNullOrEmpty(verification))
            {
                var parser = new JsonAttrParser();
                var data = parser.ParseString(verification).AsDic;
                var url = data.GetValue("url").ToString();
                var signature = Convert.FromBase64String(data.GetValue("signature").ToString());
                var salt = Convert.FromBase64String(data.GetValue("salt").ToString());
                var time = (ulong)data.GetValue("timestamp").ToLong();
                uv = new GameCenterUserVerification(url, signature, salt, time);
            }
            if (_verificationCallback != null)
            {
                var cbk = _verificationCallback;
                _verificationCallback = null;
                cbk(uv, error);
            }
        }
    }
}
