using System;

namespace SocialPoint.Utils
{
    public sealed class ScheduledAction : IUpdateable, IDisposable
    {
        Action _action;
        IUpdateScheduler _scheduler;
        bool _started;

        public ScheduledAction(IUpdateScheduler scheduler, Action action)
        {
            _scheduler = scheduler;
            _action = action;
        }

        public void Start(float interval = 0)
        {
            if(_started)
            {
                Stop();
            }

            _started = true;
            if(interval <= 0)
            {
                _scheduler.Add(this);
            }
            else
            {
                _scheduler.Add(this, interval);
            }
        }

        public void Update()
        {
            _action();
        }

        public void Stop()
        {
            _started = false;
            _scheduler.Remove(this);
        }

        public void Dispose()
        {
            if(_started)
            {
                Stop();
            }
            _scheduler = null;
            _action = null;
        }
    }
}
