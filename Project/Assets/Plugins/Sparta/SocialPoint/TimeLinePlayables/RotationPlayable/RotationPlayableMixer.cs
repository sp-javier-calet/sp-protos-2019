using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class RotationPlayableMixer : BaseTransformPlayableMixer
    {
        public Transform _trackBinding;
        public Vector3 _defaultValue;

        public override void OnGraphStop(Playable playable)
        {
            if(_trackBinding != null)
            {
                _trackBinding.eulerAngles = _defaultValue;
            }
        }

        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            _trackBinding = playerData as Transform;
            if(_trackBinding == null)
            {
                return;
            }

            if(!_firstFrameHappened)
            {
                _defaultValue = _trackBinding.eulerAngles;

                _firstFrameHappened = true;
            }

            // Get number of clips for the current track
            var inputCount = playable.GetInputCount();
            if(inputCount == 0)
            {
                return;
            }

            // Track the current value to store values between clips and avoid reseting values to the default value
            var currentRotation = _trackBinding.eulerAngles;
            var blendedRotation = Vector3.zero;
            var rotationTotalWeight = 0f;

            for(int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<BaseTransformPlayableData>)playable.GetInput(i);
                var playableBehaviour = (RotationPlayableData)playableInput.GetBehaviour();
                var inputWeight = playable.GetInputWeight(i);
                var tweenProgress = GetTweenProgress(playableInput, playableBehaviour);

                playableBehaviour.SetAnimatedValues(_defaultValue);

                rotationTotalWeight += inputWeight;
                blendedRotation += Vector3.Lerp(playableBehaviour.AnimateFrom, playableBehaviour.AnimateTo, tweenProgress) * inputWeight;
            }

            _trackBinding.eulerAngles = blendedRotation + currentRotation * (1f - rotationTotalWeight);
        }
    }
}