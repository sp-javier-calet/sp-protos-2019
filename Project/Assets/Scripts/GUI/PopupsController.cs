﻿using SocialPoint.GUIControl;
using Zenject;

public class PopupsController : UIStackController
{
    public const float DefaultFadeSpeed = 4.0f;

    [Inject("popup_fade_speed")]
    public float FadeSpeed = DefaultFadeSpeed;

    override protected void OnLoad()
    {
        ChildAnimation = new FadeAnimation(FadeSpeed);
        base.OnLoad();
    }
}