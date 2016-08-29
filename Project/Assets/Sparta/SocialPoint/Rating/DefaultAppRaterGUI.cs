using SocialPoint.Alert;

namespace SocialPoint.Rating
{
    public sealed class DefaultAppRaterGUI : IAppRaterGUI
    {
        IAppRater _appRater;
        IAlertView _prototype;

        public DefaultAppRaterGUI(IAlertView proto)
        {
            _prototype = proto;
        }

        #region IAppRaterGUI implementation

        public bool Show(bool showLaterButton)
        {
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
                        _appRater.OnRequestResult(RateRequestResult.Accept);
                        break;
                    case 1:
                        _appRater.OnRequestResult(RateRequestResult.Decline);
                        break;
                    case 2:
                        _appRater.OnRequestResult(RateRequestResult.Delay);
                        break;
                    }
                });
                return true;
            }
            return false;
        }

        public void SetAppRater(IAppRater appRater)
        {
            _appRater = appRater;
        }

        #endregion

    }
}

