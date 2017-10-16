using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SocialPoint.Utils;

namespace SocialPoint.Animations
{
    public interface ITimeTransition
    {
        /*
         * add the possible interrupting transitions from sources into transitions
         * the transitions list is checked in order calling ShouldStart on each one
         */
        void UpdateInterruptions(List<TimeTransitionPair> transitions, TimeTransitionInterruptionSources sources);

        /*
         * return true if checking for interrupting transitions should stop
         * when the current transition is found in the queue
         */
        bool StopInterruptionOnCurrent{ get; }

        /*
         * check if this transition can start
         */
        bool ShouldStart(ITimeState currentState);

        /*
         * called when transition starts
         */
        void OnStart(ITimeState currentState, ITimeState nextState);

        /*
         * called during transition on every update
         * should update the states if needed
         */
        void Update(float dt, ITimeState currentState, ITimeState nextState);

        /*
         * check if this transition should finish
         */
        bool ShouldFinish(ITimeState currentState, ITimeState nextState);

        /*
         * called when transition finish
         */
        void OnFinish(ITimeState currentState, ITimeState nextState);
    }

    public struct TimeTransitionPair
    {
        public ITimeTransition Transition;
        public string ToState;
    }

    public interface ITimeState : IDeltaUpdateable
    {
        float Speed{ get; set; }

        int Id { get; }

        string Name { get; }

        float Duration { get; }

        float NormalizedTime { get; }

        void OnStart();

        void OnFinish();
    }

    public struct TimeTransitionInterruptionSources
    {
        public ReadOnlyCollection<TimeTransitionPair> Any;
        public ReadOnlyCollection<TimeTransitionPair> Source;
        public ReadOnlyCollection<TimeTransitionPair> Destination;
    }

    public class TimeStateMachine
    {
        ITimeState _currentState;
        string _currentStateId;
        ITimeState _nextState;
        string _nextStateId;
        ITimeTransition _transition;

        List<TimeTransitionPair> _anyTransitions = new List<TimeTransitionPair>();
        Dictionary<string, List<TimeTransitionPair>> _stateTransitions = new Dictionary<string, List<TimeTransitionPair>>();
        List<TimeTransitionPair> _interruptingTransitions = new List<TimeTransitionPair>();
        Dictionary<string, ITimeState> _states = new Dictionary<string, ITimeState>();

        public Dictionary<string, ITimeState> States{ get { return _states; } }

        public Action<string> OnStateChanged;

        public ITimeState CurrentState
        {
            get
            {
                return _currentState;
            }
        }

        public ITimeState NextState
        {
            get
            {
                return _nextState;
            }
        }

        public bool IsInTransition
        {
            get
            {
                return _nextState != null;
            }
        }

        public void Update(float dt)
        {
            while(UpdateTransition())
            {
                // will iterate while transitions are started or ended
            }

            // update either transition or current state
            if(_currentState != null)
            {
                if(_transition != null && _nextState != null)
                {
                    _transition.Update(dt, _currentState, _nextState);
                }
                else
                {
                    _currentState.Update(dt);
                }
            }

            while(UpdateTransition())
            {
                // will iterate while transitions are started or ended
            }
        }

        bool UpdateTransition()
        {
            // start a possible transition
            if(_currentState != null)
            {
                for(var i = 0; i < _interruptingTransitions.Count; i++)
                {
                    var t = _interruptingTransitions[i];
                    if(t.Transition == _transition)
                    {
                        if(_transition != null && _transition.StopInterruptionOnCurrent)
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if(t.Transition.ShouldStart(_currentState))
                    {
                        if(ChangeToState(t.ToState, t.Transition))
                        {
                            return true;
                        }
                    }
                }
            }

            // update current transition
            bool finishTransition = false;
            if(_nextState != null)
            {
                if(_transition == null)
                {
                    finishTransition = true;
                }
                else if(_transition.ShouldFinish(_currentState, _nextState))
                {
                    finishTransition = true;
                }
            }

            // finish current transition
            if(finishTransition)
            {
                // finish current transition
                var oldState = _currentState;
                var oldTransition = _transition;
                _currentState = _nextState;
                _currentStateId = _nextStateId;
                _nextState = null;
                _nextStateId = null;
                _transition = null;

                if(oldTransition != null)
                {
                    oldTransition.OnFinish(oldState, _currentState);
                }
                if(oldState != null)
                {
                    oldState.OnFinish();
                }
                UpdateInterruptingTransitions();
            }

            return finishTransition;
        }

        public void DefineState(string id, ITimeState state)
        {
            _states.Add(id, state);
        }

        public void DefineTransition(string fromState, ITimeTransition trans, string toState)
        {
            List<TimeTransitionPair> outTransitions;
            if(!_stateTransitions.TryGetValue(fromState, out outTransitions))
            {
                outTransitions = new List<TimeTransitionPair>();
                _stateTransitions.Add(fromState, outTransitions);
            }
            outTransitions.Add(new TimeTransitionPair {
                Transition = trans,
                ToState = toState
            });
        }

        public void DefineTransition(ITimeTransition trans, string toState)
        {
            _anyTransitions.Add(new TimeTransitionPair {
                Transition = trans,
                ToState = toState
            });
        }

        bool ChangeToState(string stateId, ITimeTransition trans)
        {
            ITimeState state;
            if(_states.TryGetValue(stateId, out state))
            {
                _nextState = state;
                _nextStateId = stateId;
                _transition = trans;
                _nextState.OnStart();
                if(_transition != null)
                {
                    _transition.OnStart(_currentState, _nextState);
                }
                UpdateInterruptingTransitions();
                if(OnStateChanged != null)
                {
                    OnStateChanged(stateId);
                }
                return true;
            }
            return false;
        }

        public bool ChangeToState(string stateId)
        {
            return ChangeToState(stateId, null);
        }

        void UpdateInterruptingTransitions()
        {
            _interruptingTransitions.Clear();
            if(_transition != null)
            {
                List<TimeTransitionPair> current = null;
                if(_currentStateId != null)
                {
                    _stateTransitions.TryGetValue(_currentStateId, out current);
                }
                List<TimeTransitionPair> next = null;
                if(_nextStateId != null)
                {
                    _stateTransitions.TryGetValue(_nextStateId, out next);
                }
                _transition.UpdateInterruptions(_interruptingTransitions, new TimeTransitionInterruptionSources {
                    Any = _anyTransitions.AsReadOnly(),
                    Source = current == null ? null : current.AsReadOnly(),
                    Destination = next == null ? null : next.AsReadOnly()
                });
            }
            else
            {
                _interruptingTransitions.AddRange(_anyTransitions);
                List<TimeTransitionPair> transitions;
                if(_currentStateId != null && _stateTransitions.TryGetValue(_currentStateId, out transitions))
                {
                    _interruptingTransitions.AddRange(transitions);
                }
            }
        }
    }
}
