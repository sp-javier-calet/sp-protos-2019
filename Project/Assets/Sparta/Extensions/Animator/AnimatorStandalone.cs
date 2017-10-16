using System.Collections.Generic;
using System;

namespace SocialPoint.Animations
{
    public interface IAnimatorParameterProvider
    {
        ParameterStandalone GetParameter(string name);
    }

    public interface IAnimatorBehaviour
    {
        IAnimator Animator{ get; }

        void Play(string animState);
    }

    public interface IAnimator
    {
        event Action<IAnimationEvent> EventTriggered;
        event Action<IAnimationEvent> VisualEventTriggered;
        event Action<IAnimationEvent> AudioEventTriggered;

        float CurrentStateDuration { get; }

        bool IsInitialized{ get; }

        void SetInteger(string name, int value);

        void SetFloat(string name, float value);

        void SetBool(string name, bool value);

        void SetTrigger(string name);

        void ResetTrigger(string name);

        void Play(string name);

        void Update(float dt);

        bool IsName(string name);
    }

    public interface IAnimationEvent
    {
        float Time { get; }

        string StringValue { get; }

        int IntValue { get; }

        float FloatValue { get; }
    }

    public class AnimatorStandalone : IAnimator, IAnimatorParameterProvider
    {
        AnimatorData _data;

        public Dictionary<string, ParameterStandalone> Parameters{ get { return _parameters; } }

        Dictionary<string, ParameterStandalone> _parameters;

        TimeStateMachine[] _stateMachines;

        public TimeStateMachine[] StateMachines{ get { return _stateMachines; } }

        public event Action<IAnimationEvent> EventTriggered;
        public event Action<IAnimationEvent> VisualEventTriggered;
        public event Action<IAnimationEvent> AudioEventTriggered;

        public string CurrentStateName
        {
            get
            {
                if(_stateMachines.Length == 0 || _stateMachines[0].CurrentState == null || string.IsNullOrEmpty(_stateMachines[0].CurrentState.Name))
                {
                    return string.Empty;
                }
                return _stateMachines[0].CurrentState.Name;
            }
        }

        public float CurrentStateDuration
        {
            get
            {
                if(_stateMachines.Length == 0 || _stateMachines[0].CurrentState == null)
                {
                    return 0f;
                }
                return _stateMachines[0].CurrentState.Duration;
            }
        }

        public StateStandalone CurrentState
        {
            get
            {
                if(_stateMachines.Length == 0 || _stateMachines[0].CurrentState == null)
                {
                    return null;
                }
                return (StateStandalone)_stateMachines[0].CurrentState;
            }
        }

        public float CurrentStateNormalizedTime
        {
            get
            {
                if(_stateMachines.Length == 0 || _stateMachines[0].CurrentState == null)
                {
                    return 0f;
                }
                return _stateMachines[0].CurrentState.NormalizedTime;
            }
        }

        public StateStandalone MostRecentState
        {
            get
            {
                var sm = _stateMachines[0];
                return (StateStandalone)(sm.IsInTransition ? sm.NextState : sm.CurrentState);
            }
        }

        public ITimeState FindState(int layerId, string name)
        {
            if(layerId < _stateMachines.Length)
            {
                var stateMachine = _stateMachines[layerId];
                var statesEnum = stateMachine.States.GetEnumerator();
                while(statesEnum.MoveNext())
                {
                    var currentState = statesEnum.Current;
                    if(string.Compare(currentState.Value.Name, name, true) == 0)
                    {
                        return currentState.Value;
                    }
                }
            }
            return null;
        }

        public AnimatorStandalone(AnimatorData data)
        {
            _data = data;
            _parameters = new Dictionary<string, ParameterStandalone>();
            for(int i = 0; i < _data.Parameters.Length; i++)
            {
                var paramData = _data.Parameters[i];
                var param = new ParameterStandalone(paramData);
                _parameters.Add(paramData.Name, param);
            }
            _stateMachines = new TimeStateMachine[_data.Layers.Length];
            for(var i = 0; i < _data.Layers.Length; i++)
            {
                _stateMachines[i] = CreateStateMachine(_data.Layers[i].StateMachine);
            }
        }

        TimeStateMachine CreateStateMachine(StateMachineData data)
        {
            var stateMachine = new TimeStateMachine();
            //States with their transitions
            for(int i = 0; i < data.States.Length; i++)
            {
                var stateData = data.States[i];
                var state = new StateStandalone(stateData, this);
                state.EventTriggered += OnStateEventTriggered;
                state.VisualEventTriggered += OnStateVisualEventTriggered;
                stateMachine.DefineState(stateData.Name, state);
                var transitions = stateData.Transitions;
                for(int j = 0; j < transitions.Length; j++)
                {
                    var transData = transitions[j];
                    var trans = new TransitionStandalone(transData, this);
                    stateMachine.DefineTransition(stateData.Name, trans, transData.ToState);
                }
            }
            //Global transitions
            for(int i = 0; i < data.AnyStateTransitions.Length; i++)
            {
                var transData = data.AnyStateTransitions[i];
                var trans = new TransitionStandalone(transData, this);
                stateMachine.DefineTransition(trans, transData.ToState);
            }
            //Initial State
            stateMachine.ChangeToState(data.DefaultState);

            return stateMachine;
        }

