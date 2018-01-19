using UnityEngine;
using UnityEngine.Playables;
using SocialPoint.Base;
using SocialPoint.Utils;
using System.Collections.Generic;

namespace SocialPoint.TimeLinePlayables
{
    public abstract class BaseAdvancedTransformTweenMixerBehaviour
    {
        public Vector3 DefaultInitialValue;
        public Vector3 DefaultFinalValue;
        public float TotalInputWeight;
        public Vector3 BlendValue;

        public virtual void InitializeValues(Transform baseTransform)
        {
            TotalInputWeight = 0f;
            BlendValue = Vector3.zero;

            DefaultInitialValue = baseTransform.localScale;
            DefaultFinalValue = baseTransform.localScale;
        }

        public abstract void SetFinalCalculatedValue(Transform baseTransform);

        public Vector3 GetFinalCalculatedValue()
        {
            return BlendValue + DefaultInitialValue * (1f - TotalInputWeight);
        }
    }

    public class PositionAdvancedTransformTweenMixerBehaviour : BaseAdvancedTransformTweenMixerBehaviour
    {
        public override void SetFinalCalculatedValue(Transform baseTransform)
        {
            baseTransform.position = GetFinalCalculatedValue(); 
        }
    }

    public class RotationAdvancedTransformTweenMixerBehaviour : BaseAdvancedTransformTweenMixerBehaviour
    {
        public override void SetFinalCalculatedValue(Transform baseTransform)
        {
            baseTransform.eulerAngles = GetFinalCalculatedValue(); 
        }
    }

    public class ScaleAdvancedTransformTweenMixerBehaviour : BaseAdvancedTransformTweenMixerBehaviour
    {
        public override void SetFinalCalculatedValue(Transform baseTransform)
        {
            baseTransform.localScale = GetFinalCalculatedValue(); 
        }
    }

    public class AdvancedTransformTweenMixerBehaviour : PlayableBehaviour
    {

        readonly public BaseAdvancedTransformTweenMixerBehaviour[] _mixerAnimations = 
        {
            new PositionAdvancedTransformTweenMixerBehaviour(),
            new RotationAdvancedTransformTweenMixerBehaviour(),
            new ScaleAdvancedTransformTweenMixerBehaviour()
        };
            
        bool _firstFrameHappened;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var trackBinding = playerData as Transform;
            if(trackBinding == null)
            {
                return;
            }
                
            SetupInitialValues(_mixerAnimations, trackBinding);

            var inputCount = playable.GetInputCount();
            for(int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<AdvancedTransformTweenBehaviour>)playable.GetInput(i);
                var playableBehaviour = playableInput.GetBehaviour();
                var inputWeight = playable.GetInputWeight(i);

                CalculateBlendValues(_mixerAnimations, playableInput, playableBehaviour, inputWeight);
            }

