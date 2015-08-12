
using SocialPoint.Alert;

namespace SocialPoint.AppRater
{
    public class DefaultAppRaterGUI : IAppRaterGUI
    {
        AppRater _appRater;
        IAlertView _prototype;
        
        public DefaultAppRaterGUI(IAlertView proto)
        {
            _prototype = proto;
        }

        #region IAppRaterGUI implementation

        public void Show(bool showLaterButton)
        {
            var alert = _prototype.Clone() as IAlertView;
            if(alert != null)
            {
                alert.Title = "Rate this app";
                alert.Message = "Help us rating this app";
                if(showLaterButton)
                {
                    alert.Buttons = new string[]{ "Ok", "Cancel", "Later" };
                }
                else
                {
                    alert.Buttons = new string[]{ "Ok", "Cancel" };
                } 
                alert.Input = true;
                alert.Show((int result) => {
                    switch(result)
                    {
                    case 0:
                        _appRater.RequestAccepted();
                        break;
                    case 1:
                        _appRater.RequestDeclined();
                        break;
                    case 2:
                        _appRater.RequestDelayed();
                        break;
                    }
                });
            }
        }

        public void setAppRater(AppRater appRater)
        {
            _appRater = appRater;
        }

        #endregion

    }
}

