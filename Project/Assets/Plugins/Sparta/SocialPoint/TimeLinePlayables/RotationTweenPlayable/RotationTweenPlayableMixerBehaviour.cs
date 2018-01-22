using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class RotationTweenPlayableMixerBehaviour : BaseTweenPlayableMixerBehaviour
    {
        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var trackBinding = playerData as Transform;
            if(trackBinding == null)
            {
                return;
            }

            // Get number of clips for the current track
            var inputCount = playable.GetInputCount();
            if(inputCount == 0)
            {
                return;
            }

            var currentRotation = trackBinding.eulerAngles;
            var blendedRotation = Vector3.zero;
            var rotationTotalWeight = 0f;

            DebugLog("Input count: " + inputCount);
            for(int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<BaseTweenPlayableBehaviour>)playable.GetInput(i);
                var playableBehaviour = (ScaleTweenPlayableBehaviour)playableInput.GetBehaviour();

                if(trackBinding.localScale == playableBehaviour.AnimateTo)
                {
                    continue;
                }

                if(!_firstFrameHappened)
                {
                    // We need to setup this in the first processed frame for each of the clips, because from the BaseTweenPlayableBehaviour 
                    // we have no access to the current selected Transform to animate...
                    if(playableBehaviour.AnimPositionType == BaseTweenPlayableBehaviour.HowToAnimateType.UseAbsoluteValues)
                    {
                        if(playableBehaviour.UseCurrentFromValue)
                        {
                            playableBehaviour.AnimateFrom = currentRotation;
                        }

                        if(playableBehaviour.UseCurrentToValue)
                        {
                            playableBehaviour.AnimateTo = currentRotation;
                        }
                    }
                    _firstFrameHappened = true;
                }

                var inputWeight = playable.GetInputWeight(i);
                var tweenProgress = GetTweenProgress(playableInput, playableBehaviour);

                rotationTotalWeight += inputWeight;
                blendedRotation += Vector3.Lerp(playableBehaviour.AnimateFrom, playableBehaviour.AnimateTo, tweenProgress) * inputWeight;
            }

            trackBinding.eulerAngles = blendedRotation + currentRotation * (1f - rotationTotalWeight);
        }
    }
}