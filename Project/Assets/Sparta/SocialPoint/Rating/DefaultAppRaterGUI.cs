using SocialPoint.Alert;
using SocialPoint.Utils;
using System;

namespace SocialPoint.Rating
{
    public sealed class DefaultAppRaterGUI : IAppRaterGUI
    {
        IAlertView _prototype;
        bool _nativeRateDialog;
        INativeUtils _nativeUtils;

        public DefaultAppRaterGUI(IAlertView proto, INativeUtils nativeUtils, bool nativeRateDialog)
        {
            _prototype = proto;
            _nativeUtils = nativeUtils;
            // only enable native dialog if platform supports it
            if(_nativeUtils != null && !_nativeUtils.SupportsReviewDialog)
            {
                nativeRateDialog = false;
            }
            _nativeRateDialog = nativeRateDialog;
        }

        #region IAppRaterGUI implementation

        public bool Show(bool showLaterButton)
        {
            if(_nativeRateDialog)
            {
                //native dialog should be called directly without asking
                //since it can show up or not depending on app store guidelines
                AppRater.OnRequestResult(RateRequestResult.Accept);
                return true;
            }
            var alert = _prototype.Clone() as IAlertView;
            if(alert != null)
            {
                alert.Title = "Rate this app";
                alert.Message = "Help us rating this app";
                alert.Buttons = showLaterButton ? new[] {
                    "Ok",
                    "Cancel",
                    "Later"
                } : new[] {
                    "Ok",
                    "Cancel"
                }; 
                alert.Input = false;
                alert.Show(result => {
                    switch(result)
                    {
                    case 0:
                        AppRater.OnRequestResult(RateRequestResult.Accept);
                        break;
                    case 1:
                        AppRater.OnRequestResult(RateRequestResult.Decline);
                        break;
                    case 2:
                        AppRater.OnRequestResult(RateRequestResult.Delay);
                        break;
                    }
                });
                return true;
            }
            return false;
        }

        public void Rate()
        {
            if(_nativeUtils == null)
            {
                throw new InvalidOperationException("No native utils found.");
            }
            else if(_nativeRateDialog)
            {
                _nativeUtils.DisplayReviewDialog();
            }
            else
            {
                _nativeUtils.OpenReview();
            }
        }

        public IAppRater AppRater{ get; set; }

        #endregion

    }
}

