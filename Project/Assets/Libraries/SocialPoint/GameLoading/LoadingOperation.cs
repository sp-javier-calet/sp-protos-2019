using System;
using System.Collections;
using UnityEngine;

namespace SocialPoint.GameLoading
{
    public class LoadingOperation
    {
        public delegate void ProgressChanged(string message);

        public event ProgressChanged ProgressChangedEvent;

        public float Progress { private set; get; }

        public void UpdateProgress(float progress, string message = "")
        {
            Progress = progress;
            ProgressChangedEvent(message);
        }

        public void FinishProgress(string message = null)
        {
            UpdateProgress(1f, message);
        }

        //TODO: delete used as mock for develop
        public IEnumerator FakeLoadingProcess(float time)
        {
            var elapsed = 0f;
            while(elapsed < time)
            {
                elapsed += Time.deltaTime;
                if(elapsed < time)
                    UpdateProgress((elapsed / time));
                yield return null;
            }
            FinishProgress("Faked loading finished");
        }
    }
}
