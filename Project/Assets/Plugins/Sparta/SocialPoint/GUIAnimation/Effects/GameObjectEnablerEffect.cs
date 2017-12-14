using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    [System.Serializable]
    public sealed class GameObjectEnablerEffect : TriggerEffect
    {
        public sealed class TargetValueMonitor : StepMonitor
        {
            public bool WasEnabled;

            public override void Backup()
            {
                WasEnabled = Target.gameObject.activeSelf;
            }

            public override bool HasChanged()
            {
                if(Target == null)
                {
                    return false;
                }

                bool originalValue = WasEnabled;
                bool newValue = Target.gameObject.activeSelf;
				
                return newValue != originalValue;
            }
        }

        [SerializeField]
        [ShowInEditor]
        bool _startValue = true;

        public bool StartValue { get { return _startValue; } set { _startValue = value; } }

        [SerializeField]
        [ShowInEditor]
        bool _endValue = true;

        public bool EndValue { get { return _endValue; } set { _endValue = value; } }

        [SerializeField]
        [ShowInEditor]
        bool _disableAfterPlay = true;

        bool _hasBeenPlayed;

        public override void Copy(Step other)
        {
            base.Copy(other);
            CopyActionValues((GameObjectEnablerEffect)other);
        }

        public override void CopyActionValues(Effect other)
        {
            _startValue = ((GameObjectEnablerEffect)other).StartValue;
            _endValue = ((GameObjectEnablerEffect)other).EndValue;
        }

        public override void OnRemoved()
        {
        }

        public override void SetOrCreateDefaultValues()
        {
            _startValue = _endValue = true;
        }

        public override void Invert(bool invertTime = false)
        {
            base.Invert(invertTime);

            bool tempEndValue = _endValue;
            _endValue = _startValue;
            _startValue = tempEndValue;
        }

        public override void DoAction()
        {
            if(Target == null)
            {
                Log.w(GetType() + " OnBlend " + StepName + " Target is null");
                return;
            }

            if(_disableAfterPlay && _hasBeenPlayed)
            {
                return;
            }

            Target.gameObject.SetActive(EndValue);
            _hasBeenPlayed = true;
        }

        public override void OnReset()
        {
            base.OnReset();

            if(!IsEnabledInHierarchy())
            {
                return;
            }

            if(Target == null)
            {
                Log.w(GetType() + " OnBlend " + StepName + " Target is null");
                return;
            }

            if(_disableAfterPlay && _hasBeenPlayed)
            {
                return;
            }

            Target.gameObject.SetActive(StartValue);
        }

        public override StepMonitor CreateTargetMonitor()
        {
            return new TargetValueMonitor();
        }

        public override void SaveValuesAt(float localTimeNormalized)
        {
            Log.w(GetType() + " -> SaveValues. Nothing to save :(");
        }
    }
}
