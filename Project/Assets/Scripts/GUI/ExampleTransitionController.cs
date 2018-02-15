using UnityEngine;
using SocialPoint.GUIControl;

public class ExampleTransitionController : UIViewController
{
    protected override void OnLoad()
    {
        base.OnLoad();
        AppearAnimation = new UIToolAnimation("Appear");
        DisappearAnimation = new UIToolAnimation("Disappear");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.Log("ExampleTransitionController OnAppearing");
    }

    protected override void OnAppeared()
    {
        base.OnAppeared();
        Debug.Log("ExampleTransitionController OnAppeared");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Debug.Log("ExampleTransitionController OnDisappearing");
    }

    protected override void OnDisappeared()
    {
        base.OnDisappeared();
        Debug.Log("ExampleTransitionController OnDisappeared");
    }

    public void OnPointerClick()
    {
        Hide();
    }
}
