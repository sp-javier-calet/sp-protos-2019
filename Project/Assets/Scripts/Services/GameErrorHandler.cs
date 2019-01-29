using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.GUIControl;
using SocialPoint.Helpshift;
using SocialPoint.Locale;
using SocialPoint.Restart;
using UnityEngine;

class GameErrorHandler : SocialPoint.GameLoading.GameErrorHandler
{
    public GameErrorHandler(IAlertView alert, Localization locale, IAppEvents appEvents, IRestarter restarter, IHelpshift helpshift, bool debug) :
    base(alert, locale, appEvents, FindPopups, restarter, helpshift)
    {
        Debug = debug;
    }

    static UIStackController FindPopups()
    {
        return GameObject.FindObjectOfType<PopupsController>();
    }
}
