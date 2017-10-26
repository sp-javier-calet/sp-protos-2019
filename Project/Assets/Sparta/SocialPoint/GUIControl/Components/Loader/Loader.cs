using UnityEngine;
using SocialPoint.GUIControl;
using System.Collections;

public class Loader : MonoBehaviour 
{
//    [SerializeField]
//    UIViewAnimation _animation;

    IEnumerator _coroutine;

    void Awake()
    {
//        if(_animation == null)
//        {
//            _animation = new RotateAnimation(1f);
//        }
//        _animation.Load(transform);
//        _coroutine = _animation.Appear();
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
//        _animation = null;
        _coroutine = null;
    }
}
