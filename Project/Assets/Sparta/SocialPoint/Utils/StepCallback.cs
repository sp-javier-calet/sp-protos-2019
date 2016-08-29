using System;
using SocialPoint.Base;

namespace SocialPoint.Utils
{
    public sealed class StepCallback
    {
        readonly Action _callback;
        public int Count { get; set; }

        public StepCallback(int count, Action callback)
        {
            DebugUtils.Assert(count > 0);
            DebugUtils.Assert(callback != null);

            _callback = callback;
            Count = count;
        }

        public void DoStep()
        {
            DebugUtils.Assert(Count > 0);
            DebugUtils.Assert(_callback != null);

            if(--Count == 0 && _callback != null)
            {
                _callback();
            }
        }
    }

    public sealed class StepCallbackBuilder
    {
        readonly StepCallback _stepCallback;
        bool _ready;

        public StepCallbackBuilder(Action callback)
        {
            _stepCallback = new StepCallback(1, callback);
        }

        public Action Add()
        {
            DebugUtils.Assert(!_ready);
            if(_ready)
            {
                throw new Exception("Add called after Ready");
            }
            ++_stepCallback.Count;
            return DoStep;
        }

        public void Ready()
        {
            _ready = true;
            DoStep();
        }

        void DoStep()
        {
            _stepCallback.DoStep();
        }
    }
}
