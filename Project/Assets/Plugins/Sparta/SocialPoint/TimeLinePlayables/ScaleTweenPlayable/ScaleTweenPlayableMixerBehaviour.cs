using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class ScaleTweenPlayableMixerBehaviour : BaseTweenPlayableMixerBehaviour
    {
        Vector3 _defaultScale = Vector3.zero;

        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var trackBinding = playerData as Transform;

            if(trackBinding == null)
            {
                return;
            }

            _defaultScale = trackBinding.localScale;

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

                var inputWeight = playable.GetInputWeight(i);
                DebugLog("** input weight for input: " + i + " - " + inputWeight.ToString());

                if(!_firstFrameHappened)
                {
                    _defaultScale = trackBinding.localScale;
                    _firstFrameHappened = true;
                }

                var tweenProgress = GetTweenProgress(playableInput, playableBehaviour);

                scaleTotalWeight += inputWeight;
                blendedScale += Vector3.Lerp(playableBehaviour.AnimateFrom, playableBehaviour.AnimateTo, tweenProgress) * inputWeight;
            }
                
            trackBinding.localScale = blendedScale + _defaultScale * (1f - scaleTotalWeight);
        }
    }
}