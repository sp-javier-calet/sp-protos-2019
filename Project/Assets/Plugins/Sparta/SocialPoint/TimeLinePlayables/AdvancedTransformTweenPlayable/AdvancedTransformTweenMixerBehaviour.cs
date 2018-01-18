using UnityEngine;
using UnityEngine.Playables;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.TimeLinePlayables
{
    public class AdvancedTransformTweenMixerBehaviour : PlayableBehaviour
    {
        bool _firstFrameHappened;

        Vector3[] _defaultInitialValues = new Vector3[AdvancedTransformTweenBehaviour.kAnimateTotal];

        //            Vector3[] defaultFinalValues = 
        //            {
        //                trackBinding.position,
        //                trackBinding.rotation.eulerAngles,
        //                trackBinding.localScale
        //            };

        Vector3[] _blendValues = new Vector3[AdvancedTransformTweenBehaviour.kAnimateTotal];
        float[] _totalInputWeights = new float[AdvancedTransformTweenBehaviour.kAnimateTotal];

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var trackBinding = playerData as Transform;
            if(trackBinding == null)
            {
                return;
            }

            _defaultInitialValues[AdvancedTransformTweenBehaviour.kAnimatePosition] = trackBinding.position;
            _defaultInitialValues[AdvancedTransformTweenBehaviour.kAnimateRotation] = trackBinding.rotation.eulerAngles;
            _defaultInitialValues[AdvancedTransformTweenBehaviour.kAnimateScale] = trackBinding.localScale;

            _blendValues[AdvancedTransformTweenBehaviour.kAnimatePosition] = Vector3.zero;
            _blendValues[AdvancedTransformTweenBehaviour.kAnimateRotation] = Vector3.zero;
            _blendValues[AdvancedTransformTweenBehaviour.kAnimateScale] = Vector3.zero;

            _totalInputWeights[AdvancedTransformTweenBehaviour.kAnimatePosition] = 0f;
            _totalInputWeights[AdvancedTransformTweenBehaviour.kAnimateRotation] = 0f;
            _totalInputWeights[AdvancedTransformTweenBehaviour.kAnimateScale] = 0f;

            var inputCount = playable.GetInputCount();

            for(int i = 0; i < inputCount; i++)
            {
                var playableInput = (ScriptPlayable<AdvancedTransformTweenBehaviour>)playable.GetInput(i);
                var playableBehaviour = playableInput.GetBehaviour();

                var inputWeight = playable.GetInputWeight(i);

                var animBehaviours = new AdvancedTweenBehaviour[AdvancedTransformTweenBehaviour.kAnimateTotal];
                var animBehavioursCount = animBehaviours.Length;

                if(playableBehaviour.AnimatePosition)
                {
                    animBehaviours[AdvancedTransformTweenBehaviour.kAnimatePosition] = playableBehaviour.Animations[AdvancedTransformTweenBehaviour.kAnimatePosition];
                }

                if(playableBehaviour.AnimateRotation)
                {
                    animBehaviours[AdvancedTransformTweenBehaviour.kAnimateRotation] = playableBehaviour.Animations[AdvancedTransformTweenBehaviour.kAnimateRotation];
                }

                if(playableBehaviour.AnimateScale)
                {
                    animBehaviours[AdvancedTransformTweenBehaviour.kAnimateScale] = playableBehaviour.Animations[AdvancedTransformTweenBehaviour.kAnimateScale];
                }

                // We need to refresh every time the AnimateTO value because we ban have referenced objects that are currently animated and can change it's position, scale,...
                // TODO add a check if we want to refresh this every time or not
                if(!_firstFrameHappened)// && !playableBehaviour.StartLocation)
                {
                    for(int j = 0; j < animBehavioursCount; ++j)
                    {
                        if(animBehaviours[j] != null)
                        {
                            animBehaviours[j].AnimateFrom = _defaultInitialValues[j];
                        }
                    }
                        
                    _firstFrameHappened = true;
                }
                    
                for(int j = 0; j < animBehavioursCount; ++j)
                {
                    if(animBehaviours[j] != null)
                    {
                        if(animBehaviours[j].HowToAnimate == AdvancedTweenBehaviour.HowToAnimateType.UseReferencedTransforms)
                        {
                            if(animBehaviours[j].AnimateToReference == null)
                            {
                                continue;
                            }

                            //                        playableBehaviour.AnimateTo = playableBehaviour.EndLocation.position;
                        }

                        _totalInputWeights[j] += inputWeight;
                        _blendValues[j] += AnimateValues(playableInput, playableBehaviour, inputWeight, animBehaviours[j]);
                    }
                }
            }
               
//            Debug.Log("blendedPosition: " + blendedPosition);
            trackBinding.position = GetFinalValue(AdvancedTransformTweenBehaviour.kAnimatePosition);

//            Quaternion weightedDefaultRotation = ScaleQuaternion(defaultInitialRotation, 1f - rotationTotalInputWeight);
//            blendedRotation = AddQuaternions(blendedRotation, weightedDefaultRotation);
//            trackBinding.rotation = blendedRotation;

            trackBinding.localScale = GetFinalValue(AdvancedTransformTweenBehaviour.kAnimateScale);
        }

        Vector3 GetFinalValue(int index)
        {
            return _blendValues[index] + _defaultInitialValues[index] * (1f - _totalInputWeights[index]); 
        }

        [System.Diagnostics.Conditional(DebugFlags.DebugGUIControlFlag)]
        void DebugLog(string msg)
        {
            Log.i(string.Format("AdvancedTransformTweenMixerBehaviour msg {0}", msg));
        }

        static Vector3 AnimateValues(ScriptPlayable<AdvancedTransformTweenBehaviour> playableInput, AdvancedTransformTweenBehaviour playableBehaviour, float inputWeight, AdvancedTweenBehaviour anim)
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

        static float GetTweenProgress(ScriptPlayable<AdvancedTransformTweenBehaviour> playableInput, AdvancedTransformTweenBehaviour playableBehaviour, AdvancedTweenBehaviour anim)
        {
            var time = playableInput.GetTime();
            var normalisedTime = (float)(time * playableBehaviour.InverseDuration);

            if(anim.AnimationType == AdvancedTweenBehaviour.AnimateType.AnimationCurve && anim.AnimationCurve != null)
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