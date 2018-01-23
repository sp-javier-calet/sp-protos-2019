using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class ScaleTweenPlayableMixerBehaviour : BaseTweenPlayableMixerBehaviour
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

            var currentScale = trackBinding.localScale;
            var blendedScale = Vector3.zero;
            var scaleTotalWeight = 0f;

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
                    if(playableBehaviour.HowToAnimate == BaseTweenPlayableBehaviour.HowToAnimateType.UseAbsoluteValues)
                    {
                        if(playableBehaviour.UseCurrentFromValue)
                        {
                            playableBehaviour.AnimateFrom = currentScale;
                        }

                        if(playableBehaviour.UseCurrentToValue)
                        {
                            playableBehaviour.AnimateTo = currentScale;
                        }
                    }
                    _firstFrameHappened = true;
                }
 
                var inputWeight = playable.GetInputWeight(i);
                var tweenProgress = GetTweenProgress(playableInput, playableBehaviour);

                scaleTotalWeight += inputWeight;
                blendedScale += Vector3.Lerp(playableBehaviour.AnimateFrom, playableBehaviour.AnimateTo, tweenProgress) * inputWeight;
            }
                
            trackBinding.localScale = blendedScale + currentScale * (1f - scaleTotalWeight);
        }
    }
}