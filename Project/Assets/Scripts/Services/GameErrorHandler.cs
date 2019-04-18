//-----------------------------------------------------------------------
// GameErrorHandler.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.Helpshift;
using SocialPoint.Locale;
using SocialPoint.Restart;

class GameErrorHandler : SocialPoint.GameLoading.GameErrorHandler
{
    public GameErrorHandler(IAlertView alert, Localization locale, IAppEvents appEvents, IRestarter restarter,
        IHelpshift helpshift, bool debug) :
        base(alert, locale, appEvents, restarter, helpshift)
    {
        Debug = debug;
    }
}
