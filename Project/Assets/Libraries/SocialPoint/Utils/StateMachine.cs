using System;
using System.Collections.Generic;

namespace SocialPoint.Utils
{
    public interface IState : ICloneable
    {
        void OnStateLoad();

        void OnStateUnload();
        
        void OnStateEnter();
        
        void OnStateExit();
        
        void Update();

        void FixedUpdate();
    }
    
    public class StateMachine<Transition, StateType, State> : IDisposable where State : IState
    {
        private class StateTransition
        {
            public Transition Transition;
            public StateType StateType;
            
            public StateTransition(Transition t, StateType s)
            {
                Transition = t;
                StateType = s;
            }
            
            public override string ToString()
            {
                return Transition.ToString() + "," + StateType.ToString();
            }
            
            public bool Equals(StateTransition other)
            {
                return other.Transition.Equals(Transition) && other.StateType.Equals(StateType);
            }
            
            public override bool Equals(object other)
            {
                var obj = other as StateTransition;
                
                if(obj == null)
                {
                    return false;
                }
                
                return this.Equals(obj);
            }
            
            public override int GetHashCode()
            {
                return Transition.GetHashCode() ^ StateType.GetHashCode();
            }
        }

        private float _nextStateDeltaTime = 0.0f;
        private StateType _nextStateType;
        private State _nextState;

        private Dictionary<StateTransition, StateType> _stateTransitions = new Dictionary<StateTransition, StateType>();
        private Dictionary<Transition, StateType> _transitions = new Dictionary<Transition, StateType>();
        private Dictionary<StateType, State> _states = new Dictionary<StateType, State>();

        private StateType _currentStateType;
        public StateType CurrentStateType
        {
            get
            {
                return _currentStateType;
            }
        }

        private State _currentState;
        public State CurrentState
        {
            get
            {
                return _currentState;
            }
        }

        
        public bool IsInTransition
        {
            get
            {
                return _nextState != null;
            }
        }        

        public StateMachine()
        {
        }
        
        public void Update()
        {
            _nextStateDeltaTime -= UnityEngine.Time.deltaTime;
            if(_nextState != null && _nextStateDeltaTime <= 0.0f)
            {
                var oldState = _currentState;
                _currentState = _nextState;
                _currentStateType = _nextStateType;
                _nextState = default(State);
                if(oldState != null)
                {
                    oldState.OnStateExit();
                }
                _currentState.OnStateEnter();
            }
            if(_currentState != null)
            {
                _currentState.Update();
            }
        }

        public void FixedUpdate()
        {
            if(_currentState != null)
            {
                _currentState.FixedUpdate();
            }
        }

        public void DefineState(StateType type, State state)
        {
            _states[type] = state;
        }
        
        public void DefineTransition(StateType from, Transition trans, StateType to)
        {
            _stateTransitions[new StateTransition(trans, from)] = to;
        }
        
        public void DefineTransition(Transition trans, StateType to)
        {
            _transitions[trans] = to;
        }

        public bool ChangeToState(StateType type, float deltaTime=0.0f)
        {
            State proto;
            if(_states.TryGetValue(type, out proto))
            {
                return ChangeToState(type, (State)proto.Clone(), deltaTime);
            }
            return false;
        }

        public bool ChangeToState(StateType type, State state, float deltaTime=0.0f)
        {
            if(_currentState != null)
            {
                _currentState.OnStateUnload();
            }
            _nextStateDeltaTime = deltaTime;
            _nextState = state;
            _nextStateType = type;
            _nextState.OnStateLoad();
            return true;
        }
        
        public bool ChangeToStateWithTransition(Transition trans, float deltaTime=0.0f)
        {
            StateType type;
            if(!_stateTransitions.TryGetValue(new StateTransition(trans, _currentStateType), out type))
            {
                if(!_transitions.TryGetValue(trans, out type))
                {
                    return false;
                }
            }
            return ChangeToState(type, deltaTime);
        }

        public void Dispose()
        {
            _nextState = default(State);
            if(_currentState != null)
            {
                _currentState.OnStateUnload();
                _currentState.OnStateExit();
                _currentState = default(State);
            }
        }
    }
    
    public class StateMachine<Transition, State> : StateMachine<Transition, Type, State> where State : IState
    {
        public void DefineState<S>(State state) where S : State
        {
            DefineState(typeof(S), state);
        }

        public void DefineState(State state)
        {
            DefineState(state.GetType(), state);
        }
        
        public void DefineTransition<F,T>(Transition trans) where F : State where T : State
        {
            DefineTransition(typeof(F), trans, typeof(T));
        }
        
        public void DefineTransition<T>(Transition trans) where T : State
        {
            DefineTransition(trans, typeof(T));
        }
        
        public bool ChangeToState<S>(float deltaTime=0.0f) where S : State
        {
            return ChangeToState(typeof(S), deltaTime);
        }
        
        public bool ChangeToState<S>(State state, float deltaTime=0.0f) where S : State
        {
            return ChangeToState(typeof(S), state, deltaTime);
        }
    }
        
    public class StateMachine<Transition> : StateMachine<Transition, IState>
    {
    }
}