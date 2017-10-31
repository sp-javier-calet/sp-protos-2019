using UnityEngine;
using SocialPoint.GUIControl;
using System.Collections;

namespace SocialPoint.GUIControl
{
    public class Loader : MonoBehaviour 
    {
        [SerializeField]
        UIViewAnimation _animation;

        IEnumerator _coroutine;

        void Awake()
        {
            if(_animation == null)
            {
                _animation = new RotateForEverAnimation();
            }
            _animation.Load(transform);
            _coroutine = _animation.Animate();
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
