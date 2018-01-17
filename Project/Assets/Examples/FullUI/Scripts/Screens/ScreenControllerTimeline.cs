using System.Collections.Generic;
using SocialPoint.GUIControl;
using UnityEngine;
using UnityEngine.Playables;

public class ScreenControllerTimeline : UIViewController
{
    public List<PlayableDirector> PlayableDirectors = new List<PlayableDirector>();

    public void ExecuteTweenScale()
    {
        if(PlayableDirectors.Count > 0)
        {
            var playable = PlayableDirectors[0];
            if(playable != null)
            {
                if(playable.state == PlayState.Playing)
                {
                    playable.Pause();
                }
                else
                {
                    playable.Play();
                }
            }
        }
    }
}