using System;
using FyberPlugin;
using SocialPoint.Base;
using SocialPoint.Login;
using UnityEngine;

namespace SocialPoint.VideoAds
{
    public sealed class SocialPointVideoAdsManager : MonoBehaviour, IVideoAdsManager
    {
        string _appId;

        public string AppId
        {
            set
            {
                _appId = value;
            }
        }

        string _securityToken;

        public string SecurityToken
        {
            set
            {
                _securityToken = value;
            }
        }

        public ILoginData LoginData { get; set; }

        bool _enabled;
        Ad _rewardedVideoAd;

        //fix for adcolony
        #if UNITY_ANDROID && !UNITY_EDITOR
        bool was_paused = true;

        void Start()
        {
            FyberAdColonyFix.AndroidInitializePlugin();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            was_paused = true;
            FyberAdColonyFix.AndroidPause();
        }

        void Update()
        {
            if (was_paused)
            {
                was_paused = false;
                FyberAdColonyFix.AndroidResume();
            }
        }
        #endif


        #region IVideoAdsManager implementation

        public event Action AdStartedEvent;

        public event Action AdFinishedEvent;

        public void RequestAd(RequestVideoDelegate cbk)
        {
            DebugUtils.Assert(cbk != null);
            if(AdAvailable)
            {
                cbk(null, RequestVideoResult.Available);
                return;
            }
            RewardedVideoRequester.Create()
                .NotifyUserOnCompletion(false)
                .WithCallback(new SocialPointVideoAdsManager.SPFyberRequestCallback(cbk, this))
                .Request();
        }

        public void ShowAd(ShowVideoDelegate cbk)
        {
            DebugUtils.Assert(cbk != null);
            if(!AdAvailable)
            {
                cbk(new Error("VideoAd not requested"), ShowVideoResult.Error);
                return;
            }
            _rewardedVideoAd.WithCallback(new SocialPointVideoAdsManager.SPFyberAdCallback(cbk, this))
                .Start();
        }

        public void Enable()
        {
            DebugUtils.Assert(_appId != null);
            DebugUtils.Assert(LoginData != null);
            DebugUtils.Assert(_securityToken != null);
            Fyber.With(_appId).WithUserId(LoginData.UserId.ToString()).WithSecurityToken(_securityToken).Start();
            FyberCallback.NativeError += OnNativeExceptionReceivedFromSDK;
            _enabled = true;
        }

        public void Disable()
        {
            FyberCallback.NativeError -= OnNativeExceptionReceivedFromSDK;
            _enabled = false;
        }

        public bool AdAvailable
        {
            get
            {
                return (_rewardedVideoAd != null);
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _enabled;
            }
        }

        #endregion

        public void OnNativeExceptionReceivedFromSDK(string message)
        {
            //handle exception
        }

        #region IDisposable implementation

        public void Dispose()
        {
            Disable();
        }

        #endregion

        class SPFyberRequestCallback : RequestCallback
        {
            RequestVideoDelegate _cbk;
            SocialPointVideoAdsManager _manager;

            public SPFyberRequestCallback(RequestVideoDelegate cbk, SocialPointVideoAdsManager manager)
            {
                _cbk = cbk;
                _manager = manager;
            }

            #region RequestCallback implementation

            public void OnAdAvailable(Ad ad)
            {                
                _manager._rewardedVideoAd = ad;
                _cbk(null, RequestVideoResult.Available);
            }

            public void OnAdNotAvailable(AdFormat adFormat)
            {
                _cbk(null, RequestVideoResult.NotAvailable);
            }

            #endregion

            #region Callback implementation

            public void OnRequestError(RequestError error)
            {
                _cbk(new Error(error.Description), RequestVideoResult.Error);
            }

            #endregion
        }

        class SPFyberAdCallback : AdCallback
        {
            const string CloseFinished = "CLOSE_FINISHED";
            const string CloseAborted = "CLOSE_ABORTED";
            const string CloseError = "ERROR";

            ShowVideoDelegate _cbk;
            SocialPointVideoAdsManager _manager;

            public SPFyberAdCallback(ShowVideoDelegate cbk, SocialPointVideoAdsManager manager)
            {
                _cbk = cbk;
                _manager = manager;
            }

            #region AdCallback implementation

            public void OnAdStarted(Ad ad)
            {
                if(_manager._rewardedVideoAd == ad)
                {
                    _manager._rewardedVideoAd = null;
                    var handler = _manager.AdStartedEvent;
                    if(handler != null)
                        handler();
                }
            }

            public void OnAdFinished(AdResult result)
            {
                var handler = _manager.AdFinishedEvent;
                if(handler != null)
                    handler();

                if(result.Status == AdStatus.Error)
                    _cbk(new Error("AdResult Status was Error"), ShowVideoResult.Error);

                switch(result.Message)
                {
                case CloseFinished:
                    _cbk(null, ShowVideoResult.Finished);
                    break;
                case CloseAborted:
                    _cbk(null, ShowVideoResult.Aborted);
                    break;
                case CloseError:
                    _cbk(null, ShowVideoResult.Error);
                    break;
                }
            }

            #endregion

        }
    }
}