            SetupFinalValues(_mixerAnimations, trackBinding);
               

//            Debug.Log("blendedPosition: " + blendedPosition);
//            trackBinding.position = GetFinalValue(AdvancedTransformTweenBehaviour.kAnimatePosition);

//            Quaternion weightedDefaultRotation = ScaleQuaternion(defaultInitialRotation, 1f - rotationTotalInputWeight);
//            blendedRotation = AddQuaternions(blendedRotation, weightedDefaultRotation);
//            trackBinding.rotation = blendedRotation;

//            trackBinding.localScale = GetFinalValue(AdvancedTransformTweenBehaviour.kAnimateScale);
        }

        void SetupInitialValues(BaseAdvancedTransformTweenMixerBehaviour[] animMixerBehaviours, Transform baseTransform)
        {
            if(!_firstFrameHappened)
            {
                for(int i = 0; i < animMixerBehaviours.Length; ++i)
                {
                    var value = animMixerBehaviours[i];
                    if(value != null)
                    {
                        value.InitializeValues(baseTransform);
                    }
                }

                _firstFrameHappened = true;
            }
        }
            
        void SetupFinalValues(BaseAdvancedTransformTweenMixerBehaviour[] animMixerBehaviours, Transform baseTransform)
        {
            for(int i = 0; i < animMixerBehaviours.Length; ++i)
            {
                var value = animMixerBehaviours[i];
                if(value != null)
                {
                    value.SetFinalCalculatedValue(baseTransform);
                }
            }
        }

        void CalculateBlendValues(  BaseAdvancedTransformTweenMixerBehaviour[] animMixerBehaviours, 
                                    ScriptPlayable<AdvancedTransformTweenBehaviour> playableInput,
                                    AdvancedTransformTweenBehaviour playableBehaviour, 
                                    float inputWeight)
        {
            var animBehaviours = playableBehaviour.Animations;
            if(animBehaviours != null)
            {
                for(int i = 0; i < animBehaviours.Length; ++i)
                {
                    var anim = animBehaviours[i];
                    if(anim != null)
                    {
                        if(anim.Animate)
                        {
                            if(anim.HowToAnimate == BaseAdvancedTweenBehaviour.HowToAnimateType.UseReferencedTransforms)
                            {
                                if(anim.AnimateToReference == null)
                                {
                                    continue;
                                }

                                //                        playableBehaviour.AnimateTo = playableBehaviour.EndLocation.position;
                            }
                                
                            var valueMixer = animMixerBehaviours[i];
                            if(valueMixer != null)
                            {
                                valueMixer.TotalInputWeight += inputWeight;
                                valueMixer.BlendValue += AnimateValues(playableInput, playableBehaviour, inputWeight, anim);
                            }
                        }
                    }
                }
            }
        }
            
        [System.Diagnostics.Conditional(DebugFlags.DebugGUIControlFlag)]
        void DebugLog(string msg)
        {
            Log.i(string.Format("AdvancedTransformTweenMixerBehaviour msg {0}", msg));
        }

        static Vector3 AnimateValues(ScriptPlayable<AdvancedTransformTweenBehaviour> playableInput, AdvancedTransformTweenBehaviour playableBehaviour, float inputWeight, BaseAdvancedTweenBehaviour anim)
        {
            var tweenProgress = GetTweenProgress(playableInput, playableBehaviour, anim);
            return Vector3.Lerp(anim.AnimateFrom, anim.AnimateTo, tweenProgress) * inputWeight;
        }

//        static Quaternion AnimateRotation(ScriptPlayable<AdvancedTransformTweenBehaviour> playableInput, AdvancedTransformTweenBehaviour playableBehaviour, float inputWeight)
//        {
//            var anim = playableBehaviour.Animations[AdvancedTransformTweenBehaviour.kAnimateRotation];
//            if(anim != null)
//            {
////                Quaternion desiredRotation = Quaternion.Lerp(playableBehaviour.startingRotation, playableBehaviour.endLocation.rotation, tweenProgress);
////                desiredRotation = NormalizeQuaternion(desiredRotation);
////
////                if (Quaternion.Dot (blendedRotation, desiredRotation) < 0f)
////                {
////                    desiredRotation = ScaleQuaternion (desiredRotation, -1f);
////                }
////
////                desiredRotation = ScaleQuaternion(desiredRotation, inputWeight);
////
////                blendedRotation = AddQuaternions (blendedRotation, desiredRotation);
//
//                return Quaternion.identity;
//            }
//
//            return Quaternion.identity;
//        }
//
//        static Vector3 AnimateScale(ScriptPlayable<AdvancedTransformTweenBehaviour> playableInput, AdvancedTransformTweenBehaviour playableBehaviour, float inputWeight)
//        {
//            var anim = playableBehaviour.Animations[AdvancedTransformTweenBehaviour.kAnimateScale];
//            if(anim != null && trackBinding.localScale != anim.AnimateTo)
//            {
//                var tweenProgress = GetTweenProgress(playableInput, playableBehaviour, anim);
//                return Vector3.Lerp(anim.AnimateFrom, anim.AnimateTo, tweenProgress) * inputWeight;
//            }
//
//            return Vector3.zero;
//        }

        static float GetTweenProgress(ScriptPlayable<AdvancedTransformTweenBehaviour> playableInput, AdvancedTransformTweenBehaviour playableBehaviour, BaseAdvancedTweenBehaviour anim)
        {
            var time = playableInput.GetTime();
            var normalisedTime = (float)(time * playableBehaviour.InverseDuration);

            if(anim.AnimationType == BaseAdvancedTweenBehaviour.AnimateType.AnimationCurve && anim.AnimationCurve != null)
            {
                return anim.AnimationCurve.Evaluate(normalisedTime);
            }
            else
            {
                return anim.EaseType.ToFunction()(normalisedTime, 0f, 1f, 1f);
            }
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
                return ScaleQuaternion(rotation, 1f / magnitude);

            Debug.LogWarning("Cannot normalize a quaternion with zero magnitude.");
            return Quaternion.identity;
        }
    }
}