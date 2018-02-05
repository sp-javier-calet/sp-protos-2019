using SocialPoint.Base;
using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class RotationPlayableMixer : BaseTransformPlayableMixer
    {
        public Transform _trackBinding;
        public Quaternion _defaultValue;

        public override void OnGraphStop(Playable playable)
        {
            if(_trackBinding != null)
            {
                _trackBinding.rotation = _defaultValue;
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
                _defaultValue = _trackBinding.rotation;
                _firstFrameHappened = true;
            }

            // Get number of clips for the current track
            var inputCount = playable.GetInputCount();
            if(inputCount == 0)
            {
                return;
            }

            // Track the current value to store values between clips and avoid reseting values to the default value
            var currentRotation = _trackBinding.rotation;
            var blendedRotation = Quaternion.identity;
            var rotationTotalWeight = 0f;

            for(int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<BaseTransformPlayableData>)playable.GetInput(i);
                var playableBehaviour = (RotationPlayableData)playableInput.GetBehaviour();
                var inputWeight = playable.GetInputWeight(i);
                var tweenProgress = GetTweenProgress(playableInput, playableBehaviour);

                playableBehaviour.SetAnimatedValues(_defaultValue);

                rotationTotalWeight += inputWeight;
                var desiredRotation = Quaternion.Lerp(playableBehaviour.AnimateFrom, playableBehaviour.AnimateTo, tweenProgress);
                desiredRotation = NormalizeQuaternion(desiredRotation);

                if(Quaternion.Dot(blendedRotation, desiredRotation) < 0f)
                {
                    desiredRotation = ScaleQuaternion(desiredRotation, -1f);
                }

                desiredRotation = ScaleQuaternion(desiredRotation, inputWeight);
                blendedRotation = AddQuaternions(blendedRotation, desiredRotation);
            }

            var weightedDefaultRotation = ScaleQuaternion(_defaultValue, 1f - rotationTotalWeight);
            _trackBinding.rotation = AddQuaternions(blendedRotation, weightedDefaultRotation);
        }

        static Quaternion AddQuaternions(Quaternion first, Quaternion second)
        {
            first.w += second.w;
            first.x += second.x;
            first.y += second.y;
            first.z += second.z;

            return first;
        }

        static Quaternion ScaleQuaternion(Quaternion rotation, float multiplier)
        {
            rotation.w *= multiplier;
            rotation.x *= multiplier;
            rotation.y *= multiplier;
            rotation.z *= multiplier;

            return rotation;
        }

        static float QuaternionMagnitude(Quaternion rotation)
        {
            return Mathf.Sqrt((Quaternion.Dot(rotation, rotation)));
        }

        static Quaternion NormalizeQuaternion(Quaternion rotation)
        {
            float magnitude = QuaternionMagnitude(rotation);

            if(magnitude > 0f)
            {
                return ScaleQuaternion(rotation, 1f / magnitude);
            }

            Log.w("Cannot normalize a quaternion with zero magnitude.");
            return Quaternion.identity;
        }
    }
}