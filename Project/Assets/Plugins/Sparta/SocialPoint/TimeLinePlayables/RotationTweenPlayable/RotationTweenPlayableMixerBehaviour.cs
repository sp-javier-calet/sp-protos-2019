using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    public class RotationTweenPlayableMixerBehaviour : BaseTweenPlayableMixerBehaviour
    {
        // NOTE: This function is called at runtime and edit time.  Keep that in mind when setting the values of properties.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            Transform trackBinding = playerData as Transform;

            if(trackBinding == null)
            {
                return;
            }

            var defaultRotation = trackBinding.rotation.eulerAngles;

            // Get number of tracks for the clip
            var inputCount = playable.GetInputCount();
            var rotationTotalWeight = 0f;
            var blendedRotation = Vector3.zero;

            for (int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<BaseTweenPlayableBehaviour>)playable.GetInput(i);
                var playableBehaviour = (RotationTweenPlayableBehaviour)playableInput.GetBehaviour();

                if(trackBinding.rotation.eulerAngles == playableBehaviour.AnimateTo)
                {
                    continue;
                }

//                if(playableBehaviour.AnimPositionType == BaseTweenPlayableBehaviour.HowToAnimateType.UseReferencedTransforms)
//                {
//                    if(playableBehaviour.TransformTo == null)
//                    {
//                        continue;
//                    }
//
//                    playableBehaviour.AnimateTo = playableBehaviour.TransformTo.eulerAngles;
//                }

                var inputWeight = playable.GetInputWeight(i);

                if(!_firstFrameHappened && playableBehaviour.AnimPositionType == BaseTweenPlayableBehaviour.HowToAnimateType.UseReferencedTransforms && playableBehaviour.TransformFrom == null)
                {
                    playableBehaviour.AnimateFrom = defaultRotation;
                    _firstFrameHappened = true;
                }

                var tweenProgress = GetTweenProgress(playableInput, playableBehaviour);

//                Quaternion desiredRotation = Quaternion.Lerp(playableBehaviour.AnimateFrom, playableBehaviour.AnimateTo, tweenProgress);
//                desiredRotation = NormalizeQuaternion(desiredRotation);
//
//                if(Quaternion.Dot(blendedRotation, desiredRotation) < 0f)
//                {
//                    desiredRotation = ScaleQuaternion(desiredRotation, -1f);
//                }
//
//                desiredRotation = ScaleQuaternion(desiredRotation, inputWeight);
//                blendedRotation = AddQuaternions(blendedRotation, desiredRotation);

                rotationTotalWeight += inputWeight;
                blendedRotation += Vector3.Lerp(playableBehaviour.AnimateFrom, playableBehaviour.AnimateTo, tweenProgress) * inputWeight;
            }
                
            trackBinding.eulerAngles = blendedRotation + defaultRotation * (1f - rotationTotalWeight);
//            trackBinding.rotation = blendedRotation;
        }

//        static Quaternion AddQuaternions(Quaternion first, Quaternion second)
//        {
//            first.w += second.w;
//            first.x += second.x;
//            first.y += second.y;
//            first.z += second.z;
//
//            return first;
//        }
//
//        static Quaternion ScaleQuaternion(Quaternion rotation, float multiplier)
//        {
//            rotation.w *= multiplier;
//            rotation.x *= multiplier;
//            rotation.y *= multiplier;
//            rotation.z *= multiplier;
//
//            return rotation;
//        }
//
//        static float QuaternionMagnitude(Quaternion rotation)
//        {
//            return Mathf.Sqrt((Quaternion.Dot(rotation, rotation)));
//        }
//
//        static Quaternion NormalizeQuaternion(Quaternion rotation)
//        {
//            float magnitude = QuaternionMagnitude(rotation);
//
//            if(magnitude > 0f)
//            {
//                return ScaleQuaternion(rotation, 1f / magnitude);
//            }
//
//            Debug.LogWarning("Cannot normalize a quaternion with zero magnitude.");
//            return Quaternion.identity;
//        }
    }
}