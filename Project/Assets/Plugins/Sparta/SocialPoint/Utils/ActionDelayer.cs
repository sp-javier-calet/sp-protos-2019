using UnityEngine;
using System;
using System.Collections;

namespace SocialPoint.Utils
{
    //TODO: Avoid using a singleton? Group this behaviour with others that require a GameObject in scene and instantiate only one with all scripts?
    public sealed class ActionDelayer : MonoBehaviourSingleton<ActionDelayer>
    {
        /// <summary>
        /// Fires an action after an amount of time.
        /// </summary>
        /// <param name="action">Action to execute after a delay.</param>
        /// <param name="seconds">time in seconds to delay the action.</param>
        /// <param name="timeScaleDependant">If set to <c>true</c> the delay will be affected by Time.timeScale.</param>
        public void FireActionWithDelay(System.Action action, float seconds, bool timeScaleDependant = false)
        {
            StartCoroutine(DelayedAction(action, seconds, timeScaleDependant));
        }

        private IEnumerator DelayedAction(System.Action action, float seconds, bool timeScaleDependant)
        {
            if(timeScaleDependant)
            {
                yield return new WaitForSeconds(seconds);
            }
            else
            {
                float startTime = Time.realtimeSinceStartup;
                float elapsedTime = 0.0f;
                while(elapsedTime < seconds)
                {
                    yield return null;
                    elapsedTime = Time.realtimeSinceStartup - startTime;
                }
            }

            //Do Action after delay
            action();
        }
    }
}
