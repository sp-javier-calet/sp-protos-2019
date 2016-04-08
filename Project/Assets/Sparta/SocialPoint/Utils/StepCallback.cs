using System;
using UnityEngine.Assertions;

namespace SocialPoint.Utils
{
    public class StepCallback
    {
        readonly Action _callback;
        public int Count { get; set; }

        public StepCallback(int count, Action callback)
        {
            Assert.IsTrue(count > 0);
            Assert.IsNotNull(callback);

            _callback = callback;
            Count = count;
        }

        public void DoStep()
        {
            Assert.IsTrue(Count > 0);
            Assert.IsNotNull(_callback);

            if(--Count == 0 && _callback != null)
            {
                _callback();
            }
        }
    }

    public class StepCallbackBuilder
    {
        readonly StepCallback _stepCallback;
        bool _ready;

        public StepCallbackBuilder(Action callback)
        {
            _stepCallback = new StepCallback(1, callback);
        }

        public Action Add()
        {
            Assert.IsTrue(!_ready);
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
