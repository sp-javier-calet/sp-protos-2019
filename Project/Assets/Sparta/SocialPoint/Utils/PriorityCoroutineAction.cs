using System;
using System.Collections;
using UnityEngine;
using SocialPoint.Base;

namespace SocialPoint.Utils
{
    public class PriorityCoroutineAction<T> : PriorityQueue<T, Func<IEnumerator>>
    {
        ICoroutineRunner _runner;
        Func<T, IEnumerator> _defaultPriorityAction;

        public PriorityCoroutineAction(ICoroutineRunner runner) : base()
        {
            _runner = runner;
        }

        public PriorityCoroutineAction(PriorityCoroutineAction<T> other) : base(other)
        {
        }

        public override object Clone()
        {
            return new PriorityCoroutineAction<T>(this);
        }

        public void Add(Func<T, IEnumerator> action)
        {
            _defaultPriorityAction += action;
        }

        public void Remove(Func<T, IEnumerator> action)
        {
            _defaultPriorityAction -= action;
        }

        public void Run()
        {
            _runner.StartCoroutine(RunCoroutines());
        }

        IEnumerator RunCoroutines()
        {
            var queues = CopyQueues();
            var runData = new RunData();
            foreach(var kvp in queues)
            {
                foreach(var action in kvp.Value)
                {
                    if(_defaultPriorityAction != null)
                    {
                        _defaultPriorityAction(kvp.Key);
                    }
                    if(action != null)
                    {
                        _runner.StartCoroutine(RunCorroutine(action, runData));
                    }
                }
                while(!runData.Ended)
                    yield return null;
            }
        }

        IEnumerator RunCorroutine(Func<IEnumerator> corroutine, RunData data)
        {
            data.AddCoroutine();
            yield return corroutine();
            data.RemoveCoroutine();
        }
    }

    class RunData
    {
        int _runningCoroutines = 0;
        bool _started = false;

        public void AddCoroutine()
        {
            _runningCoroutines++;
            if(!_started)
                _started = true;
        }

        public void RemoveCoroutine()
        {
            DebugUtils.Assert(_runningCoroutines > 0, "trying to remove more coroutines than added");
            _runningCoroutines--;
        }

        bool _ended;

        public bool Ended
        {
            get
            {
                _ended = (_started && _runningCoroutines == 0);
                return _ended;
            }

            private set
            {
                _ended = value;
            }
        }
    }

    public class PriorityCoroutineAction : PriorityCoroutineAction<int>
    {
        public PriorityCoroutineAction(ICoroutineRunner runner) : base(runner)
        {
        }

        public PriorityCoroutineAction(PriorityCoroutineAction other) : base(other)
        {
        }
    }
}

