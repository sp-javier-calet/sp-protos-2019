using System.Collections;
using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Animations
{
    public class TransitionStandalone : ITimeTransition
    {
        IAnimatorParameterProvider _parameterProvider;
        TransitionData _data;
        float _time;

        public TransitionStandalone(TransitionData data, IAnimatorParameterProvider parameterProvider)
        {
            _parameterProvider = parameterProvider;
            _data = data;
        }

        public bool StopInterruptionOnCurrent
        {
            get
            {
                return _data.OrderedInterruption;
            }
        }

        public void UpdateInterruptions(List<TimeTransitionPair> transitions, TimeTransitionInterruptionSources sources)
        {
            transitions.AddRange(sources.Any);
            switch(_data.InterruptionSource)
            {
            case InterruptionSourceType.None:
                break;
            case InterruptionSourceType.Source:
                transitions.AddRange(sources.Source);
                break;
            case InterruptionSourceType.Destination:
                transitions.AddRange(sources.Destination);
                break;
            case InterruptionSourceType.SourceThenDestination:
                transitions.AddRange(sources.Source);
                transitions.AddRange(sources.Destination);
                break;
            case InterruptionSourceType.DestinationThenSource:
                transitions.AddRange(sources.Destination);
                transitions.AddRange(sources.Source);
                break;
            }
        }

        public bool ShouldStart(ITimeState currentState)
        {
            if(!MeetsAllConditions)
            {
                return false;
            }
            if(_data.HasExitTime && currentState.NormalizedTime < _data.ExitTime)
            {
                return false;
            }
            return true;
        }

        public void OnStart(ITimeState currentState, ITimeState nextState)
        {
            ResetTriggerParams();
            
            if(_data.HasExitTime)
            {
                var diff = currentState.NormalizedTime - _data.ExitTime;
                _time = diff * currentState.Duration;
            }
            else
            {
                _time = 0.0f;
            }
            nextState.Update(_time);
// Comments are kept here on purpose to reenebale when testing            UnityEngine.Debug.Log("[animator] stand trans start " + (_time/(currentState.Duration*_data.Duration)));
        }

        public void Update(float dt, ITimeState currentState, ITimeState nextState)
        {
            _time += dt;
// Comments are kept here on purpose to reenebale when testing            UnityEngine.Debug.Log("[animator] stand trans " + (_time/(currentState.Duration*_data.Duration)));
            currentState.Update(dt);
            nextState.Update(dt);
        }

        public bool ShouldFinish(ITimeState currentState, ITimeState nextState)
        {
            var factor = 1.0f;
            if(!_data.HasFixedDuration)
            {
                factor = currentState.Duration;
            }
            return _time > _data.Duration*factor;
        }

        public void OnFinish(ITimeState currentState, ITimeState nextState)
        {
        }

        bool MeetsAllConditions
        {
            get
            {
                for(int i = 0; i < _data.Conditions.Length; i++)
                {
                    var condition = _data.Conditions[i];
                    var param = _parameterProvider.GetParameter(condition.Paramater);
                    if(!MeetsCondition(param, condition))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        void ResetTriggerParams()
        {
            for(int i = 0; i < _data.Conditions.Length; i++)
            {
                var condition = _data.Conditions[i];
                var param = _parameterProvider.GetParameter(condition.Paramater);
                if(param.Type == ParameterDataType.Trigger)
                {
                    param.ResetTrigger();
                }
            }
        }

        bool MeetsCondition(ParameterStandalone parameter, ConditionData condition)
        {
            switch(parameter.Type)
            {
            case ParameterDataType.Int:
                return MeetsIntCondition(parameter.IntValue, condition);
            case ParameterDataType.Float:
                return MeetsFloatCondition(parameter.FloatValue, condition);
            case ParameterDataType.Bool:
                return MeetsBoolCondition(parameter.BoolValue, condition);
            case ParameterDataType.Trigger:
                return MeetsBoolCondition(parameter.BoolValue, condition);
            default:
                return false;
            }
        }

        bool MeetsIntCondition(int value, ConditionData condition)
        {
            switch(condition.Type)
            {
            case ConditionDataType.Greater:
                return (value > condition.Threshold);
            case ConditionDataType.Less:
                return (value < condition.Threshold);
            case ConditionDataType.Equals:
                return (value == condition.Threshold);
            case ConditionDataType.NotEqual:
                return (value != condition.Threshold);
            default:
                return false;
            }
        }

        bool MeetsFloatCondition(float value, ConditionData condition)
        {
            switch(condition.Type)
            {
            case ConditionDataType.Greater:
                return (value > condition.Threshold);
            case ConditionDataType.Less:
                return (value < condition.Threshold);
            case ConditionDataType.Equals:
                return (value == condition.Threshold);
            case ConditionDataType.NotEqual:
                return (value != condition.Threshold);
            default:
                return false;
            }
        }

        bool MeetsBoolCondition(bool value, ConditionData condition)
        {
            switch(condition.Type)
            {
            case ConditionDataType.If:
                return (value == true);
            case ConditionDataType.IfNot:
                return (value == false);
            default:
                return false;
            }
        }
    }
}
