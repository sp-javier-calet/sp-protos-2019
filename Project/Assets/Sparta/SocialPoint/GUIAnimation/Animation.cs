using System;
using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    [System.Serializable]
    public sealed class Animation : MonoBehaviour
    {
        public enum State
        {
            Idle,
            Playing
        }

        public enum PlayMode
        {
            Once,
            Loop,
            PingPong
        }

        // First step of the animation, this will contain the rest of steps that makes the animation
        [SerializeField]
        Step _root;

        public Step Root
        {
            get
            {
                if(_root == null)
                {
                    _root = GetComponent<Step>();
                }
                return _root;
            }
        }

        // Flat list of all actions in the animation
        List<Effect> _actions = new List<Effect>();

        [SerializeField]
        bool _hideSerializedObjects = true;

        public bool HideSerializedObjects { get { return _hideSerializedObjects; } }

        [SerializeField]
        bool _playOnStart;

        public bool PlayOnStart { get { return _playOnStart; } set { _playOnStart = value; } }

        [SerializeField]
        PlayMode _playMode = PlayMode.Once;

        public PlayMode Mode { get { return _playMode; } set { _playMode = value; } }

        bool _hasStartPlaying;
        State _state = State.Idle;

        float _playTime;
        float _currentTime;

        public float CurrentTime { get { return _currentTime; } }

        float _prevTime;

        public float PrevTime { get { return _prevTime; } }

        public float DeltaTime { get { return CurrentTime - _prevTime; } }

        [SerializeField]
        bool _enableWarnings;

        public bool EnableWarnings { get { return _enableWarnings; } }

        bool _isInverted;

        public bool IsInverted { get { return _isInverted; } }

        [SerializeField]
        bool _ignoreTimeScale = true;

        public bool IgnoreTimeScale{ get { return _ignoreTimeScale; } set { _ignoreTimeScale = value; } }

        ITimeGetter _editorTimeGetter;

        Action _onEndCallback;

        public Action OnEndCallback { get { return _onEndCallback; } set { _onEndCallback = value; } }

        [SerializeField]
        float _endDelayTime;

        public float EndDelayTime { get { return _endDelayTime; } set { _endDelayTime = Math.Max(value, 0f); } }

        public string AnimationName
        {
            get
            {
                return gameObject.name;
            }
            set
            {
                gameObject.name = value;
                if(_root != null)
                {
                    _root.name = gameObject.name;
                }
            }
        }

        public void PlayBackwards()
        {
            if(!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            RefreshAndInit();
            TryToTriggerStartPlayingMessage();

            Invert();
            DoPlay();
        }

        public void Stop()
        {
            ChangeState(State.Idle);
        }

        public void Play()
        {
            if(!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            RefreshAndInit();
            TryToTriggerStartPlayingMessage();

            DoPlay();
        }


        public void SetEditorTimeGetter(ITimeGetter timeGetter)
        {
            _editorTimeGetter = timeGetter;
        }

        void DoPlay()
        {
            ChangeState(State.Playing);
        }

        void TryToTriggerStartPlayingMessage()
        {
            if(!_hasStartPlaying && Application.isPlaying)
            {
                _hasStartPlaying = true;
                TriggerStartPlayingMessage();
            }
        }

        void TriggerStartPlayingMessage()
        {
            ((Group)Root).OnAnimationStartPlaying();
        }

        public void RefreshAndInit()
        {
            Refresh();
            Init();
        }

        public void Refresh()
        {
            if(Root != null)
            {
                Root.Refresh();
            }
        }

        // Set the animation, parent animationItem to the hierarchy. Save all the actions and order them by starting time
        public void Init()
        {
            _actions.Clear();
            _root.Init(this, null);

            OrderActions();
        }

        public List<Effect> FindEffectsByName(string name)
        {
            RefreshAndInit();

            return _actions.FindAll(a => a.StepName == name);
        }

        public Effect FindEffectByName(string name)
        {
            List<Effect> effects = FindEffectsByName(name);
            return effects.Count > 0 ? effects[0] : null;
        }

        public List<Effect> FindEffectsByType<T>() where T : Effect
        {
            RefreshAndInit();

            return _actions.FindAll(a => a is T);
        }

        public T FindEffectByType<T>() where T : Effect
        {
            List<Effect> effects = FindEffectsByType<T>();
            return effects.Count > 0 ? (T)effects[0] : null;
        }

        void OrderActions()
        {
            _actions.Sort(SortByEndTime);
        }

        public static int SortByEndTime(Step a, Step b)
        {
            float aTime = a.GetEndTime(AnimTimeMode.Global);
            float bTime = b.GetEndTime(AnimTimeMode.Global);

            return aTime < bTime ? -1 : aTime > bTime ? 1 : 0;
        }

        public static int SortByStartTime(Step a, Step b)
        {
            float aTime = a.GetStartTime(AnimTimeMode.Global);
            float bTime = b.GetStartTime(AnimTimeMode.Global);
			
            return aTime < bTime ? -1 : aTime > bTime ? 1 : 0;
        }


        void Start()
        {
            if(_playOnStart)
            {
                Play();
            }
        }

        //Absolute time from zero as the start animation time
        public void PlayAt(float t)
        {
            // Set everything at start point
            ResetEffects();

            // Set current time with a big deltaTime to run every action till the current time
            _currentTime = t;
            _prevTime = 0f;

            for(int i = 0; i < _actions.Count; ++i)
            {
                _actions[i].OnUpdate();
            }
        }

        public void RevertToOriginal(bool invert = true)
        {
            if(invert && _isInverted)
            {
                Invert();
            }

            ResetEffects(invert);

            ChangeState(State.Idle);
        }

        void ResetEffects(bool actions = true)
        {
            RefreshAndInit();

            if(actions)
            {
                for(int i = _actions.Count - 1; i >= 0; --i)
                {
                    _actions[i].OnReset();
                }
            }
        }

        public void Update()
        {
            switch(_state)
            {
            case State.Idle:
                IdleState();
                break;

            case State.Playing:
                PlayingState();
                break;
            }
        }

        void ChangeState(State newState)
        {
            if(newState == State.Playing)
            {
                _playTime = GetTime();
                _currentTime = 0f;
                _prevTime = 0f;

                ResetEffects();
            }
            else if(newState == State.Idle)
            {
            }

            _state = newState;
        }

        void IdleState()
        {
        }

        public bool IsPlaying()
        {
            return _state != State.Idle;
        }

        void PlayingState()
        {
            _currentTime = GetTime() - _playTime;
            for(int i = 0; i < _actions.Count; ++i)
            {
                _actions[i].OnUpdate();
            }
            _prevTime = _currentTime;

            if(Root != null && CurrentTime >= GetEndingTime())
            {
                PlayMode mode = Application.isPlaying ? _playMode : PlayMode.Once;
                switch(mode)
                {
                case PlayMode.Once:
                    ChangeState(State.Idle);
                    break;
                case PlayMode.Loop:
                    DoPlay();
                    break;
                case PlayMode.PingPong:
                    Invert();
                    DoPlay();
                    break;
                }
                TriggerOnEndCallback();
            }
        }

        float GetTime()
        {
            if(Application.isPlaying || _editorTimeGetter == null)
            {
                return IgnoreTimeScale ? Time.unscaledTime : Time.time;
            }
            return _editorTimeGetter.Get();
        }

        void TriggerOnEndCallback()
        {
            if(_onEndCallback != null)
            {
                _onEndCallback();
            }
        }

        public bool HasFinished()
        {
            if(Root == null || _state == State.Idle)
            {
                return true;
            }
            if(_playMode != PlayMode.Once)
            {
                return false;
            }
            return CurrentTime >= GetEndingTime();
        }

        public float GetEndingTime()
        {
            float endTime = 0f;
            if(_actions.Count > 0)
            {
                endTime = _actions[_actions.Count - 1].GetEndTime(AnimTimeMode.Global);
            }

            if(_playMode == PlayMode.Once)
            {
                return endTime + _endDelayTime;
            }

            return endTime;
        }

        public void Invert()
        {
            if(Root != null)
            {
                _isInverted = !_isInverted;
                Root.Invert();
            }
        }

        public void AddAction(Effect action)
        {
            _actions.Add(action);
        }

        public T SetRootAnimationItem<T>() where T:Step
        {
            _root = gameObject.AddComponent<T>();
            RefreshAndInit();

            return (T)_root;
        }

        public void Copy(Animation other)
        {
            AnimationName = other.AnimationName;

            _playOnStart = other._playOnStart;
            _playMode = other._playMode;

            Root.Copy(other.Root);
        }
    }
}
