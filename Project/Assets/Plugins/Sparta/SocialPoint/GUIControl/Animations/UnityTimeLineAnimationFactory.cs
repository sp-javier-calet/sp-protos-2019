using UnityEngine;
using System.Collections;
using UnityEngine.Playables;

namespace SocialPoint.GUIControl
{
    /// <summary>
    /// This class can execute TimeLine animations.
    /// </summary>
    public class UnityTimeLineAnimation : UIViewAnimation
    {
        PlayableDirector _director;

        public void Load(GameObject gameObject)
        {
            _director = gameObject.GetComponent<PlayableDirector>();
            if(_director == null)
            {
                throw new MissingComponentException("Missing TimeLine director component in UIViewAnimation Load");
            }
        }

        public UnityTimeLineAnimation(PlayableDirector director)
        {
            _director = director;
        }

        public IEnumerator Animate()
        {
            if(_director != null)
            {
                _director.Play();
                // Here we need to track exactly that the timeline has finished playing, but it seems that until
                // Unity 2018.2 or 2018.3 we will not have events to track state changes
                while(_director.state == PlayState.Playing) 
                {
                    yield return null;
                }

                _director.Stop();
            }
        }
    }

    [CreateAssetMenu(menuName = "UI Animations/Unity TimeLine Animation")]
    public class UnityTimeLineAnimationFactory : UIViewAnimationFactory
    {
        public PlayableDirector Director;

        public override UIViewAnimation Create()
        {
            return new UnityTimeLineAnimation(Director);
        }
    }
}
