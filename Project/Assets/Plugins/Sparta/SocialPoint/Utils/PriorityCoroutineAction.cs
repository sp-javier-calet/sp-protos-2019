using System;
using System.Collections;
using SocialPoint.Base;

namespace SocialPoint.Utils
{
    public class PriorityCoroutineAction<T> : PriorityQueue<T, Func<IEnumerator>>
    {
        ICoroutineRunner _runner;
        Func<T, IEnumerator> _defaultPriorityAction;

        public PriorityCoroutineAction(ICoroutineRunner runner)
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
            var runData = new CoroutineRunData();
            var itr = queues.GetEnumerator();
            while(itr.MoveNext())
            {
                var kvp = itr.Current;
                var itr2 = kvp.Value.GetEnumerator();
                while(itr2.MoveNext())
                {
                    var action = itr2.Current;
                    if(_defaultPriorityAction != null)
                    {
                        _defaultPriorityAction(kvp.Key);
                    }
                    if(action != null)
                    {
                        _runner.StartCoroutine(RunCoroutine(action, runData));
                    }
                }
                itr2.Dispose();
                while(!runData.Ended)
                    yield return null;
            }
            itr.Dispose();
        }

        static IEnumerator RunCoroutine(Func<IEnumerator> corroutine, CoroutineRunData data)
        {
            data.AddCoroutine();
            yield return corroutine();
            data.RemoveCoroutine();
        }
    }

    class CoroutineRunData
    {
        int _runningCoroutines;
        bool _started;

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

        public bool Ended
        {
            get
            {
                return (_started && _runningCoroutines == 0);
            }
        }
    }

    public sealed class PriorityCoroutineAction : PriorityCoroutineAction<int>
    {
        public PriorityCoroutineAction(ICoroutineRunner runner) : base(runner)
        {
        }

        public PriorityCoroutineAction(PriorityCoroutineAction other) : base(other)
        {
        }
    }
}