        public bool IsName(string name)
        {
            for(int i = 0; i < _stateMachines.Length; ++i)
            {
                if(_stateMachines[i].CurrentState != null && string.Compare(_stateMachines[i].CurrentState.Name, name, true) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsInitialized
        {
            get
            {
                return true;
            }
        }

        public ParameterStandalone GetParameter(string name)
        {
            return _parameters[name];
        }

        public void Update(float dt)
        {
            for(var i = 0; i < _stateMachines.Length; i++)
            {
                _stateMachines[i].Update(dt);
            }
        }

        void OnStateEventTriggered(AnimationEventData ev)
        {
            if(EventTriggered != null)
            {
                EventTriggered(ev);
            }
        }

        void OnStateVisualEventTriggered(AnimationEventData ev)
        {
            if(VisualEventTriggered != null)
            {
                VisualEventTriggered(ev);
            }
        }

        public void SetInteger(string name, int value)
        {
            if(!_parameters.ContainsKey(name))
            {
                System.Console.WriteLine("not found key: " + name);
                return;
            }
            _parameters[name].IntValue = value;
        }

        public void SetFloat(string name, float value)
        {
            if(!_parameters.ContainsKey(name))
            {
                System.Console.WriteLine("not found key: " + name);
                return;
            }
            _parameters[name].FloatValue = value;
        }

        public void SetBool(string name, bool value)
        {
            if(!_parameters.ContainsKey(name))
            {
                System.Console.WriteLine("not found key: " + name);
                return;
            }
            _parameters[name].BoolValue = value;
        }

        public void SetTrigger(string name)
        {
            if(!_parameters.ContainsKey(name))
            {
                System.Console.WriteLine("not found key: " + name);
                return;
            }
            _parameters[name].BoolValue = true;
        }

        public void ResetTrigger(string name)
        {
            if(!_parameters.ContainsKey(name))
            {
                System.Console.WriteLine("not found key: " + name);
                return;
            }
            _parameters[name].BoolValue = false;
        }

        public float GetFloat(string name)
        {
            if(!_parameters.ContainsKey(name))
            {
                return 0f;
            }
            return _parameters[name].FloatValue;
        }

        public void Play(string name)
        {
            for(var i = 0; i < _stateMachines.Length; i++)
            {
                _stateMachines[i].ChangeToState(name);
            }
        }
    }

    public class CollectionAnimator : IAnimator
    {
        List<IAnimator> _animators = new List<IAnimator>();

        public float CurrentStateDuration
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public event Action<IAnimationEvent> EventTriggered
        {
            add
            {
                throw new NotImplementedException();
            }
            remove
            {
                throw new NotImplementedException();
            }
        }

        public  event Action<IAnimationEvent> VisualEventTriggered
        {
            add
            {
                throw new NotImplementedException();
            }
            remove
            {
                throw new NotImplementedException();
            }
        }

        public event Action<IAnimationEvent> AudioEventTriggered
        {
            add
            {
                throw new NotImplementedException();
            }
            remove
            {
                throw new NotImplementedException();
            }
        }

        void IAnimator.ResetTrigger(string name)
        {
            
        }

        void IAnimator.Update(float dt)
        {
            throw new NotImplementedException();
        }

        bool IAnimator.IsName(string name)
        {
            throw new NotImplementedException();
        }

        bool IAnimator.IsInitialized
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Add(IAnimator animator)
        {
            _animators.Add(animator);
        }

        public void Remove(IAnimator animator)
        {
            _animators.Remove(animator);
        }

        public void SetInteger(string name, int value)
        {
            for(var i = 0; i < _animators.Count; i++)
            {
                _animators[i].SetInteger(name, value);
            }
        }

        public void SetFloat(string name, float value)
        {
            for(var i = 0; i < _animators.Count; i++)
            {
                _animators[i].SetFloat(name, value);
            }
        }

        public void SetBool(string name, bool value)
        {
            for(var i = 0; i < _animators.Count; i++)
            {
                _animators[i].SetBool(name, value);
            }
        }

        public void SetTrigger(string name)
        {
            for(var i = 0; i < _animators.Count; i++)
            {
                _animators[i].SetTrigger(name);
            }
        }

        public void Play(string name)
        {
            for(var i = 0; i < _animators.Count; i++)
            {
                _animators[i].Play(name);
            }
        }
    }
}
