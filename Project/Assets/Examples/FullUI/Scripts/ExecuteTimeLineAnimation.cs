using UnityEngine;
using UnityEngine.Playables;

public class ExecuteTimeLineAnimation : MonoBehaviour 
{
    [SerializeField]
    PlayableDirector _playableDirector;

    public void ExecuteTimeLineAnimartion()
    {
        if(_playableDirector != null)
        {
            if(_playableDirector.state == PlayState.Playing)
            {
                _playableDirector.Pause();
            }
            else
            {
                _playableDirector.Play();
            }
        }
    }
}
