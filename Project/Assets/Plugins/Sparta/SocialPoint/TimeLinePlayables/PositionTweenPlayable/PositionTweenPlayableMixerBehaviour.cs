using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class PositionTweenPlayableMixerBehaviour : BaseTweenPlayableMixerBehaviour
    {
        public Transform _binding;
        public Vector3 _defaultValue;

        public override void OnGraphStop(Playable playable)
        {
            if(_binding != null)
            {
                _binding.position = _defaultValue;
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
                _defaultValue = _binding.position;
            }

            // Get number of clips for the current track
            var inputCount = playable.GetInputCount();
            if(inputCount == 0)
            {
                return;
            }

            var currentPosition = trackBinding.position;
            var blendedPosition = Vector3.zero;
            var positionTotalWeight = 0f;

            for(int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<BaseTweenPlayableBehaviour>)playable.GetInput(i);
                var playableBehaviour = (PositionTweenPlayableBehaviour)playableInput.GetBehaviour();
                var inputWeight = playable.GetInputWeight(i);
                var tweenProgress = GetTweenProgress(playableInput, playableBehaviour);

                playableBehaviour.SetAnimatedValues(_defaultValue);

                positionTotalWeight += inputWeight;
                blendedPosition += Vector3.Lerp(playableBehaviour.AnimateFrom, playableBehaviour.AnimateTo, tweenProgress) * inputWeight;
            }

            trackBinding.position = blendedPosition + currentPosition * (1f - positionTotalWeight);
        }
    }
}