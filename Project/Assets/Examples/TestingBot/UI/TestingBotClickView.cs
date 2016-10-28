using UnityEngine;
using System.Collections;
using SocialPoint.EventSystems;

public class TestingBotClickView : MonoBehaviour
{
    [SerializeField]
    Animator _animator;

    [SerializeField]
    float _minDuration = 1f;

    MouseAction _mouseAction;
    Canvas _canvas;

    bool _started;
    bool _finished;

    float _elapsedTime = 0f;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Init(MouseAction a, Canvas canvas)
    {
        _mouseAction = a;
        _mouseAction.Started += OnStarted;
        _mouseAction.Finished += OnFinished;
        _canvas = canvas;
    }

    void OnFinished(MouseAction obj)
    {
        _finished = true;
    }

    void OnStarted(MouseAction obj)
    {
        _started = true;
        gameObject.SetActive(true);
        _animator.Play("Click");
    }

    void Update()
    {
        if(_started)
        {
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform, _mouseAction.Position, _canvas.worldCamera, out position);
            transform.position = _canvas.transform.TransformPoint(position);

            _elapsedTime += Time.unscaledDeltaTime;
            if(_finished && _elapsedTime >= _minDuration)
            {
                _started = false;
                _animator.Play("Unclick");
                GameObject.Destroy(gameObject, 1f);
            }
        }
    }
}
