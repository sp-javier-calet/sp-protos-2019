
using Zenject;
using SocialPoint.GUI;
using UnityEngine;

public class ScreensController : UIStackController, IInitializable
{
    [InjectOptional("first_screen")]
    GameObject FirstScreen;

    [PostInject]
    public void Initialize()
    {
        if(FirstScreen != null)
        {
            Push(FirstScreen);
        }
    }
}