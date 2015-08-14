
using Zenject;
using SocialPoint.GUI;
using UnityEngine;

public class ScreensController : UIStackController
{
    [InjectOptional("first_screen")]
    GameObject FirstScreen;

    [PostInject]
    public void PostInject()
    {
        if(FirstScreen != null)
        {
            ReplaceImmediate(FirstScreen);
        }
    }

}