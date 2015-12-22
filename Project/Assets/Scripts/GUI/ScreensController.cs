using SocialPoint.EventSystems;
using Zenject;
using SocialPoint.GUIControl;
using UnityEngine;

public class ScreensController : UIStackController
{
    [SerializeField]
    ActionEventSystem _actionEventSystem;

    override protected void OnAppearing()
    {
        if(_actionEventSystem != null)
        {
            _actionEventSystem.Enabled = false;
        }
        base.OnAppearing();
    }

    override protected void OnDisappeared()
    {
        if(_actionEventSystem != null)
        {
            _actionEventSystem.Enabled = true;
        }
        base.OnDisappeared();
    }
}