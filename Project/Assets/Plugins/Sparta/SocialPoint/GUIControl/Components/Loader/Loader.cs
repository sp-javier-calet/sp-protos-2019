using UnityEngine;
using SocialPoint.GUIControl;
using System.Collections;

namespace SocialPoint.GUIControl
{
    public class Loader : MonoBehaviour 
    {
        [SerializeField]
        UIViewAnimationFactory _animationFactory;

        UIViewAnimation _animation;
        IEnumerator _coroutine;

        void Awake()
        {
            if(_animationFactory != null)
            {
                _animation = _animationFactory.Create();
                if(_animation != null)
                {
                    _animation.Load(gameObject);
                    _coroutine = _animation.Animate();
                }
            }
        }

        void OnEnable()
        {
            StartCoroutine(_coroutine);
        }

        void OnDisable()
        {
            StopCoroutine(_coroutine);
        }

        void OnDestroy()
        {
            _animation = null;
            _coroutine = null;
        }
    }
}
