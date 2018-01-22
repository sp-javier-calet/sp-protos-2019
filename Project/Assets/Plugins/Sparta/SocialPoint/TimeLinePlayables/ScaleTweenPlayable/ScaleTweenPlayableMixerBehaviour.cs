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

            var defaultScale = trackBinding.localScale;
                
            // Get number of tracks for the clip
            var inputCount = playable.GetInputCount();
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

                if(!_firstFrameHappened && playableBehaviour.AnimPositionType == BaseTweenPlayableBehaviour.HowToAnimateType.UseReferencedTransforms && playableBehaviour.TransformFrom == null)
                {
                    playableBehaviour.AnimateFrom = defaultScale;
                    _firstFrameHappened = true;
                }

                var inputWeight = playable.GetInputWeight(i);
                DebugLog("** input weight for input: " + i + " - " + inputWeight.ToString());

                var tweenProgress = GetTweenProgress(playableInput, playableBehaviour);

                scaleTotalWeight += inputWeight;
                blendedScale += Vector3.Lerp(playableBehaviour.AnimateFrom, playableBehaviour.AnimateTo, tweenProgress) * inputWeight;
            }
                
            trackBinding.localScale = blendedScale + defaultScale * (1f - scaleTotalWeight);
        }
    }
}