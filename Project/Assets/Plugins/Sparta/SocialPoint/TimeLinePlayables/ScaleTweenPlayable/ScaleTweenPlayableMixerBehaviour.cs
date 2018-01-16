using System;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class ScaleTweenPlayableMixerBehaviour : PlayableBehaviour
    {
        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var trackBinding = playerData as Transform;

            if(trackBinding == null)
            {
                return;
            }

            var defaultScale = trackBinding.localScale;
            var inputCount = playable.GetInputCount();
            var blendedScale = Vector3.zero;
            var scaleTotalWeight = 0f;

            for(int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<ScaleTweenPlayableBehaviour>)playable.GetInput(i);
                var playableBehaviour = playableInput.GetBehaviour();

                if(playableBehaviour.AnimateTo == playableBehaviour.AnimateFrom)
                {
                    continue;
                }

                var inputWeight = playable.GetInputWeight(i);
                var tweenProgress = GetTweenProgress(playableInput, playableBehaviour);

                scaleTotalWeight += inputWeight;
                blendedScale += Vector3.Lerp(playableBehaviour.AnimateFrom, playableBehaviour.AnimateTo, tweenProgress) * inputWeight;

                Debug.Log("Blended Scale while animating:  " + blendedScale);
            }

            // TODO Final blend that we will apply when we are out of the clip
            // We need to keep the initial state and the final
            blendedScale += defaultScale * (1f - scaleTotalWeight);
            Debug.Log("Blended Scale when not animating:  " + blendedScale);

            trackBinding.localScale = blendedScale;
        }

        static float GetTweenProgress(ScriptPlayable<ScaleTweenPlayableBehaviour> playableInput, ScaleTweenPlayableBehaviour playableBehaviour)
        {
            var time = playableInput.GetTime();
            var normalisedTime = (float)(time * playableBehaviour.InverseDuration);

            if(playableBehaviour.AnimationType == ScaleTweenPlayableBehaviour.TweeningType.AnimationCurve && playableBehaviour.AnimationCurve != null)
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