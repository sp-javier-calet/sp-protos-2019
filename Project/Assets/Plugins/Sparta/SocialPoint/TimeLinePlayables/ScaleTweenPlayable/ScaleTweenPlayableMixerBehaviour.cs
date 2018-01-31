using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class ScaleTweenPlayableMixerBehaviour : BaseTweenPlayableMixerBehaviour
    {
        public Transform _binding;
        public Vector3 _defaultValue;

        public override void OnGraphStop(Playable playable)
        {
            if(_binding != null)
            {
                _binding.localScale = _defaultValue;
            }
        }

        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var trackBinding = playerData as Transform;
            if(trackBinding == null)
            {
                return;
            }

            if(_binding == null)
            {
                _binding = trackBinding;
                _defaultValue = _binding.localScale;
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

            for(int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<BaseTweenPlayableBehaviour>)playable.GetInput(i);
                var playableBehaviour = (ScaleTweenPlayableBehaviour)playableInput.GetBehaviour();
                var inputWeight = playable.GetInputWeight(i);
                var tweenProgress = GetTweenProgress(playableInput, playableBehaviour);

                playableBehaviour.SetAnimatedValues(_defaultValue);

                scaleTotalWeight += inputWeight;
                blendedScale += Vector3.Lerp(playableBehaviour.AnimateFrom, playableBehaviour.AnimateTo, tweenProgress) * inputWeight;
            }
                
            trackBinding.localScale = blendedScale + currentScale * (1f - scaleTotalWeight);
        }
    }
}