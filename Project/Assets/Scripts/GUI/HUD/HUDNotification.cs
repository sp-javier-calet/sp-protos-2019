//-----------------------------------------------------------------------
// HUDNotification.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System;

public class HUDNotification : MonoBehaviour
{
    [SerializeField]
    Text _text;

    public event Action<HUDNotification> Finished;

    const float _fadeInTime = 0.3f;
    const float _fadeOutTime = 0.3f;
    float _remainingTime = 0f;
    float _totalTime = 0f;
    float _alpha;
    Color _color;

    public void Show(string text, Color color, float duration)
    {
        _color = color;
        _alpha = color.a;
        color.a = 0f;
        _text.color = color;
        _text.text = text;
        _remainingTime = _totalTime = duration;
    }

    void Update()
    {
        if(_remainingTime > 0f)
        {
            _remainingTime -= Time.deltaTime;
            if(_remainingTime <= 0f)
            {
                if(Finished != null)
                {
                    Finished(this);
                }
            }
            else if(_remainingTime >= _totalTime - _fadeInTime)
            {
                Color color = _color;
                color.a = _alpha * (1f - ((_remainingTime - (_totalTime - _fadeInTime)) / _fadeInTime));
                _text.color = color;
            }
            else if(_remainingTime <= _fadeOutTime)
            {
                Color color = _color;
                color.a = _alpha * _remainingTime / _fadeOutTime;
                _text.color = color;
            }
            else
            {
                _text.color = _color;
            }
        }
    }
}


