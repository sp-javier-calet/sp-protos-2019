using System;
using UnityEngine;

namespace SocialPoint.GameLoading
{
    public class LoadingOperation
    {
        public delegate void ProgressChanged(string message);

        public event ProgressChanged ProgressChangedEvent;

        public float Progress { private set; get; }

        float _elapsed = 0;
        float _finishedAt = 0;
        float _fakeDuration;
        float _speedUpTime;//time to end progress when real action is finished
        
        public float FakeProgress { private set; get; }

        public LoadingOperation(float fakeDuration = 2, float speedUpTime = 0.35f)
        {
            FakeProgress = 0;
            Progress = 0;
            _fakeDuration = fakeDuration;
            _speedUpTime = speedUpTime;
        }

        public void UpdateProgress(float newProgress, string message = "")
        {
            Progress = newProgress;
            ProgressChangedEvent(message);
            if(Progress == 1)
            {
                _finishedAt = _elapsed;
            }
        }

        public void FinishProgress(string message = null)
        {
            UpdateProgress(1f, message);
        }

        public void Update(float elapsed)
        {
            _elapsed += elapsed;
            if(FakeProgress < 1)
            {
                if(_finishedAt != 0)
                {
                    FakeProgress = Mathf.Lerp(FakeProgress, 1, (_elapsed - _finishedAt) / _speedUpTime);
                }
                else
                {   
                    FakeProgress = _elapsed / _fakeDuration;
                }
            }
            else
            {
                FakeProgress = 1;
            }
        }
    }
}
