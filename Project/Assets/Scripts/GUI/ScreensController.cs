using SocialPoint.GUIControl;
using SocialPoint.Dependency;
using UnityEngine;

public class ScreensController : UIStackController
{
    #if UNITY_EDITOR || UNITY_ANDROID
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            OnSpecialButtonClickedEvent();
        }
    }
    #endif
}