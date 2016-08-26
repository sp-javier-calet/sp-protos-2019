using UnityEngine;
using System.Collections;

namespace SocialPoint.Utils
{
    public sealed class Timer
    {
        float _startTime = 0f;
        float _endTime = 0f;
        bool _isCanceled = false;

        public Timer()
        {
            _startTime = 0f;
            _endTime = 0f;
        }

        public void Wait(float waitTime)
        {
            _startTime = Time.timeSinceLevelLoad;
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
                return Time.timeSinceLevelLoad < _endTime;
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
                return Time.timeSinceLevelLoad >= _endTime;
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
                return Time.timeSinceLevelLoad - _startTime;
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
    }
}
