using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    // Used to monitor Effect values
    public abstract class StepMonitor
    {
        public Transform Target;

        public void Init(Transform iTarget)
        {
            Target = iTarget;
        }

        public abstract void Backup();

        public abstract bool HasChanged();
    }
}
