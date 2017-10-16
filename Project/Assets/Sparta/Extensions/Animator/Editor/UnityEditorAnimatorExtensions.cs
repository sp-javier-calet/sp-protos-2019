using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Animations
{
    public static class UnityEditorAnimatorExtensions
    {

        public static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
        {
            var childState = FindChildState(stateMachine, stateName);
            return childState.state;
        }

        public static ChildAnimatorState FindChildState(AnimatorStateMachine stateMachine, string stateName)
        {
            for(var i = 0; i < stateMachine.states.Length; i++)
            {
                var state = stateMachine.states[i];
                if(state.state.name == stateName)
                {
                    return state;
                }
            }
            return new ChildAnimatorState();
        }

        public static AnimatorData ToStandalone(this AnimatorController v)
        {
            var result = new AnimatorData {
                Name = v.name,
                Layers = UnityAnimatorExtensions.ConvertArray<LayerData, AnimatorControllerLayer>(v.layers, ToStandalone),
                Parameters = UnityAnimatorExtensions.ConvertArray<ParameterData, AnimatorControllerParameter>(v.parameters, UnityAnimatorExtensions.ToStandalone),
            };
            return result;
        }

        public static AnimatorController ToUnity(this AnimatorData v)
        {
            var result = new AnimatorController {
                name = v.Name,
                layers = UnityAnimatorExtensions.ConvertArray<AnimatorControllerLayer, LayerData>(v.Layers, ToUnity),
                parameters = UnityAnimatorExtensions.ConvertArray<AnimatorControllerParameter, ParameterData>(v.Parameters, ToUnity),
            };
            return result;
        }

        public static AnimatorControllerParameter ToUnity(this ParameterData v)
        {
            var result = new AnimatorControllerParameter {
                name = v.Name,
                type = v.Type.ToUnity(),
                defaultInt = v.DefaultInt,
                defaultFloat = v.DefaultFloat,
                defaultBool = v.DefaultBool,
            };
            return result;
        }

        static AnimatorControllerParameterType ToUnity(this ParameterDataType paramType)
        {
            switch(paramType)
            {
            case ParameterDataType.Int:
                return AnimatorControllerParameterType.Int;
            case ParameterDataType.Float:
                return AnimatorControllerParameterType.Float;
            case ParameterDataType.Bool:
                return AnimatorControllerParameterType.Bool;
            case ParameterDataType.Trigger:
                return AnimatorControllerParameterType.Trigger;
            default:
                throw new InvalidOperationException("Cannot convert paramater type: " + paramType + " to an Unity type");
            }
        }


        public static LayerData ToStandalone(this AnimatorControllerLayer v)
        {
            var result = new LayerData {
                StateMachine = v.stateMachine.ToStandalone()
            };
            return result;
        }

        public static AnimatorControllerLayer ToUnity(this LayerData v)
        {
            var result = new AnimatorControllerLayer {
                stateMachine = v.StateMachine.ToUnity(),
            };
            return result;
        }

        public static StateData ToStandalone(this ChildAnimatorState v)
        {
            var state = v.state;
            var result = new StateData {
                NameHash = state.nameHash,
                Name = state.name,
                Speed = state.speed,
                SpeedParameter = state.speedParameter,
                SpeedParameterActive = state.speedParameterActive,
                Motion = state.motion.ToStandalone(),
                Transitions = UnityAnimatorExtensions.ConvertArray<TransitionData, AnimatorStateTransition>(state.transitions, ToStandalone),
            };
            return result;
        }

        public static ChildAnimatorState ToUnity(StateData v)
        {
            var state = new AnimatorState {
                name = v.Name,
                speed = v.Speed,
                speedParameter = v.SpeedParameter,
                speedParameterActive = v.SpeedParameterActive,
                motion = v.Motion.ToUnity(),
                transitions = UnityAnimatorExtensions.ConvertArray<AnimatorStateTransition, TransitionData>(v.Transitions, ToUnity),
            };
            var result = new ChildAnimatorState {
                state = state,
            };
            return result;
        }

        public static MotionData ToStandalone(this Motion v)
        {
            var motionData = new MotionData();

            //NOTE: A Motion can be an AnimationClip, but this is not documented! (Can also be a BlendTree)
            //TODO: Check other possible types for Motion

            var clip = v as AnimationClip;
            if(clip != null)
            {
                motionData.Type = MotionDataType.Clip;
                motionData.Animation = clip.ToStandalone();
            }

            var blendTree = v as BlendTree;
            if(blendTree != null)
            {
                motionData.Type = MotionDataType.BlendTree;
                //TODO
            }

            return motionData;
        }

        public static Motion ToUnity(this MotionData v)
        {
            Motion result = null;
            switch(v.Type)
            {
            case MotionDataType.Clip:
                //TODO: cast from animation clip?
                break;
            default:
                break;
            }
            return result;
        }

        public static ConditionData ToStandalone(this AnimatorCondition v)
        {
            var result = new ConditionData {
                Paramater = v.parameter,
                Type = v.mode.ToStandalone(),
                Threshold = v.threshold,
            };
            return result;
        }

        public static AnimatorCondition ToUnity(this ConditionData v)
        {
            var result = new AnimatorCondition {
                parameter = v.Paramater,
                mode = v.Type.ToUnity(),
                threshold = v.Threshold,
            };
            return result;
        }

        static ConditionDataType ToStandalone(this AnimatorConditionMode conditionType)
        {
            switch(conditionType)
            {
            case AnimatorConditionMode.If:
                return ConditionDataType.If;
            case AnimatorConditionMode.IfNot:
                return ConditionDataType.IfNot;
            case AnimatorConditionMode.Greater:
                return ConditionDataType.Greater;
            case AnimatorConditionMode.Less:
                return ConditionDataType.Less;
            case AnimatorConditionMode.Equals:
                return ConditionDataType.Equals;
            case AnimatorConditionMode.NotEqual:
                return ConditionDataType.NotEqual;
            default:
                throw new Exception("Animator condition has an unsupported mode: " + conditionType);
            }
        }

        public static AnimatorConditionMode ToUnity(this ConditionDataType conditionType)
        {
            switch(conditionType)
            {
            case ConditionDataType.If:
                return AnimatorConditionMode.If;
            case ConditionDataType.IfNot:
                return AnimatorConditionMode.IfNot;
            case ConditionDataType.Greater:
                return AnimatorConditionMode.Greater;
            case ConditionDataType.Less:
                return AnimatorConditionMode.Less;
            case ConditionDataType.Equals:
                return AnimatorConditionMode.Equals;
            case ConditionDataType.NotEqual:
                return AnimatorConditionMode.NotEqual;
            default:
                throw new Exception("Cannot convert condition type: " + conditionType + " to an Unity type");
            }
        }

        public static StateMachineData ToStandalone(this AnimatorStateMachine v)
        {
            var result = new StateMachineData {
                Name = v.name,
                DefaultState = v.defaultState.name,
                States = UnityAnimatorExtensions.ConvertArray<StateData, ChildAnimatorState>(v.states, ToStandalone),
                AnyStateTransitions = UnityAnimatorExtensions.ConvertArray<TransitionData, AnimatorStateTransition>(v.anyStateTransitions, ToStandalone),
            };
            return result;
        }

        public static AnimatorStateMachine ToUnity(this StateMachineData v)
        {
            var result = new AnimatorStateMachine {
                name = v.Name,
                states = UnityAnimatorExtensions.ConvertArray<ChildAnimatorState, StateData>(v.States, ToUnity),
                anyStateTransitions = UnityAnimatorExtensions.ConvertArray<AnimatorStateTransition, TransitionData>(v.AnyStateTransitions, ToUnity),
            };

            for(var i = 0; i < result.states.Length; i++)
            {
                var state = result.states[i];
                //Set as default if it is the one
                if(state.state.name == v.DefaultState)
                {
                    result.defaultState = state.state;
                }

                //Add transitions
                for(var j = 0; j < state.state.transitions.Length; i++)
                {
                    var transition = state.state.transitions[j];
                    transition.destinationState = FindState(result, transition.name);
                }
            }

            //Add any state transitions
            for(var i = 0; i < result.anyStateTransitions.Length; i++)
            {
                var transition = result.anyStateTransitions[i];
                transition.destinationState = FindState(result, transition.name);
            }

            return result;
        }

        public static TransitionData ToStandalone(this AnimatorStateTransition v)
        {
            var result = new TransitionData {
                ToState = v.destinationState.name,
                Duration = v.duration,
                ExitTime = v.exitTime,
                HasFixedDuration = v.hasFixedDuration,
                HasExitTime = v.hasExitTime,
                InterruptionSource = v.interruptionSource.ToStandalone(),
                OrderedInterruption = v.orderedInterruption,
                Conditions = UnityAnimatorExtensions.ConvertArray<ConditionData, AnimatorCondition>(v.conditions, ToStandalone),
            };
            return result;
        }

        public static InterruptionSourceType ToStandalone(this TransitionInterruptionSource unitySource)
        {
            switch(unitySource)
            {
            case TransitionInterruptionSource.Source:
                return InterruptionSourceType.Source;
            case TransitionInterruptionSource.Destination:
                return InterruptionSourceType.Destination;
            case TransitionInterruptionSource.SourceThenDestination:
                return InterruptionSourceType.SourceThenDestination;
            case TransitionInterruptionSource.DestinationThenSource:
                return InterruptionSourceType.DestinationThenSource;
            default:
                return InterruptionSourceType.None;
            }
        }


        public static TransitionInterruptionSource ToUnity(this InterruptionSourceType unitySource)
        {
            switch(unitySource)
            {
            case InterruptionSourceType.Source:
                return TransitionInterruptionSource.Source;
            case InterruptionSourceType.Destination:
                return TransitionInterruptionSource.Destination;
            case InterruptionSourceType.SourceThenDestination:
                return TransitionInterruptionSource.SourceThenDestination;
            case InterruptionSourceType.DestinationThenSource:
                return TransitionInterruptionSource.DestinationThenSource;
            default:
                return TransitionInterruptionSource.None;
            }
        }

        public static AnimatorStateTransition ToUnity(this TransitionData v)
        {
            var result = new AnimatorStateTransition {
                name = v.ToState,//Use later to set real target state object (destinationState)
                duration = v.Duration,
                exitTime = v.ExitTime,
                hasFixedDuration = v.HasFixedDuration,
                hasExitTime = v.HasExitTime,
                conditions = UnityAnimatorExtensions.ConvertArray<AnimatorCondition, ConditionData>(v.Conditions, ToUnity)
            };
            return result;
        }

    }
}
