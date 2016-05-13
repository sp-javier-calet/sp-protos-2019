
using SocialPoint.Hardware;
using SocialPoint.Alert;
using SocialPoint.Locale;
using SocialPoint.AppEvents;
using SocialPoint.GUIControl;
using SocialPoint.Dependency;
using UnityEngine;

class GameErrorHandler : SocialPoint.GameLoading.GameErrorHandler
{
    public GameErrorHandler(IAlertView alert, Localization locale, IAppEvents appEvents, bool debug):
        base(alert, locale, appEvents, FindPopups)
    {
        Debug = debug;
    }

    static UIStackController FindPopups()
    {
        return GameObject.FindObjectOfType<PopupsController>();
    }
}
