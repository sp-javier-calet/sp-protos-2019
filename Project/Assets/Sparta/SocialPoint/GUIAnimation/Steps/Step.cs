using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    [System.Serializable]
    public abstract class Step : MonoBehaviour, IStep
    {
        [SerializeField]
        float _startTime;

        public float StartTime { get { return _startTime; } }

        [SerializeField]
        float _endTime = 1f;

        public float EndTime { get { return _endTime; } }

        [SerializeField]
        int _slot;

        public int Slot { get { return _slot; } }

        [SerializeField]
        protected string _stepName = "Step";

        public virtual string StepName { get { return _stepName; } set { _stepName = value; } }

        [SerializeField]
        bool _isEnabled = true;

        public bool IsEnabled { get { return _isEnabled; } set { _isEnabled = value; } }

        [SerializeField]
        Color _editorColor = Color.white;

        public Color EditorColor { get { return _editorColor; } set { _editorColor = value; } }

        protected Animation _animation;

        public Animation Animation { get { return _animation; } set { _animation = value; } }

        protected Step _parent;

        public Step Parent { get { return _parent; } set { _parent = value; } }

        public abstract void Refresh();

        public abstract void OnRemoved();

        public abstract void SaveValuesAt(float localTimeNormalized);

        public virtual void SaveValues()
        {
            SaveValuesAt(0f);
            SaveValuesAt(1f);
        }

        public void ScaleTime(float scale)
        {
            _startTime *= scale;
            _endTime *= scale;
        }

        public virtual void OnCreated()
        {
            _editorColor = new Color(1f, 1f, 1f, 1f);
        }

        public virtual void Copy(Step other)
        {
            _startTime = other.StartTime;
            _endTime = other.EndTime;

            _slot = other.Slot;
            _stepName = other._stepName;
            _isEnabled = other.IsEnabled;
            _animation = other.Animation;
            _editorColor = other.EditorColor;
        }

        public virtual void Invert(bool invertTime = false)
        {
            if(_parent == null || !invertTime)
            {
                return;
            }

            float duration = GetDuration(AnimTimeMode.Local);
            float newStartTime = 1f - GetEndTime(AnimTimeMode.Local);
            float newEndTime = newStartTime + duration;

            _startTime = newStartTime;
            _endTime = newEndTime;
        }

        public virtual void Init(Animation animation, Step parent)
        {
            _animation = animation;
            _parent = parent;
        }

        public float NormalizedToAbsoluteTime(float iTime)
        {
            float localTime = _startTime + (_endTime - _startTime) * iTime;

            return _parent != null ? _parent.NormalizedToAbsoluteTime(localTime) : localTime;
        }

        public void SetSlot(int timelineIdx)
        {
            _slot = timelineIdx;
        }

        public float AbsoluteToNormalizedTime(float iAbsoluteTime)
        {
            if(_parent != null)
            {
                float parentStartTime = _parent.GetStartTime(AnimTimeMode.Global);
                float parentEndTime = _parent.GetEndTime(AnimTimeMode.Global);

                return (iAbsoluteTime - parentStartTime) / (parentEndTime - parentStartTime);
            }
            return iAbsoluteTime;
        }

        public void SetDuration(float duration, AnimTimeMode mode)
        {
            if(_parent == null)
            {
                _endTime = _startTime + duration;
            }
            else
            {
                if(mode == AnimTimeMode.Local)
                {
                    _endTime = _startTime + duration;
                }
                else
                {
                    float parentStartTime = _parent.GetStartTime(AnimTimeMode.Global);
                    float parentEndTime = _parent.GetEndTime(AnimTimeMode.Global);
                    float durationNormalized = duration / (parentEndTime - parentStartTime);
                    _endTime = _startTime + durationNormalized;
                }
            }
        }

        public virtual float GetDuration(AnimTimeMode mode)
        {
            if(mode == AnimTimeMode.Global)
            {
                return GetEndTime(AnimTimeMode.Global) - GetStartTime(AnimTimeMode.Global);
            }
            return GetEndTime(AnimTimeMode.Local) - GetStartTime(AnimTimeMode.Local);
        }

        public void SetStartTime(float time, AnimTimeMode mode)
        {
            _startTime = mode == AnimTimeMode.Global ? AbsoluteToNormalizedTime(time) : time;
        }

        public void SetEndTime(float time, AnimTimeMode mode)
        {
            _endTime = mode == AnimTimeMode.Global ? AbsoluteToNormalizedTime(time) : time;
        }

        public float GetStartTime(AnimTimeMode mode)
        {
            if(mode == AnimTimeMode.Global && _parent != null)
            {
                return _parent.NormalizedToAbsoluteTime(_startTime);
            }
            return _startTime;
        }

        public float GetEndTime(AnimTimeMode mode)
        {
            if(mode == AnimTimeMode.Global && _parent != null)
            {
                return _parent.NormalizedToAbsoluteTime(_endTime);
            }
            return _endTime;
        }

        public bool IsEnabledInHierarchy()
        {
            bool isEnabledInH = _isEnabled;
            if(_isEnabled && _parent != null)
            {
                isEnabledInH = _parent.IsEnabledInHierarchy();
            }

            return isEnabledInH;
        }
    }
}
