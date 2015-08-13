using System;
using System.Collections.Generic;

namespace SocialPoint.Utils
{
    public interface IState
    {
        void OnStateLoad();

        void OnStateUnload();
        
        void OnStateEnter();
        
        void OnStateExit();
        
        void Update();

        void FixedUpdate();
    }
    
    public class StateMachine<TIdState, TIdTransition, TState> where TIdState : IConvertible where TIdTransition : IConvertible where TState : IState
    {
        private class StateTransition
        {
            public TIdTransition transition;
            public TIdState state;
            
            public StateTransition(TIdTransition t, TIdState s)
            {
                transition = t;
                state = s;
            }
            
            public override string ToString()
            {
                return transition.ToString() + "," + state.ToString();
            }
            
            public bool Equals(StateTransition other)
            {
                return other.transition.Equals(transition) && other.state.Equals(state);
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
                return transition.GetHashCode() ^ state.GetHashCode();
            }
        }
        
        
        private TState currentState;
        
        public TState CurrentState
        {
            get { return currentState; }
        }
        
        public TState GetState(TIdState state)
        {
            return states[state];
        }
        
        protected TIdState currentStateId;
        
        public TIdState CurrentStateId
        {
            get { return currentStateId; }
        }
        
        protected float nextStateDeltaTime = 0.0f;
        protected TState nextState;
        protected TIdState nextStateId;
        private Dictionary<TIdState, TState> states;
        private Dictionary<StateTransition, TIdState> transitionTable;
        private Dictionary<TIdTransition, TIdState> defaultTransitionTable;
        
        public StateMachine()
        {
            states = new Dictionary<TIdState, TState>();
            transitionTable = new Dictionary<StateTransition, TIdState>();
            defaultTransitionTable = new Dictionary<TIdTransition, TIdState>();
        }
        
        public bool AddState(TIdState id, TState state)
        {
            states.Add(id, state);
            return true;
        }
        
        public void Update()
        {
            nextStateDeltaTime -= UnityEngine.Time.deltaTime;
            if(nextState != null && nextStateDeltaTime <= 0.0f)
            {
                TState oldState = currentState;
                currentState = nextState;
                currentStateId = nextStateId;
                nextState = default(TState);
                if(oldState != null)
                {
                    oldState.OnStateExit();
                }
                currentState.OnStateEnter();
            }
            if(currentState != null)
            {
                currentState.Update();
            }
        }

        public void FixedUpdate()
        {
            if(currentState != null)
            {
                currentState.FixedUpdate();
            }
        }
        
        public void SetTransition(TIdTransition trans, TIdState current, TIdState next)
        {
            transitionTable[new StateTransition(trans, current)] = next;
        }
        
        public void SetTransition(TIdTransition trans, TIdState next)
        {
            defaultTransitionTable[trans] = next;
        }
        
        public bool ChangeToState(TIdState id, float deltaTime)
        {
            if(!states.TryGetValue(id, out nextState))
            {
                return false;
            }

            if(currentState != null)
            {
                currentState.OnStateUnload();
            }

            nextStateDeltaTime = deltaTime;
            nextStateId = id;
            nextState.OnStateLoad();
            return true;
        }
        
        public bool ChangeToState(TIdState id)
        {
            return ChangeToState(id, 0.0f);
        }
        
        public bool IsInTransition
        {
            get { return nextState != null; }
        }
        
        public bool ChangeToStateWithTransition(TIdTransition trans, float deltaTime)
        {
            TIdState current = CurrentStateId;
            TIdState next = default(TIdState);
            if(!transitionTable.TryGetValue(new StateTransition(trans, current), out next))
            {
                if(!defaultTransitionTable.TryGetValue(trans, out next))
                {
                    return false;
                }
            }
            return ChangeToState(next, deltaTime);
        }
        
        public bool ChangeToStateWithTransition(TIdTransition trans)
        {
            return ChangeToStateWithTransition(trans, 0.0f);
        }

        public void Dispose()
        {
            states.Clear();
            nextState = default (TState);
            if(currentState != null)
            {
                currentState.OnStateUnload();
                currentState.OnStateExit();
                currentState = default (TState);
            }
        }
    }
    
    public class StateMachine<TIdState, TIdTransition> : StateMachine<TIdState, TIdTransition, IState> where TIdState : IConvertible where TIdTransition : IConvertible
    {
    }
}