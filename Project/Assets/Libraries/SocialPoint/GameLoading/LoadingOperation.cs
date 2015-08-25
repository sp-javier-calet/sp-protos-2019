using System;
using System.Collections;
using UnityEngine;

namespace SocialPoint.GameLoading
{
    public class LoadingOperation
    {
        public delegate void ProgressChanged(string message);

        public event ProgressChanged ProgressChangedEvent;

        public float progress { private set; get; }

        public void UpdateProgress(float newProgress, string message = "")
        {
            progress = newProgress;
            ProgressChangedEvent(message);
        }

        public void FinishProgress(string message = null)
        {
            UpdateProgress(1f, message);
        }
    }
}
