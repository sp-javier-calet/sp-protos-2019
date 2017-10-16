using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Animations
{
    public static class UnityAnimatorExtensions
    {
        public static ParameterData ToStandalone(this AnimatorControllerParameter v)
        {
            var result = new ParameterData {
                Name = v.name,
                Type = v.type.ToStandalone(),
                DefaultBool = v.defaultBool,
                DefaultInt = v.defaultInt,
                DefaultFloat = v.defaultFloat,
            };
            return result;
        }

        static ParameterDataType ToStandalone(this AnimatorControllerParameterType paramType)
        {
            switch(paramType)
            {
            case AnimatorControllerParameterType.Int:
                return ParameterDataType.Int;
            case AnimatorControllerParameterType.Float:
                return ParameterDataType.Float;
            case AnimatorControllerParameterType.Bool:
                return ParameterDataType.Bool;
            case AnimatorControllerParameterType.Trigger:
                return ParameterDataType.Trigger;
            default:
                throw new InvalidOperationException("Animator parameter has an unsupported type: " + paramType);
            }
        }

        public static AnimationData ToStandalone(this AnimationClip v)
        {
            var result = new AnimationData {
                Name = v.name,
                Length = v.length,
                Loop = v.isLooping,
                Events = ConvertArray<AnimationEventData, AnimationEvent>(FilterEvents(v.events), ToStandalone),
            };
            return result;
        }

        public static AnimationClip ToUnity(this AnimationData v)
        {
            var result = new AnimationClip {
                //TODO: Must find asset with name? create a new one to compare events?
            };
            return result;
        }

        public static AnimationEventData ToStandalone(this AnimationEvent v)
        {
            var result = new AnimationEventData {
                StringValue = v.stringParameter,
                IntValue = v.intParameter,
                FloatValue = v.floatParameter,
                Time = v.time,
                IsVisual = v.functionName == AnimationVisualEventFunctionName,
            };
            return result;
        }

        public static AnimationEvent ToUnity(this AnimationEventData v)
        {
            var result = new AnimationEvent {
                stringParameter = v.StringValue,
                intParameter = v.IntValue,
                floatParameter = v.FloatValue,
                time = v.Time,
            };
            return result;
        }

        const string AnimationEventFunctionName = "OnAnimationEvent";
        const string AnimationVisualEventFunctionName = "OnAnimationVisualEvent";

        public static AnimationEvent[] FilterEvents(this AnimationEvent[] events)
        {
            var filtered = new List<AnimationEvent>();
            for(int i = 0; i < events.Length; i++)
            {
                if(events[i].functionName == AnimationEventFunctionName || events[i].functionName == AnimationVisualEventFunctionName)
                {
                    filtered.Add(events[i]);
                }
            }
            return filtered.ToArray();
        }

        public static TNew[] ConvertArray<TNew, TOld>(TOld[] unityData, Func<TOld, TNew> converter)
        {
            TNew[] data = new TNew[unityData.Length];
            for(int i = 0; i < unityData.Length; i++)
            {
                data[i] = converter(unityData[i]);
            }
            return data;
        }
    }
}
