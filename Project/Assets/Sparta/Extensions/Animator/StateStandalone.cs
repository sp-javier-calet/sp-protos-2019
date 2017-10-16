using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Animations
{
    public class StateStandalone : ITimeState
    {
        public Action<AnimationEventData> EventTriggered;
        public Action<AnimationEventData> VisualEventTriggered;

        StateData _data;
        AnimatorStandalone _animator;
        float _time;

        float _speed;

        public bool Dirty;

        public float Speed
        {
            get
            {
                var multiplier = 1f;
                if(_data.SpeedParameterActive && !string.IsNullOrEmpty(_data.SpeedParameter))
                {
                    multiplier = _animator.GetFloat(_data.SpeedParameter);
                }
                return _speed * multiplier;

            }
            set
            {
                _speed = value;
            }
        }

        public int Id{ get { return _data.NameHash; } }

        public float Duration
        {
            get
            {
                return _data.AnimationDuration / Speed;
            }
        }

        public float NormalizedTime
        {
            get
            {
                var l = _data.AnimationDuration;
                if(l > 0.0f)
                {
                    return _time / l;
                }
                return 1f;
            }
        }

        public string Name
        {
            get
            {
                return _data.Name;
            }
        }

        public StateStandalone(StateData data, AnimatorStandalone animator)
        {
            _data = data;
            Speed = _data.Speed;
            _animator = animator;
        }

        public void OnStart()
        {
            Dirty = true;
            _time = 0.0f;
        }

        public void OnFinish()
        {
        }

        public void Update(float dt)
        {   
            var oldTime = _time;
            var newTime = _time + dt * Speed;

            if(_data.Loop)
            {
                newTime = Repeat(newTime, _data.AnimationDuration);
                Dirty |= ((int)Math.Floor(oldTime / _data.AnimationDuration)) != ((int)Math.Floor(newTime / _data.AnimationDuration));
            }
            else
            {
                newTime = Math.Min(newTime, _data.AnimationDuration);
            }

            _time = newTime;

            if(oldTime != _time)
            {
                TriggerEvents(oldTime, _time);
            }
        }

        public void TriggerEvents(float fromTime, float toTime)
        {
            if(EventTriggered != null && _data.Motion.Events != null)
            {
                for(var i = 0; i < _data.Motion.Events.Length; i++)
                {
                    var ev = _data.Motion.Events[i];
                    // oldTime > timeRepeat could happen if we finished current animation loop
                    if(ev.Time >= fromTime && ev.Time < toTime ||
                       (fromTime > toTime && (ev.Time >= fromTime || ev.Time < toTime)))
                    {
                        if(ev.IsVisual)
                        {
                            if(VisualEventTriggered != null)
                            {
                                VisualEventTriggered(ev);
                            }
                        }
                        else
                        {
                            if(EventTriggered != null)
                            {
                                EventTriggered(ev);
                            }
                        }
                    }
                }
            }
        }


        float Repeat(float num, float rep)
        {
            var tmp = num / rep;
            var vi = (float)Math.Floor(tmp);
            var vf = tmp - vi;
            return vf * rep;
        }

        public object Clone()
        {
            return new StateStandalone(_data, _animator);
        }

        public override string ToString()
        {
            return string.Format("[StateStandalone: {0} duration={1} ntime={2}]", _data.Name, Duration, NormalizedTime);
        }
    }
}
