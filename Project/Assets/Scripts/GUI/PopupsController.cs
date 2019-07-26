//-----------------------------------------------------------------------
// PopupsController.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.GUIControl;
using SocialPoint.Dependency;
using UnityEngine;

public class PopupsController : UIStackController
{
    protected override void OnLoad()
    {
        float animationTime = Services.Instance.Resolve<float>("popup_animation_time");
        AppearAnimation = new FadeAnimation(animationTime, 0f, 1f);
        DisappearAnimation = new FadeAnimation(animationTime, 1f, 0f);

        base.OnLoad();
    }
}
