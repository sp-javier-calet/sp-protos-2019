using System;
using SocialPoint.Base;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class BaseTweenPlayableMixerBehaviour : PlayableBehaviour
    {
        protected bool _firstFrameHappened;

        [System.Diagnostics.Conditional(DebugFlags.DebugGUIControlFlag)]
        protected virtual void DebugLog(string msg)
        {
            Log.i(string.Format("PlayableMixerBehaviour msg {0}", msg));
        }

        protected static float GetTweenProgress(ScriptPlayable<BaseTweenPlayableBehaviour> playableInput, BaseTweenPlayableBehaviour playableBehaviour)
        {
            var time = playableInput.GetTime();
            var normalisedTime = (float)(time * playableBehaviour.InverseDuration);

            if(playableBehaviour.AnimationType == BaseTweenPlayableBehaviour.TweeningType.AnimationCurve && playableBehaviour.AnimationCurve != null)
            {
                return playableBehaviour.AnimationCurve.Evaluate(normalisedTime);
            }
            else
            {
               return playableBehaviour.EaseType.ToFunction()(normalisedTime, 0f, 1f, 1f);
            }
        }
    }
}