
namespace SocialPoint.GUIAnimation
{
    public abstract class TriggerEffect : Effect
    {
        bool _wasRun;

        public override void OnReset()
        {
            _wasRun = false;
        }

        public override void OnUpdate()
        {
            if(_wasRun)
            {
                return;
            }

            if(IsEnabledInHierarchy())
            {
                float actionStartTime = GetStartTime(AnimTimeMode.Global);
				
                float t = _animation.CurrentTime;
				
                if(t >= actionStartTime)
                {
                    DoAction();

                    _wasRun = true;
                }
            }
        }

        public abstract void DoAction();

        public virtual float GetFixedDuration()
        {
            return 0.25f;
        }
    }
}
