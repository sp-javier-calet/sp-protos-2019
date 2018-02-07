using System.Collections.Generic;
using SocialPoint.GUIControl;
using UnityEngine;
using UnityEngine.Playables;

public class ScreenControllerTimeline : UIViewController
{
    public PlayableDirector PlayableDirectors;

    public void ExecuteTweenScale()
    {
        if(PlayableDirectors != null)
        {
            if(PlayableDirectors.state == PlayState.Playing)
            {
                PlayableDirectors.Pause();
            }
            else
            {
                PlayableDirectors.Play();
            }
        }
    }
}