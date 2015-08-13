
using Zenject;
using SocialPoint.GUI;
using UnityEngine;

public class ScreensController : UIStackController
{
    [InjectOptional("first_screen")]
    GameObject FirstScreen;

    [PostInject]
    public void LoadFirstScreen()
    {
        if(FirstScreen != null)
        {
            ReplaceImmediate(FirstScreen);
        }
    }

}