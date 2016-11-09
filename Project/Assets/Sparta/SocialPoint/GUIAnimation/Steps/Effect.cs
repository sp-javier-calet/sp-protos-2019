using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    [System.Serializable]
    public abstract class Effect : Step
    {
        [SerializeField]
        Transform _target;

        public Transform Target { get { return _target; } set { _target = value; } }

        public override void Init(Animation animation, Step parent)
        {
            base.Init(animation, parent);

            _animation.AddAction(this);
        }

        public override void Refresh()
        {
        }

        public override void Copy(Step other)
        {
            base.Copy(other);

            _target = ((Effect)other).Target;
        }

        // Copy another effect
        public abstract void CopyActionValues(Effect other);

        // Copy the values that are shared between different targets of the same effect
        public virtual void CopySharedValues(Effect other)
        {
        }

        public virtual StepMonitor CreateTargetMonitor()
        {
            return null;
        }

        public abstract void OnUpdate();

        public abstract void SetOrCreateDefaultValues();

        public abstract void OnReset();
    }
}
