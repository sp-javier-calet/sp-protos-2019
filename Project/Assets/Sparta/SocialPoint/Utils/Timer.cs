﻿using UnityEngine;
using System.Collections;

namespace SocialPoint.Utils
{
    public sealed class Timer
    {
        float _startTime = 0f;
        float _endTime = 0f;
        bool _isCanceled = false;
        bool _ignoreTimeScale = false;

        public Timer()
        {
            _startTime = 0f;
            _endTime = 0f;
        }

        float GetCurrentTime()
        {
            return _ignoreTimeScale ? Time.unscaledTime : Time.time;
        }

        public void Wait(float waitTime)
        {
            _startTime = GetCurrentTime();
            _endTime = _startTime + waitTime;
            _isCanceled = false;
        }

        public bool IsWaiting
        {
            get
            {
                if(_isCanceled)
                {
                    return false;
                }
                return !IsFinished;
            }
        }

        public bool IsFinished
        {
            get
            {
                if(_isCanceled)
                {
                    return false;
                }
                return GetCurrentTime() >= _endTime;
            }
        }

        public float Duration
        {
            get
            {
                return _endTime - _startTime;
            }
        }

        public void Cancel()
        {
            _isCanceled = true;
        }

        public float Delta
        {
            get
            {
                return Mathf.Max(GetCurrentTime() - _startTime, 0f);
            }
        }

        public float DeltaNormalized
        {
            get
            {
                float dt = Delta / (_endTime - _startTime);

                return Mathf.Clamp(dt, 0f, 1f);
            }
        }

        public bool IgnoreTimeScale
        {
            get
            {
                return _ignoreTimeScale;
            }
            set
            {
                _ignoreTimeScale = value;
            }
        }
    }
}
