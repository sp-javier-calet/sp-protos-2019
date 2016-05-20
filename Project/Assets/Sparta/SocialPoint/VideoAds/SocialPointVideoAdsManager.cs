using FyberPlugin;
using SocialPoint.Base;
using System;
using UnityEngine.Assertions;

namespace SocialPoint.VideoAds
{
    public class SocialPointVideoAdsManager : IVideoAdsManager
    {
        string _appId;

        public string AppId
        {
            set
            {
                _appId = value;
            }
        }

        string _userId;

        public string UserId
        {
            set
            {
                _userId = value;
            }
        }

        bool _enabled = false;
        Ad _rewardedVideoAd;

        #region IVideoAdsManager implementation

        public event Action AdStartedEvent;

        public event Action AdFinishedEvent;

        public void RequestAd(RequestVideoDelegate cbk = null)
        {
            if(_rewardedVideoAd != null)
            {
                cbk(new Error("VideoAd already requested"), RequestVideoResult.Error);
                return;
            }
            RewardedVideoRequester.Create()
                .NotifyUserOnCompletion(false)
                .WithCallback(new SocialPointVideoAdsManager.SPFyberRequestCallback(cbk, this))
                .Request();
        }

        public void ShowAd(ShowVideoDelegate cbk = null)
        {
            if(_rewardedVideoAd != null)
            {
                _rewardedVideoAd.WithCallback(new SocialPointVideoAdsManager.SPFyberAdCallback(cbk, this))
                    .Start();
            }
        }

        public void Enable()
        {
            Assert.IsNotNull(_appId);
            Assert.IsNotNull(_userId);
            Fyber.With(_appId).WithUserId(_userId).Start();
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

