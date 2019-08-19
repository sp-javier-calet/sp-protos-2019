//-----------------------------------------------------------------------
// GameLoadingController.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.GameLoading;

public class GameLoadingController : LoadingController
{
    public override bool OnBeforeClose()
    {
        return false;
    }
}
