using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class RotationTweenPlayableMixerBehaviour : BaseTweenPlayableMixerBehaviour
    {
        public Transform _binding;
        public Vector3 _defaultValue;

        public override void OnGraphStop(Playable playable)
        {
            if(_binding != null)
            {
                _binding.eulerAngles = _defaultValue;
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
                _defaultValue = _binding.eulerAngles;
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

            for(int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<BaseTweenPlayableBehaviour>)playable.GetInput(i);
                var playableBehaviour = (RotationTweenPlayableBehaviour)playableInput.GetBehaviour();
                var inputWeight = playable.GetInputWeight(i);
                var tweenProgress = GetTweenProgress(playableInput, playableBehaviour);

                playableBehaviour.SetAnimatedValues(_defaultValue);

                rotationTotalWeight += inputWeight;
                blendedRotation += Vector3.Lerp(playableBehaviour.AnimateFrom, playableBehaviour.AnimateTo, tweenProgress) * inputWeight;
            }

            trackBinding.eulerAngles = blendedRotation + currentRotation * (1f - rotationTotalWeight);
        }
    }
}